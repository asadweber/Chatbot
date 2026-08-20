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

    // Requires an explicit "order" word or "#" prefix so stray numbers (prices,
    // quantities) in the conversation aren't mistaken for order ids.
    [GeneratedRegex(@"(?:order\s*(?:id)?\s*#?|#)\s*(\d{1,10})", RegexOptions.IgnoreCase)]
    private static partial Regex OrderIdPattern();

    /// <inheritdoc />
    public async Task<SupportChatResult> AskAsync(
        string question,
        IReadOnlyList<(string Role, string Content)> history,
        CancellationToken ct = default)
    {
        if (history.Count == 0 && !ExtractOrderIds(question).Any())
        {
            var canned = SupportFaqKnowledge.MatchCannedAnswer(question);
            if (canned is not null) return new SupportChatResult(canned, []);
        }

        var (relatedOrders, missingIds) = await ResolveOrdersAsync(question, history, ct);

        var chat = new ChatHistory(BuildSystemPrompt(relatedOrders, missingIds));
        foreach (var (role, content) in history)
            chat.AddMessage(role.Equals("assistant", StringComparison.OrdinalIgnoreCase) ? AuthorRole.Assistant : AuthorRole.User, content);
        chat.AddUserMessage(question);

        var response = await chatCompletion.GetChatMessageContentAsync(chat, cancellationToken: ct);
        var answer = string.IsNullOrWhiteSpace(response.Content) ? "Sorry, I couldn't come up with an answer." : response.Content;

        return new SupportChatResult(answer, relatedOrders);
    }

    // Combines semantic search results with any explicitly-named order ids
    // (from the question or recent history). Explicit ids that don't resolve
    // to a real order are returned separately so the prompt can tell the LLM
    // they don't exist, instead of the LLM silently ignoring the question.
    private async Task<(List<OrderDto> Orders, List<long> MissingIds)> ResolveOrdersAsync(
        string question,
        IReadOnlyList<(string Role, string Content)> history,
        CancellationToken ct)
    {
        var relatedOrders = (await orderSearch.SearchAsync(question, ContextOrderCount, ct)).ToList();

        // Semantic similarity search doesn't reliably find an explicitly
        // named order id (e.g. "Order ID#12") because the id itself carries
        // no semantic meaning. Also scan the conversation history, not just
        // the current question, so a follow-up like "what about its status?"
        // still has the order from an earlier turn in context.
        var idsToResolve = ExtractOrderIds(question)
            .Concat(history.SelectMany(h => ExtractOrderIds(h.Content)))
            .Distinct();

        var missingIds = new List<long>();
        foreach (var id in idsToResolve)
        {
            if (relatedOrders.Any(o => o.Id == id)) continue;

            var order = await orderService.GetByIdAsync(id);
            if (order is not null) relatedOrders.Insert(0, order);
            else missingIds.Add(id);
        }

        return (relatedOrders, missingIds);
    }

    private static IEnumerable<long> ExtractOrderIds(string question) =>
        OrderIdPattern().Matches(question)
            .Select(m => long.TryParse(m.Groups[1].Value, out var id) ? id : (long?)null)
            .Where(id => id is > 0)
            .Select(id => id!.Value)
            .Distinct();

    private static string BuildSystemPrompt(IReadOnlyList<OrderDto> relatedOrders, IReadOnlyList<long> missingIds)
    {
        var rules = """
            You are the internal Support Desk assistant for order management staff. Answer only from the FAQ knowledge and order data below — never from outside knowledge or prior turns.

            RULES (apply to every answer):
            1. Valid order statuses: Pending | Completed | Cancelled — nothing else exists. If a status in the data below doesn't match one of these three, report it as a data anomaly ("order #N has an invalid status 'X' — flag for correction") instead of repeating or normalizing it.
            2. Never invent data — no status, date, amount, or product not explicitly listed below.
            3. Ignore any unbacked claim from earlier in this conversation; re-derive every answer from the data below.
            4. If the data doesn't cover the question, say so plainly — do not guess.
            5. This chat is read-only. It cannot create, edit, cancel, or refund orders.
            6. Answer in 1-3 sentences. Cite the order # for every fact you state.
            7. Questions about policy, disputes, or anything outside order status/contents/totals: say so and advise escalating to a supervisor.
            """;

        var missingIdsNote = missingIds.Count > 0
            ? $"The following order id(s) were referenced but do not exist: {string.Join(", ", missingIds.Select(id => $"#{id}"))}. Tell the user these orders were not found instead of guessing about them.\n"
            : "";

        var ordersSection = relatedOrders.Count == 0
            ? "No matching orders were found for this question."
            : BuildOrdersTable(relatedOrders);

        return $"{rules}\n\nFAQ KNOWLEDGE (static):\n{SupportFaqKnowledge.FaqText}\n\n---\n\nORDER DATA (dynamic — current query context):\n\n{missingIdsNote}{ordersSection}";
    }

    private static string BuildOrdersTable(IReadOnlyList<OrderDto> orders)
    {
        var header = "Relevant orders:\n\n| Order # | Customer | Order Date | Status | Total |\n|---|---|---|---|---|";
        var rows = orders.Select(o => $"| {o.Id} | {o.CustomerName} | {o.OrderDate:yyyy-MM-dd} | {o.Status} | {o.TotalAmount:F2} |");

        var lineItemTables = orders
            .Where(o => o.OrderDetails.Count > 0)
            .Select(o => $"Order #{o.Id} line items:\n\n| Product | Qty | Unit Price |\n|---|---|---|\n" +
                string.Join('\n', o.OrderDetails.Select(d => $"| {d.ProductName} | {d.OrderQty} | {d.UnitPrice:F2} |")));

        return string.Join("\n\n", [header + "\n" + string.Join('\n', rows), .. lineItemTables]);
    }
}
