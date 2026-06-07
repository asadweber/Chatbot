using Chatbot.Data;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;

namespace Chatbot.Services;

public class RetrievalService : IRetrievalService
{
    private readonly ApplicationDbContext _db;
    private readonly IEmbeddingService _embeddings;

    public RetrievalService(ApplicationDbContext db, IEmbeddingService embeddings)
    {
        _db = db;
        _embeddings = embeddings;
    }

    public async Task<IReadOnlyList<string>> RetrieveRelevantChunksAsync(string query, int topK = 4, CancellationToken ct = default)
    {
        var queryEmbedding = await _embeddings.EmbedAsync(query, ct);

        return await _db.DocumentChunks
            .OrderBy(c => c.Embedding!.CosineDistance(queryEmbedding))
            .Take(topK)
            .Select(c => c.Content)
            .ToListAsync(ct);
    }
}
