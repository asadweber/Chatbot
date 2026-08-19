using Domain.Entities;

namespace Domain.Repositories;

/// <summary>
/// Persistence for ingested documents and their chunks, plus similarity
/// search over chunk embeddings used for retrieval-augmented generation.
/// </summary>
public interface IDocumentRepository
{
    /// <summary>
    /// Persists <paramref name="document"/> and its already-embedded
    /// <paramref name="chunks"/> (each chunk's <c>DocumentId</c> is set to
    /// the document's DB-generated id before saving).
    /// </summary>
    Task AddDocumentWithChunksAsync(Document document, IReadOnlyList<DocumentChunk> chunks, CancellationToken ct = default);

    /// <summary>
    /// Returns all documents (with their chunks loaded), newest first.
    /// </summary>
    Task<IReadOnlyList<Document>> GetAllWithChunksAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns the content of the <paramref name="topK"/> chunks closest to
    /// <paramref name="queryEmbedding"/> by cosine distance, most relevant
    /// first.
    /// </summary>
    Task<IReadOnlyList<string>> SearchSimilarChunksAsync(Pgvector.Vector queryEmbedding, int topK, CancellationToken ct = default);
}
