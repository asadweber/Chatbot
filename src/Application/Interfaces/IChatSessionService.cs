using Application.Dtos;

namespace Application.Interfaces;

/// <summary>
/// Manages chat session lifecycle (list/create/rename/delete) and drives the
/// core RAG chat turn (persist user message, retrieve context, get an
/// assistant reply, persist that reply too).
/// </summary>
public interface IChatSessionService
{
    /// <summary>All sessions, newest first (messages not loaded).</summary>
    Task<IReadOnlyList<ChatSessionDto>> GetSessionsAsync(CancellationToken ct = default);

    /// <summary>A single session with its messages loaded, or null if not found.</summary>
    Task<ChatSessionDto?> GetSessionWithMessagesAsync(int sessionId, CancellationToken ct = default);

    /// <summary>Creates a new, empty session.</summary>
    Task<ChatSessionDto> CreateSessionAsync(CancellationToken ct = default);

    /// <summary>
    /// Renames a session (title truncated to 60 chars). Returns the updated
    /// title, or null if the session doesn't exist.
    /// </summary>
    Task<string?> RenameSessionAsync(int sessionId, string title, CancellationToken ct = default);

    /// <summary>Deletes a session (and its messages). Returns false if it didn't exist.</summary>
    Task<bool> DeleteSessionAsync(int sessionId, CancellationToken ct = default);

    /// <summary>
    /// Persists the user's message (auto-titling new sessions from it),
    /// retrieves relevant document chunks for grounding, requests an
    /// assistant reply, and persists that reply too. Returns the reply text,
    /// or null if the session doesn't exist.
    /// </summary>
    Task<string?> SendMessageAsync(int sessionId, string message, CancellationToken ct = default);
}
