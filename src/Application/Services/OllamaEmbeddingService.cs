using Application.Interfaces;
using Microsoft.SemanticKernel.Embeddings;
using Pgvector;

namespace Application.Services;

/// <summary>
/// <see cref="IOllamaEmbeddingService"/> implementation backed by a Semantic Kernel
/// <see cref="ITextEmbeddingGenerationService"/> connected to a local Ollama
/// embedding model. Converts the generated float vector into a pgvector
/// <see cref="Vector"/> for storage/querying via Npgsql.
/// </summary>
public class OllamaEmbeddingService : IOllamaEmbeddingService
{
    private readonly ITextEmbeddingGenerationService _generator;

    public OllamaEmbeddingService(ITextEmbeddingGenerationService generator)
    {
        _generator = generator;
    }

    /// <inheritdoc />
    public async Task<Vector> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        var embedding = await _generator.GenerateEmbeddingAsync(text, cancellationToken: ct);
        return new Vector(embedding);
    }
}
