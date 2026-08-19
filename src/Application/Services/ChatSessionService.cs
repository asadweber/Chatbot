using Application.Dtos;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories;

namespace Application.Services;

/// <summary>
/// <see cref="IChatSessionService"/> implementation: session CRUD via
/// <see cref="IChatSessionRepository"/>, and the RAG chat turn combining
/// <see cref="IRetrievalService"/> (context lookup) with
/// <see cref="IChatService"/> (LLM completion). Entities are mapped to DTOs
/// at the boundary so callers never see <c>Domain.Entities</c> types.
/// </summary>
public class ChatSessionService(
    IChatSessionRepository sessions,
    IRetrievalService retrieval,
    IChatService chat,
    IMapper mapper) : IChatSessionService
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<ChatSessionDto>> GetSessionsAsync(CancellationToken ct = default)
    {
        var result = await sessions.GetAllAsync(ct);
        return mapper.Map<List<ChatSessionDto>>(result);
    }

    /// <inheritdoc />
    public async Task<ChatSessionDto?> GetSessionWithMessagesAsync(int sessionId, CancellationToken ct = default)
    {
        var session = await sessions.GetByIdWithMessagesAsync(sessionId, ct);
        return session is null ? null : mapper.Map<ChatSessionDto>(session);
    }

    /// <inheritdoc />
    public async Task<ChatSessionDto> CreateSessionAsync(CancellationToken ct = default)
    {
        var session = await sessions.AddAsync(ct);
        return mapper.Map<ChatSessionDto>(session);
    }

    /// <inheritdoc />
    public async Task<string?> RenameSessionAsync(int sessionId, string title, CancellationToken ct = default)
    {
        var session = await sessions.GetByIdWithMessagesAsync(sessionId, ct);
        if (session is null) return null;

        var trimmed = title.Trim();
        session.Title = trimmed.Length > 60 ? trimmed[..60] : trimmed;
        await sessions.SaveChangesAsync(ct);

        return session.Title;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteSessionAsync(int sessionId, CancellationToken ct = default)
    {
        var session = await sessions.GetByIdWithMessagesAsync(sessionId, ct);
        if (session is null) return false;

        await sessions.RemoveAsync(session, ct);
        return true;
    }

    /// <inheritdoc />
    public async Task<string?> SendMessageAsync(int sessionId, string message, CancellationToken ct = default)
    {
        var session = await sessions.GetByIdWithMessagesAsync(sessionId, ct);
        if (session is null) return null;

        session.Messages.Add(new ChatMessage { SessionId = session.Id, Role = "user", Content = message });

        // First message in the session: derive a display title from it so
        // the sidebar shows something more useful than "New chat".
        if (session.Messages.Count == 1)
            session.Title = message.Length > 60 ? message[..60] + "..." : message;

        await sessions.SaveChangesAsync(ct);

        // Conversation so far, including the user message just added above.
        var history = session.Messages.Select(m => (m.Role, m.Content)).ToList();

        // Retrieval-augmented generation: look up document chunks relevant
        // to the user's message to ground the assistant's answer.
        var contextChunks = await retrieval.RetrieveRelevantChunksAsync(message, ct: ct);

        var reply = await chat.GetResponseAsync(message, history, contextChunks, ct);

        session.Messages.Add(new ChatMessage { SessionId = session.Id, Role = "assistant", Content = reply });
        await sessions.SaveChangesAsync(ct);

        return reply;
    }
}
