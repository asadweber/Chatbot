namespace Chatbot.Services;

/// <summary>
/// Retrieves the document chunks most relevant to a query, for use as
/// grounding context in retrieval-augmented chat responses.
/// </summary>
public interface IRetrievalService
{
    /// <summary>
    /// Embeds <paramref name="query"/> and returns the content of the
    /// <paramref name="topK"/> closest document chunks by vector similarity.
    /// </summary>
    /// <param name="query">The text to find relevant chunks for (typically the user's question).</param>
    /// <param name="topK">Maximum number of chunks to return.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The matching chunks' text content, ordered by relevance (most relevant first).</returns>
    Task<IReadOnlyList<string>> RetrieveRelevantChunksAsync(string query, int topK = 4, CancellationToken ct = default);
}
