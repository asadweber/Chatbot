using Chatbot.Data;
using Chatbot.Models;
using Chatbot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chatbot.Controllers;

public class ChatController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IRetrievalService _retrieval;
    private readonly IChatService _chat;

    public ChatController(ApplicationDbContext db, IRetrievalService retrieval, IChatService chat)
    {
        _db = db;
        _retrieval = retrieval;
        _chat = chat;
    }

    public async Task<IActionResult> Index(int? sessionId)
    {
        var sessions = await _db.ChatSessions.OrderByDescending(s => s.CreatedAt).ToListAsync();

        ChatSession? active = null;
        if (sessionId.HasValue)
        {
            active = await _db.ChatSessions
                .Include(s => s.Messages.OrderBy(m => m.CreatedAt))
                .FirstOrDefaultAsync(s => s.Id == sessionId.Value);
        }

        return View(new ChatPageViewModel { Sessions = sessions, ActiveSession = active });
    }

    [HttpPost]
    public async Task<IActionResult> NewSession()
    {
        var session = new ChatSession();
        _db.ChatSessions.Add(session);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { sessionId = session.Id });
    }

    [HttpPost]
    public async Task<IActionResult> RenameSession([FromBody] RenameSessionRequest request, CancellationToken ct)
    {
        var title = request.Title.Trim();
        if (string.IsNullOrWhiteSpace(title))
            return BadRequest("Title cannot be empty.");

        var session = await _db.ChatSessions.FirstOrDefaultAsync(s => s.Id == request.SessionId, ct);
        if (session is null)
            return NotFound("Chat session not found.");

        session.Title = title.Length > 60 ? title[..60] : title;
        await _db.SaveChangesAsync(ct);

        return Json(new { title = session.Title });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteSession([FromBody] DeleteSessionRequest request, CancellationToken ct)
    {
        var session = await _db.ChatSessions.FirstOrDefaultAsync(s => s.Id == request.SessionId, ct);
        if (session is null)
            return NotFound("Chat session not found.");

        _db.ChatSessions.Remove(session);
        await _db.SaveChangesAsync(ct);

        return Json(new { deleted = true });
    }

    [HttpPost]
    public async Task<IActionResult> Send([FromBody] SendMessageRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message cannot be empty.");

        var session = await _db.ChatSessions
            .Include(s => s.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, ct);

        if (session is null)
            return NotFound("Chat session not found.");

        var userMessage = new ChatMessage { SessionId = session.Id, Role = "user", Content = request.Message };
        _db.ChatMessages.Add(userMessage);

        if (session.Messages.Count == 0)
            session.Title = request.Message.Length > 60 ? request.Message[..60] + "..." : request.Message;

        await _db.SaveChangesAsync(ct);

        var history = session.Messages.Select(m => (m.Role, m.Content)).ToList();
        
        
        var contextChunks = await _retrieval.RetrieveRelevantChunksAsync(request.Message, ct: ct);
        
        
        var reply = await _chat.GetResponseAsync(request.Message, history, contextChunks, ct);

        _db.ChatMessages.Add(new ChatMessage { SessionId = session.Id, Role = "assistant", Content = reply });
        await _db.SaveChangesAsync(ct);

        return Json(new SendMessageResponse { Reply = reply });
    }
}
