namespace Chatbot.Models;

/// <summary>
/// A source document uploaded for ingestion (e.g. PDF, TXT, Markdown), split
/// into one or more <see cref="DocumentChunk"/> records for retrieval.
/// </summary>
public class Document
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>Original file name as uploaded by the user.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>UTC timestamp at which the document was uploaded/ingested.</summary>
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    /// <summary>The chunks this document was split into during ingestion.</summary>
    public List<DocumentChunk> Chunks { get; set; } = new();
}
