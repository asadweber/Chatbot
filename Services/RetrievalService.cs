using Chatbot.Data;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;

namespace Chatbot.Services;

/// <summary>
/// <see cref="IRetrievalService"/> implementation that embeds the query and
/// performs a pgvector cosine-distance nearest-neighbor search over stored
/// <c>DocumentChunk</c> embeddings via EF Core + Npgsql.
/// </summary>
public class RetrievalService : IRetrievalService
{
    private readonly ApplicationDbContext _db;
    private readonly IEmbeddingService _embeddings;

    public RetrievalService(ApplicationDbContext db, IEmbeddingService embeddings)
    {
        _db = db;
        _embeddings = embeddings;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> RetrieveRelevantChunksAsync(string query, int topK = 4, CancellationToken ct = default)
    {
        var queryEmbedding = await _embeddings.EmbedAsync(query, ct);

        // CosineDistance is translated by Pgvector.EntityFrameworkCore into a
        // pgvector "<=>" SQL operator, letting Postgres do the nearest-
        // neighbor ranking (and use an index) instead of pulling all rows.
        return await _db.DocumentChunks
            .OrderBy(c => c.Embedding!.CosineDistance(queryEmbedding))
            .Take(topK)
            .Select(c => c.Content)
            .ToListAsync(ct);
    }
}
