using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// <see cref="IChatSessionRepository"/> implementation backed by
/// <see cref="VectorDbContext"/>.
/// </summary>
public class ChatSessionRepository(VectorDbContext context) : IChatSessionRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<ChatSession>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.ChatSessions
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<ChatSession?> GetByIdWithMessagesAsync(int sessionId, CancellationToken ct = default)
    {
        return await context.ChatSessions
            .Include(s => s.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct);
    }

    /// <inheritdoc />
    public async Task<ChatSession> AddAsync(CancellationToken ct = default)
    {
        var session = new ChatSession();
        context.ChatSessions.Add(session);
        await context.SaveChangesAsync(ct);
        return session;
    }

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);

    /// <inheritdoc />
    public async Task RemoveAsync(ChatSession session, CancellationToken ct = default)
    {
        context.ChatSessions.Remove(session);
        await context.SaveChangesAsync(ct);
    }
}
