using Application.Interfaces;
using Domain.Repositories;

namespace Application.Services;

/// <summary>
/// <see cref="IRetrievalService"/> implementation that embeds the query and
/// delegates the nearest-neighbor chunk search to <see cref="IDocumentRepository"/>.
/// </summary>
public class RetrievalService : IRetrievalService
{
    private readonly IDocumentRepository _documents;
    private readonly IEmbeddingService _embeddings;

    public RetrievalService(IDocumentRepository documents, IEmbeddingService embeddings)
    {
        _documents = documents;
        _embeddings = embeddings;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> RetrieveRelevantChunksAsync(string query, int topK = 4, CancellationToken ct = default)
    {
        var queryEmbedding = await _embeddings.EmbedAsync(query, ct);
        return await _documents.SearchSimilarChunksAsync(queryEmbedding, topK, ct);
    }
}
