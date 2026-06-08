using Chatbot.Data;
using Chatbot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;

// ============================================================================
// Application entry point and composition root.
//
// Wires up the ASP.NET Core MVC pipeline, the PostgreSQL/pgvector data store
// (via EntityFrameworkCore + Npgsql), and the Semantic Kernel / Ollama
// integration that powers the chat and embedding (RAG) features.
// ============================================================================

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// --- Database (PostgreSQL with pgvector extension) -------------------------
// The connection string must be present in configuration (appsettings.json,
// environment variables, user secrets, etc.). Failing fast here avoids a
// confusing runtime error later when the DbContext is first resolved.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Missing connection string 'DefaultConnection'.");

// Register the EF Core DbContext, enabling Npgsql's vector support so that
// embedding columns (pgvector) can be mapped and queried (e.g. similarity
// search for retrieval-augmented generation).
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql => npgsql.UseVector()));

// --- Ollama / Semantic Kernel configuration ---------------------------------
// Read model and endpoint settings from the "Ollama" configuration section,
// falling back to sensible local-development defaults when not specified.
var ollamaSection = builder.Configuration.GetSection("Ollama");
var ollamaEndpoint = new Uri(ollamaSection["Endpoint"] ?? "http://localhost:11434");
var chatModel = ollamaSection["ChatModel"] ?? "llama3.1:8b";
var embeddingModel = ollamaSection["EmbeddingModel"] ?? "nomic-embed-text:v1.5";

// Register a Semantic Kernel instance with both a chat completion connector
// and a text embedding generation connector, both backed by the local Ollama
// server. The kernel is the central object the chat and embedding services
// use to talk to the underlying LLM.
builder.Services.AddKernel()
    .AddOllamaChatCompletion(chatModel, ollamaEndpoint)
    .AddOllamaTextEmbeddingGeneration(embeddingModel, ollamaEndpoint);

// --- Application services ----------------------------------------------------
// All scoped (one instance per HTTP request), matching the lifetime of the
// EF Core DbContext they depend on.
builder.Services.AddScoped<IEmbeddingService, OllamaEmbeddingService>();           // Generates vector embeddings via Ollama.
builder.Services.AddScoped<IChatService, OllamaChatService>();                     // Handles chat completions via Ollama.
builder.Services.AddScoped<IDocumentIngestionService, DocumentIngestionService>(); // Ingests documents: chunking, embedding, persisting.
builder.Services.AddScoped<IRetrievalService, RetrievalService>();                 // Similarity search/retrieval over stored embeddings.

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // Non-development: route unhandled exceptions to a friendly error page
    // and enforce HTTPS via HSTS.
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

// Serves static assets (wwwroot) via the new ASP.NET Core static asset pipeline.
app.MapStaticAssets();

// Default MVC route: /{controller}/{action}/{id?}, defaulting to Home/Index.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Chat}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
