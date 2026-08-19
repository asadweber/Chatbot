using Chatbot.Models;
using Microsoft.EntityFrameworkCore;

namespace Chatbot.Data;

/// <summary>
/// EF Core database context for the chatbot: documents/chunks (with pgvector
/// embeddings) used for retrieval-augmented generation, plus chat sessions
/// and messages. Backed by PostgreSQL with the <c>vector</c> extension via
/// Npgsql + Pgvector.EntityFrameworkCore.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    /// <summary>Uploaded source documents.</summary>
    public DbSet<Document> Documents => Set<Document>();

    /// <summary>Text chunks extracted from documents, with their embeddings.</summary>
    public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();

    /// <summary>Chat conversations.</summary>
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();

    /// <summary>Individual messages within chat conversations.</summary>
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    /// <summary>
    /// Configures the pgvector extension and entity relationships: chunk
    /// embeddings as <c>vector(768)</c> columns, and cascade-delete
    /// relationships so removing a document/session removes its
    /// chunks/messages.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Required for pgvector column types (e.g. vector(768)) and operators
        // (e.g. cosine distance "<=>") to be available in PostgreSQL.
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<DocumentChunk>(entity =>
        {
            // 768 = embedding dimension produced by the configured Ollama
            // embedding model (nomic-embed-text:v1.5 by default).
            entity.Property(c => c.Embedding).HasColumnType("vector(768)");
            entity.HasOne(c => c.Document)
                  .WithMany(d => d.Chunks)
                  .HasForeignKey(c => c.DocumentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasOne(m => m.Session)
                  .WithMany(s => s.Messages)
                  .HasForeignKey(m => m.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
