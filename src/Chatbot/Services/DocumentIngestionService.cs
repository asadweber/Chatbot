using Chatbot.Data;
using Chatbot.Models;
using System.Text;
using UglyToad.PdfPig;

namespace Chatbot.Services;

/// <summary>
/// <see cref="IDocumentIngestionService"/> implementation: extracts text from
/// uploaded files (PDF via PdfPig, plain text otherwise), splits it into
/// overlapping chunks using a recursive separator-based strategy, embeds each
/// chunk, and persists the <see cref="Document"/>/<see cref="DocumentChunk"/>
/// graph via EF Core.
/// </summary>
public class DocumentIngestionService : IDocumentIngestionService
{
    // NOTE: unused by the active chunker (ChunkRecursive uses its own
    // defaults: maxSize 500, overlap 50). These were the parameters for the
    // simpler fixed-size chunker (see commented-out ChunkText below); kept
    // here in case that strategy is revived.
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

    /// <inheritdoc />
    public async Task<int> IngestionAsync(string fileName, Stream content, CancellationToken ct = default)
    {
        var text = ExtractText(fileName, content);
        var chunks = ChunkRecursive(text);

        // Persist the Document first so its DB-generated Id is available
        // when constructing the chunk rows below.
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

    /// <summary>
    /// Extracts plain text from the uploaded file. PDFs are parsed page by
    /// page via PdfPig and concatenated; any other extension is read as
    /// plain text (covers .txt, .md, etc.).
    /// </summary>
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

    // Earlier fixed-size chunking strategy (slide a ChunkSize-char window with
    // ChunkOverlap chars of overlap, ignoring text structure). Superseded by
    // the separator-aware ChunkRecursive below, which prefers paragraph/
    // sentence/word boundaries for more semantically coherent chunks. Left
    // here for reference in case the simpler strategy is needed again.
    //private static List<string> ChunkText(string text)
    //{
    //    var chunks = new List<string>();
    //    var normalized = text.Replace("\r\n", "\n").Trim();
    //    if (normalized.Length == 0) return chunks;

    //    var start = 0;
    //    while (start < normalized.Length)
    //    {
    //        var length = Math.Min(ChunkSize, normalized.Length - start);
    //        chunks.Add(normalized.Substring(start, length));

    //        if (start + length >= normalized.Length) break;
    //        start += ChunkSize - ChunkOverlap;
    //    }

    //    return chunks;
    //}

    /// <summary>
    /// Splits <paramref name="text"/> into chunks of roughly
    /// <paramref name="maxSize"/> characters, preferring to break on
    /// paragraph, line, sentence, then word boundaries (in that order) so
    /// chunks remain semantically coherent. Carries
    /// <paramref name="overlap"/> characters of trailing context from each
    /// chunk into the next, to preserve context across boundaries.
    /// </summary>
    public List<string> ChunkRecursive(string text, int maxSize = 500, int overlap = 50)
    {
        // Priority order of separators to try: blank line, single newline,
        // sentence end, word boundary, then "" (no separator — last resort
        // for unbroken runs of text longer than maxSize).
        var separators = new[] { "\n\n", "\n", ". ", " ", "" };
        return SplitRecursive(text, separators, maxSize, overlap);
    }

    /// <summary>
    /// Splits on the first separator (from <paramref name="separators"/>,
    /// in priority order) that actually occurs in the text, then greedily
    /// packs the resulting pieces into chunks no larger than
    /// <paramref name="maxSize"/>, carrying <paramref name="overlap"/>
    /// trailing characters of each chunk into the next.
    /// </summary>
    /// <remarks>
    /// Despite the name, this does not recurse with the next separator for
    /// over-long pieces — a piece longer than <paramref name="maxSize"/> is
    /// appended whole, so individual output chunks can still exceed
    /// <paramref name="maxSize"/> for long unbroken runs of text.
    /// </remarks>
    private List<string> SplitRecursive(string text, string[] separators, int maxSize, int overlap)
    {
        var chunks = new List<string>();
        if (text.Length <= maxSize)
        {
            chunks.Add(text);
            return chunks;
        }

        // "" means none of the real separators occur in the text — treat the
        // whole text as a single piece to pack below.
        var separator = separators.FirstOrDefault(s => text.Contains(s)) ?? "";
        var splits = separator == ""
            ? new[] { text }
            : text.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);

        var currentChunk = new StringBuilder();
        foreach (var split in splits)
        {
            if (currentChunk.Length + split.Length > maxSize)
            {
                if (currentChunk.Length > 0)
                    chunks.Add(currentChunk.ToString().Trim());

                // Overlap: carry last N chars into next chunk
                var overlapText = currentChunk.ToString();
                currentChunk.Clear();
                if (overlap > 0 && overlapText.Length > overlap)
                    currentChunk.Append(overlapText[^overlap..] + " ");
            }
            currentChunk.Append(split + separator);
        }

        if (currentChunk.Length > 0)
            chunks.Add(currentChunk.ToString().Trim());

        return chunks;
    }
}
