using System.Text;
using System.Text.RegularExpressions;
using Application.Dtos;
using Application.Interfaces;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Application.Services;

/// <inheritdoc cref="IOrderSupportChatService" />
public partial class OrderSupportChatService(
    IOrderSemanticSearchService orderSearch,
    IOrderService orderService,
    IChatCompletionService chatCompletion) : IOrderSupportChatService
{
    private const int ContextOrderCount = 5;

    [GeneratedRegex(@"#?\b(\d{1,10})\b")]
    private static partial Regex OrderIdPattern();

    /// <inheritdoc />
    public async Task<SupportChatResult> AskAsync(
        string question,
        IReadOnlyList<(string Role, string Content)> history,
        CancellationToken ct = default)
    {
        // Fast path: pure FAQ questions (no order id, no history) get a
        // deterministic canned answer with no LLM round-trip — faster and
        // avoids the model paraphrasing/hallucinating policy text.
        if (history.Count == 0 && !ExtractOrderIds(question).Any())
        {
            var canned = MatchCannedFaq(question);
            if (canned is not null) return new SupportChatResult(canned, []);
        }

        var relatedOrders = (await orderSearch.SearchAsync(question, ContextOrderCount, ct)).ToList();

        // Semantic similarity search doesn't reliably find an explicitly
        // named order id (e.g. "Order ID#12") because the id itself carries
        // no semantic meaning. If the question mentions numbers, look those
        // orders up directly and merge them in.
        foreach (var id in ExtractOrderIds(question))
        {
            if (relatedOrders.Any(o => o.Id == id)) continue;

            var order = await orderService.GetByIdAsync(id);
            if (order is not null) relatedOrders.Insert(0, order);
        }

        var chat = new ChatHistory(BuildSystemPrompt(relatedOrders));
        foreach (var (role, content) in history)
            chat.AddMessage(role.Equals("assistant", StringComparison.OrdinalIgnoreCase) ? AuthorRole.Assistant : AuthorRole.User, content);
        chat.AddUserMessage(question);

        var response = await chatCompletion.GetChatMessageContentAsync(chat, cancellationToken: ct);
        var answer = response.Content ?? "Sorry, I couldn't come up with an answer.";

        return new SupportChatResult(answer, relatedOrders);
    }

    private static IEnumerable<long> ExtractOrderIds(string question) =>
        OrderIdPattern().Matches(question)
            .Select(m => long.TryParse(m.Groups[1].Value, out var id) ? id : (long?)null)
            .Where(id => id is > 0)
            .Select(id => id!.Value)
            .Distinct();

    // Static process/policy knowledge, always available to the assistant so
    // it can answer general "how does this work" questions in addition to
    // questions grounded in specific order data below.
    private const string FaqKnowledge = """
        Order statuses:
        - Pending: order has been placed but not yet fulfilled.
        - Completed: order has been fulfilled and closed out.
        - Cancelled: order was cancelled and will not be fulfilled.

        How order totals are calculated:
        - Each line item's total = quantity x unit price.
        - The order's total amount = sum of all its line items' totals.

        General guidance:
        - To look up a specific order, ask by its order ID (e.g. "status of order 12").
        - To find orders matching a description (e.g. "high-value orders", "orders with laptops"), ask in plain language and the system will search for semantically similar orders.
        - Staff can create, edit, and delete orders from the Orders section of the app; this chat is read-only and cannot modify orders.

        Returns, refunds and cancellations:
        - This chat cannot process returns, refunds, or cancellations. Direct the customer/requester to the order's owning team or update the order status manually in the Orders section.
        - An order should only be marked Cancelled before it has shipped; once Completed, treat it as a return, not a cancellation.

        Escalation:
        - If a question is about a policy, dispute, or anything outside order status/contents/totals, say so and advise escalating to a supervisor rather than guessing.
        """;

    // Exact/keyword canned answers for the most common support questions,
    // checked before the FAQ text is even sent to the LLM. Keys are matched
    // as case-insensitive substrings of the question.
    private static readonly (string[] Keywords, string Answer)[] CannedFaqs =
    [
        (["what does pending mean", "pending status", "what is pending"],
            "Pending means the order has been placed but not yet fulfilled."),
        (["what does completed mean", "completed status", "what is completed"],
            "Completed means the order has been fulfilled and closed out."),
        (["what does cancelled mean", "cancelled status", "what is cancelled"],
            "Cancelled means the order was cancelled and will not be fulfilled."),
        (["how is the total", "how is total amount calculated", "how do you calculate the total"],
            "Each line item's total is quantity x unit price, and the order's total amount is the sum of all its line items' totals."),
        (["process a refund", "issue a refund", "refund"],
            "This chat can't process refunds. Direct the request to the order's owning team, or update the order status manually in the Orders section."),
        (["cancel an order", "how do i cancel"],
            "Orders can only be cancelled before they've shipped. Update the order's status to Cancelled from the Orders section; if it's already Completed, treat it as a return instead."),
        (["create a new order", "how do i create an order", "add an order"],
            "Go to the Orders section and use the \"+ New Order\" button to create an order."),
    ];

    private static string? MatchCannedFaq(string question)
    {
        var normalized = question.Trim().ToLowerInvariant();
        foreach (var (keywords, answer) in CannedFaqs)
        {
            if (keywords.Any(normalized.Contains))
                return answer;
        }
        return null;
    }

    private static string BuildSystemPrompt(IReadOnlyList<OrderDto> relatedOrders)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are the internal Support Desk assistant for order management staff.");
        sb.AppendLine("Use the FAQ knowledge for general/process questions, and the order data for questions about specific orders.");
        sb.AppendLine("Do not invent order data that isn't listed below. If neither section answers the question, say so plainly.");
        sb.AppendLine();
        sb.AppendLine("FAQ knowledge:");
        sb.AppendLine(FaqKnowledge);
        sb.AppendLine();

        if (relatedOrders.Count == 0)
        {
            sb.AppendLine("No matching orders were found for this question.");
            return sb.ToString();
        }

        sb.AppendLine("Relevant orders:");
        foreach (var order in relatedOrders)
        {
            sb.AppendLine($"- Order #{order.Id}: customer {order.CustomerName}, placed {order.OrderDate:yyyy-MM-dd}, status {order.Status}, total {order.TotalAmount:F2}.");
            foreach (var detail in order.OrderDetails)
                sb.AppendLine($"    * {detail.ProductName} x{detail.OrderQty} @ {detail.UnitPrice:F2}");
        }

        return sb.ToString();
    }
}
