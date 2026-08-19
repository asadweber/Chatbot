using Application.Interfaces;
using Domain;
using Domain.Repositories;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;


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
        var connectionString = configuration.GetConnectionString("PostgresDefaultConnection")
            ?? throw new InvalidOperationException("Missing connection string 'PostgresDefaultConnection'.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql => npgsql.UseVector()));

        var ollamaSection = configuration.GetSection("Ollama");
        var ollamaEndpoint = new Uri(ollamaSection["Endpoint"] ?? "http://localhost:11434");
        var chatModel = ollamaSection["ChatModel"] ?? "llama3.1:8b";
        var embeddingModel = ollamaSection["EmbeddingModel"] ?? "nomic-embed-text:v1.5";

        services.AddKernel()
            .AddOllamaChatCompletion(chatModel, ollamaEndpoint)
            .AddOllamaTextEmbeddingGeneration(embeddingModel, ollamaEndpoint);

        services.AddScoped<IEmbeddingService, OllamaEmbeddingService>();
        services.AddScoped<IChatService, OllamaChatService>();
        services.AddScoped<IDocumentIngestionService, DocumentIngestionService>();
        services.AddScoped<IRetrievalService, RetrievalService>();

        return services;
    }
}
