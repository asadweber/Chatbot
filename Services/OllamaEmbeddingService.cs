using Microsoft.SemanticKernel.Embeddings;
using Pgvector;

namespace Chatbot.Services;

public class OllamaEmbeddingService : IEmbeddingService
{
    private readonly ITextEmbeddingGenerationService _generator;

    public OllamaEmbeddingService(ITextEmbeddingGenerationService generator)
    {
        _generator = generator;
    }

    public async Task<Vector> EmbedAsync(string text, CancellationToken ct = default)
    {
        var embedding = await _generator.GenerateEmbeddingAsync(text, cancellationToken: ct);
        return new Vector(embedding);
    }
}
