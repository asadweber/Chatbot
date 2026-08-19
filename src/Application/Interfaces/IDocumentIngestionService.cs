using Application.Dtos;

namespace Application.Interfaces;

/// <summary>
/// Ingests an uploaded document: extracts its text, splits it into chunks,
/// generates embeddings for each chunk, and persists everything for later
/// retrieval.
/// </summary>
public interface IDocumentIngestionService
{
    /// <summary>
    /// Extracts text from <paramref name="content"/>, chunks it, embeds each
    /// chunk, and stores the resulting <c>Document</c>/<c>DocumentChunk</c> records.
    /// </summary>
    /// <param name="fileName">Original file name (used to detect format, e.g. ".pdf").</param>
    /// <param name="content">The file's raw byte stream.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The newly created document, as a <see cref="DocumentDto"/>.</returns>
    Task<DocumentDto> IngestionAsync(string fileName, Stream content, CancellationToken ct = default);
}
