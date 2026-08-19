using Chatbot.Data;
using Chatbot.Models;
using Chatbot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chatbot.Controllers;

/// <summary>
/// Drives the chat UI and conversation lifecycle: listing/creating/renaming/
/// deleting sessions, and handling message sends — which combine retrieval
/// (RAG context lookup) with LLM chat completion and persist both sides of
/// the exchange.
/// </summary>
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

    /// <summary>
    /// Main chat page: lists all sessions (newest first) and, if
    /// <paramref name="sessionId"/> is given, loads that session with its
    /// messages for display in the main pane.
    /// </summary>
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

    /// <summary>Creates an empty chat session and redirects to it.</summary>
    [HttpPost]
    public async Task<IActionResult> NewSession()
    {
        var session = new ChatSession();
        _db.ChatSessions.Add(session);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { sessionId = session.Id });
    }

    /// <summary>
    /// Renames a session (trimmed, capped at 60 characters). Returns 400 for
    /// an empty title and 404 if the session doesn't exist.
    /// </summary>
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

    /// <summary>Deletes a session and (via cascade delete) its messages.</summary>
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

    /// <summary>
    /// Core RAG chat turn: persists the user's message (auto-titling new
    /// sessions from it), retrieves relevant document chunks for grounding,
    /// requests an assistant reply informed by both the conversation history
    /// and that retrieved context, then persists the assistant's reply too.
    /// </summary>
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

        // First message in the session: derive a display title from it so
        // the sidebar shows something more useful than "New chat".
        if (session.Messages.Count == 0)
            session.Title = request.Message.Length > 60 ? request.Message[..60] + "..." : request.Message;

        await _db.SaveChangesAsync(ct);

        // Conversation so far — session.Messages is a tracked, loaded
        // collection so it already includes the user message just added
        // above. Passed to the chat service for full conversational context.
        var history = session.Messages.Select(m => (m.Role, m.Content)).ToList();


        // Retrieval-augmented generation: look up document chunks relevant
        // to the user's message to ground the assistant's answer.
        var contextChunks = await _retrieval.RetrieveRelevantChunksAsync(request.Message, ct: ct);


        var reply = await _chat.GetResponseAsync(request.Message, history, contextChunks, ct);

        _db.ChatMessages.Add(new ChatMessage { SessionId = session.Id, Role = "assistant", Content = reply });
        await _db.SaveChangesAsync(ct);

        return Json(new SendMessageResponse { Reply = reply });
    }
}
