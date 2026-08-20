using Application.Dtos;

namespace Application.Interfaces;

/// <summary>Result of a single Support Desk turn: the LLM's answer plus the orders it was grounded on.</summary>
public record SupportChatResult(string Answer, IReadOnlyList<OrderDto> RelatedOrders);

/// <summary>
/// Internal Support Desk chat: answers natural-language staff questions about
/// orders, grounded in the order semantic-search index
/// (<see cref="IOrderSemanticSearchService"/>). Stateless per call — no
/// conversation persistence.
/// </summary>
public interface IOrderSupportChatService
{
    /// <summary>
    /// Answers <paramref name="question"/> using the top matching orders as
    /// context, optionally continuing a prior exchange via <paramref name="history"/>.
    /// </summary>
    Task<SupportChatResult> AskAsync(
        string question,
        IReadOnlyList<(string Role, string Content)> history,
        CancellationToken ct = default);
}
