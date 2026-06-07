using Chatbot.Data;
using Chatbot.Models;
using UglyToad.PdfPig;

namespace Chatbot.Services;

public class DocumentIngestionService : IDocumentIngestionService
{
    private const int ChunkSize = 1000;
    private const int ChunkOverlap = 200;

    private readonly ApplicationDbContext _db;
    private readonly IEmbeddingService _embeddings;
    private readonly ILogger<DocumentIngestionService> _logger;

    public DocumentIngestionService(ApplicationDbContext db, IEmbeddingService embeddings, ILogger<DocumentIngestionService> logger)
    {
        _db = db;
        _embeddings = embeddings;
        _logger = logger;
    }

    public async Task<int> IngestAsync(string fileName, Stream content, CancellationToken ct = default)
    {
        var text = ExtractText(fileName, content);
        var chunks = ChunkText(text);

        var document = new Document { FileName = fileName };
        _db.Documents.Add(document);
        await _db.SaveChangesAsync(ct);

        foreach (var chunkText in chunks)
        {
            var embedding = await _embeddings.EmbedAsync(chunkText, ct);
            _db.DocumentChunks.Add(new DocumentChunk
            {
                DocumentId = document.Id,
                Content = chunkText,
                Embedding = embedding
            });
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Ingested {FileName} as document {DocumentId} with {ChunkCount} chunks", fileName, document.Id, chunks.Count);
        return document.Id;
    }

    private static string ExtractText(string fileName, Stream content)
    {
        if (fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            using var pdf = PdfDocument.Open(content);
            var sb = new System.Text.StringBuilder();
            foreach (var page in pdf.GetPages())
            {
                sb.AppendLine(page.Text);
            }
            return sb.ToString();
        }

        using var reader = new StreamReader(content);
        return reader.ReadToEnd();
    }

    private static List<string> ChunkText(string text)
    {
        var chunks = new List<string>();
        var normalized = text.Replace("\r\n", "\n").Trim();
        if (normalized.Length == 0) return chunks;

        var start = 0;
        while (start < normalized.Length)
        {
            var length = Math.Min(ChunkSize, normalized.Length - start);
            chunks.Add(normalized.Substring(start, length));

            if (start + length >= normalized.Length) break;
            start += ChunkSize - ChunkOverlap;
        }

        return chunks;
    }
}
