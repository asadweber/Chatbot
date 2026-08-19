using Domain.Entities;

namespace Domain.Repositories;

/// <summary>
/// Persistence for chat sessions and their messages.
/// </summary>
public interface IChatSessionRepository
{
    /// <summary>All sessions, newest first (messages not loaded).</summary>
    Task<IReadOnlyList<ChatSession>> GetAllAsync(CancellationToken ct = default);

    /// <summary>A single session with its messages loaded in chronological order, or null if not found.</summary>
    Task<ChatSession?> GetByIdWithMessagesAsync(int sessionId, CancellationToken ct = default);

    /// <summary>Creates a new, empty session and persists it.</summary>
    Task<ChatSession> AddAsync(CancellationToken ct = default);

    /// <summary>Persists changes made to a tracked session (e.g. title, added messages).</summary>
    Task SaveChangesAsync(CancellationToken ct = default);

    /// <summary>Deletes a session (and, via cascade delete, its messages).</summary>
    Task RemoveAsync(ChatSession session, CancellationToken ct = default);
}
