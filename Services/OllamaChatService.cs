using Microsoft.SemanticKernel.ChatCompletion;

namespace Chatbot.Services;

public class OllamaChatService : IChatService
{
    private readonly IChatCompletionService _chat;

    private const string SystemPromptTemplate =
        "You are a helpful assistant that answers questions using the provided context. " +
        "If the context does not contain relevant information, say so and answer from general knowledge, " +
        "making clear no relevant documents were found.\n\nContext:\n{0}";

    public OllamaChatService(IChatCompletionService chat)
    {
        _chat = chat;
    }

    public async Task<string> GetResponseAsync(
        string question,
        IReadOnlyList<(string Role, string Content)> history,
        IReadOnlyList<string> contextChunks,
        CancellationToken ct = default)
    {
        var contextText = contextChunks.Count > 0
            ? string.Join("\n---\n", contextChunks)
            : "(no relevant documents found)";

        var chatHistory = new ChatHistory(string.Format(SystemPromptTemplate, contextText));

        foreach (var (role, content) in history)
        {
            if (role == "user")
                chatHistory.AddUserMessage(content);
            else
                chatHistory.AddAssistantMessage(content);
        }

        chatHistory.AddUserMessage(question);

        var result = await _chat.GetChatMessageContentAsync(chatHistory, cancellationToken: ct);
        return result.Content ?? string.Empty;
    }
}
