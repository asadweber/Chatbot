using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Chatbot.Controllers;

/// <summary>
/// Drives the chat UI and conversation lifecycle: listing/creating/renaming/
/// deleting sessions, and handling message sends — which combine retrieval
/// (RAG context lookup) with LLM chat completion and persist both sides of
/// the exchange. All persistence/orchestration is delegated to
/// <see cref="IChatSessionService"/>.
/// </summary>
public class ChatController : Controller
{
    private readonly IChatSessionService _chatSessions;

    public ChatController(IChatSessionService chatSessions)
    {
        _chatSessions = chatSessions;
    }

    /// <summary>
    /// Main chat page: lists all sessions (newest first) and, if
    /// <paramref name="sessionId"/> is given, loads that session with its
    /// messages for display in the main pane.
    /// </summary>
    public async Task<IActionResult> Index(int? sessionId, CancellationToken ct)
    {
        var sessions = await _chatSessions.GetSessionsAsync(ct);

        var active = sessionId.HasValue
            ? await _chatSessions.GetSessionWithMessagesAsync(sessionId.Value, ct)
            : null;

        return View(new ChatPageViewModel { Sessions = sessions.ToList(), ActiveSession = active });
    }

    /// <summary>Creates an empty chat session and redirects to it.</summary>
    [HttpPost]
    public async Task<IActionResult> NewSession(CancellationToken ct)
    {
        var session = await _chatSessions.CreateSessionAsync(ct);
        return RedirectToAction(nameof(Index), new { sessionId = session.Id });
    }

    /// <summary>
    /// Renames a session (trimmed, capped at 60 characters). Returns 400 for
    /// an empty title and 404 if the session doesn't exist.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> RenameSession([FromBody] RenameSessionRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest("Title cannot be empty.");

        var title = await _chatSessions.RenameSessionAsync(request.SessionId, request.Title, ct);
        if (title is null)
            return NotFound("Chat session not found.");

        return Json(new { title });
    }

    /// <summary>Deletes a session and (via cascade delete) its messages.</summary>
    [HttpPost]
    public async Task<IActionResult> DeleteSession([FromBody] DeleteSessionRequest request, CancellationToken ct)
    {
        var deleted = await _chatSessions.DeleteSessionAsync(request.SessionId, ct);
        if (!deleted)
            return NotFound("Chat session not found.");

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

        var reply = await _chatSessions.SendMessageAsync(request.SessionId, request.Message, ct);
        if (reply is null)
            return NotFound("Chat session not found.");

        return Json(new SendMessageResponse { Reply = reply });
    }
}
