using Domain;
using Domain.Repositories;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Pgvector.EntityFrameworkCore;


namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>((provider, options) =>
           options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                  .UseApplicationServiceProvider(provider));



        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();

        // --- Chatbot / RAG (PostgreSQL + pgvector) ------------------------------
        // DbContext + Semantic Kernel wiring stays here (Infrastructure owns data
        // access/external clients); the service implementations that consume them
        // are registered by Application.AddApplication to avoid a circular
        // project reference (those services live in Application.Services).
        var connectionString = configuration.GetConnectionString("PostgresDefaultConnection")
            ?? throw new InvalidOperationException("Missing connection string 'PostgresDefaultConnection'.");

        services.AddDbContext<VectorDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql => npgsql.UseVector()));

        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IChatSessionRepository, ChatSessionRepository>();

        var ollamaSection = configuration.GetSection("Ollama");
        var ollamaEndpoint = new Uri(ollamaSection["Endpoint"] ?? "http://localhost:11434");
        var chatModel = ollamaSection["ChatModel"] ?? "llama3.1:8b";
        var embeddingModel = ollamaSection["EmbeddingModel"] ?? "nomic-embed-text:v1.5";

        services.AddKernel()
            .AddOllamaChatCompletion(chatModel, ollamaEndpoint)
            .AddOllamaTextEmbeddingGeneration(embeddingModel, ollamaEndpoint);

        return services;
    }
}
