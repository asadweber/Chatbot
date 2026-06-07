using Chatbot.Data;
using Chatbot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Missing connection string 'DefaultConnection'.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql => npgsql.UseVector()));

var ollamaSection = builder.Configuration.GetSection("Ollama");
var ollamaEndpoint = new Uri(ollamaSection["Endpoint"] ?? "http://localhost:11434");
var chatModel = ollamaSection["ChatModel"] ?? "llama3.1:8b";
var embeddingModel = ollamaSection["EmbeddingModel"] ?? "nomic-embed-text:v1.5";

builder.Services.AddKernel()
    .AddOllamaChatCompletion(chatModel, ollamaEndpoint)
    .AddOllamaTextEmbeddingGeneration(embeddingModel, ollamaEndpoint);

builder.Services.AddScoped<IEmbeddingService, OllamaEmbeddingService>();
builder.Services.AddScoped<IChatService, OllamaChatService>();
builder.Services.AddScoped<IDocumentIngestionService, DocumentIngestionService>();
builder.Services.AddScoped<IRetrievalService, RetrievalService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
