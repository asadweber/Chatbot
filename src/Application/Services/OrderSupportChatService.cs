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
        """;

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
