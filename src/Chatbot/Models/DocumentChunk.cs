using Pgvector;

namespace Chatbot.Models;

/// <summary>
/// A chunk of text extracted from a source <see cref="Document"/>, together
/// with its vector embedding used for similarity search (RAG retrieval).
/// </summary>
public class DocumentChunk
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>Foreign key to the owning <see cref="Document"/>.</summary>
    public int DocumentId { get; set; }

    /// <summary>Navigation property to the owning document.</summary>
    public Document? Document { get; set; }

    /// <summary>The chunk's raw text content.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Vector embedding of <see cref="Content"/> (pgvector column,
    /// dimension 768 — see <c>ApplicationDbContext.OnModelCreating</c>),
    /// used for cosine-distance similarity search during retrieval.
    /// </summary>
    public Vector? Embedding { get; set; }
}
