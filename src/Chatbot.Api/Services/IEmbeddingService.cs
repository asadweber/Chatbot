using Pgvector;

namespace Chatbot.Services;

/// <summary>
/// Generates vector embeddings for text, used both when ingesting document
/// chunks and when embedding a query for similarity search.
/// </summary>
public interface IEmbeddingService
{
    /// <summary>Computes a vector embedding for <paramref name="text"/>.</summary>
    /// <param name="text">The text to embed.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The embedding vector (dimension matches the configured embedding model, e.g. 768).</returns>
    Task<Vector> EmbedAsync(string text, CancellationToken ct = default);
}
