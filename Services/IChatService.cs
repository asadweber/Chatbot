namespace Chatbot.Services;

public interface IChatService
{
    Task<string> GetResponseAsync(
        string question,
        IReadOnlyList<(string Role, string Content)> history,
        IReadOnlyList<string> contextChunks,
        CancellationToken ct = default);
}
