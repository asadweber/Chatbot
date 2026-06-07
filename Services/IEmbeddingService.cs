using Pgvector;

namespace Chatbot.Services;

public interface IEmbeddingService
{
    Task<Vector> EmbedAsync(string text, CancellationToken ct = default);
}
