using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

/// <summary>
/// EF Core database context for the chatbot: documents/chunks (with pgvector
/// embeddings) used for retrieval-augmented generation, plus chat sessions
/// and messages. Backed by PostgreSQL with the <c>vector</c> extension via
/// Npgsql + Pgvector.EntityFrameworkCore.
/// </summary>
public class VectorDbContext : DbContext
{
    public VectorDbContext(DbContextOptions<VectorDbContext> options) : base(options) { }

   
    /// <summary>Semantic (row-level) documents for orders, with their embeddings.</summary>
    public DbSet<OrderDocument> OrderDocuments => Set<OrderDocument>();

   
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

       
        modelBuilder.Entity<OrderDocument>(entity =>
        {
            // 768 = embedding dimension produced by the configured Ollama
            // embedding model (nomic-embed-text:v1.5 by default).
            entity.Property(d => d.Embedding).HasColumnType("vector(768)");
            entity.HasIndex(d => d.OrderId).IsUnique();
        });

       
    }
}
