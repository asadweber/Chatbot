namespace Chatbot.Services;

public interface IRetrievalService
{
    Task<IReadOnlyList<string>> RetrieveRelevantChunksAsync(string query, int topK = 4, CancellationToken ct = default);
}
