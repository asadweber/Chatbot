namespace Chatbot.Services;

public interface IDocumentIngestionService
{
    Task<int> IngestionAsync(string fileName, Stream content, CancellationToken ct = default);
}
