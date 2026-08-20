using Application.Interfaces;
using Application.Mappings;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICustomerService, CustomerService>();

        // Row-level semantic RAG for orders (see docs/plan): renders each order
        // to text, embeds it, and stores it alongside the existing document
        // RAG data in the pgvector-backed VectorDbContext.
        services.AddScoped<IOrderDocumentTextBuilder, OrderDocumentTextBuilder>();
        services.AddScoped<IOrderIngestionService, OrderIngestionService>();
        services.AddScoped<IOrderSemanticSearchService, OrderSemanticSearchService>();

        // Chatbot / RAG services. Their DbContext + Semantic Kernel dependencies
        // are registered by Infrastructure.AddInfrastructure (called before this
        // in the composition root); the implementations live here rather than in
        // Infrastructure to avoid Infrastructure needing a ProjectReference back
        // to Application.
        services.AddScoped<IEmbeddingService, EmbeddingService>();

        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(MapperProfile).Assembly));

        return services;
    }
}
