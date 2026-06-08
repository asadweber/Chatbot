namespace Chatbot.Services;

/// <summary>
/// Generates assistant chat responses, grounding the LLM in retrieved
/// document context (retrieval-augmented generation).
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Produces an assistant reply to <paramref name="question"/>, given the
    /// prior conversation and retrieved context chunks to ground the answer.
    /// </summary>
    /// <param name="question">The user's current question/message.</param>
    /// <param name="history">Prior turns in the conversation as (role, content) pairs, in chronological order.</param>
    /// <param name="contextChunks">Relevant document excerpts retrieved for this question, used as grounding context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The assistant's generated reply text.</returns>
    Task<string> GetResponseAsync(
        string question,
        IReadOnlyList<(string Role, string Content)> history,
        IReadOnlyList<string> contextChunks,
        CancellationToken ct = default);
}
