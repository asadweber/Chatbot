namespace Application.Services;

// Static process/policy knowledge and canned answers for the order support
// chat. Kept separate from OrderSupportChatService so that orchestration
// logic isn't buried under FAQ content, and so FAQ text can be edited without
// touching the service.
public static class SupportFaqKnowledge
{
    // Always available to the assistant so it can answer general "how does
    // this work" questions in addition to questions grounded in specific
    // order data.
    public const string FaqText = """
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

    /// <summary>One canned FAQ entry: a display question shown in the UI, the substrings that match it, and its answer.</summary>
    public record FaqEntry(string DisplayQuestion, string[] Keywords, string Answer);

    // Order-scoped question templates: no fixed answer (depends on which
    // order), so they're formatted with the session's order id and asked
    // through the normal chat/LLM flow instead of being matched as a canned
    // answer. {0} is replaced with the order id.
    public const string CustomerDetailQuestionTemplate = "What is the customer detail of order #{0}?";
    public const string CurrentStatusQuestionTemplate = "What is the current status of order #{0}?";
    public const string OrderDetailQuestionTemplate = "What are the order details of order #{0}?";

    // Exact/keyword canned answers for the most common support questions,
    // checked before the FAQ text is even sent to the LLM. Keywords are
    // matched as case-insensitive substrings of the question. DisplayQuestion
    // is shown as a clickable prompt in the Support Desk UI.
    private static readonly FaqEntry[] CannedAnswers =
    [
        new("What does Pending mean?", ["pending"], "Pending means the order has been placed but not yet fulfilled."),
        new("What does Completed mean?", ["completed"], "Completed means the order has been fulfilled and closed out."),
        new("What does Cancelled mean?", ["cancelled"], "Cancelled means the order was cancelled and will not be fulfilled."),
        new("How do I cancel an order?", ["cancel an order", "how do i cancel"],
            "Orders can only be cancelled before they've shipped. Update the order's status to Cancelled from the Orders section; if it's already Completed, treat it as a return instead."),
        new("How do I create a new order?", ["create a new order", "add an order"],
            "Go to the Orders section and use the \"+ New Order\" button to create an order."),
        new("How do I edit an order?", ["edit an order", "update an order"],
            "Go to the Orders section, open the order, and use the Edit action there. This chat is read-only and cannot modify orders."),
        new("How do I delete an order?", ["delete an order", "remove an order"],
            "Go to the Orders section and use the Delete action on the order. This chat is read-only and cannot delete orders."),
    ];

    /// <summary>All canned FAQ entries, for display in the Support Desk UI.</summary>
    public static IReadOnlyList<FaqEntry> AllEntries => CannedAnswers;

    public static string? MatchCannedAnswer(string question)
    {
        var normalized = question.Trim().ToLowerInvariant();
        foreach (var entry in CannedAnswers)
        {
            if (entry.Keywords.Any(normalized.Contains))
                return entry.Answer;
        }
        return null;
    }
}
