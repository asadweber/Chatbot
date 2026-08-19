using Microsoft.SemanticKernel.ChatCompletion;

namespace Chatbot.Services;

/// <summary>
/// <see cref="IChatService"/> implementation backed by a Semantic Kernel
/// <see cref="IChatCompletionService"/> connected to a local Ollama model.
/// Builds a grounded prompt from retrieved context chunks and conversation
/// history, then requests a completion.
/// </summary>
public class OllamaChatService : IChatService
{
    private readonly IChatCompletionService _chat;

    /// <summary>
    /// System prompt template. <c>{0}</c> is replaced with the retrieved
    /// context text (or a placeholder when no chunks were found), instructing
    /// the model to ground its answer in that context and fall back to
    /// general knowledge — while disclosing that fallback — when it can't.
    /// </summary>
    private const string SystemPromptTemplate =
        "You are a helpful assistant that answers questions using the provided context. " +
        "If the context does not contain relevant information, say so and answer from general knowledge, " +
        "making clear no relevant documents were found.\n\nContext:\n{0}";

    public OllamaChatService(IChatCompletionService chat)
    {
        _chat = chat;
    }

    /// <inheritdoc />
    public async Task<string> GetResponseAsync(
        string question,
        IReadOnlyList<(string Role, string Content)> history,
        IReadOnlyList<string> contextChunks,
        CancellationToken ct = default)
    {
        // Join retrieved chunks with a separator so the model can tell them
        // apart; fall back to an explicit "nothing found" marker so the
        // system prompt's grounding instructions still make sense.
        var contextText = contextChunks.Count > 0
            ? string.Join("\n---\n", contextChunks)
            : "(no relevant documents found)";

        var chatHistory = new ChatHistory(string.Format(SystemPromptTemplate, contextText));

        // Replay prior turns so the model has conversational context beyond
        // just the current question.
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
