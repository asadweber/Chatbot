namespace Chatbot.Services;

public interface IDocumentIngestionService
{
    Task<int> IngestAsync(string fileName, Stream content, CancellationToken ct = default);
}
