using System.Text;
using Application.Dtos;
using Application.Interfaces;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Application.Services;

/// <inheritdoc cref="IOrderSupportChatService" />
public class OrderSupportChatService(
    IOrderSemanticSearchService orderSearch,
    IChatCompletionService chatCompletion) : IOrderSupportChatService
{
    private const int ContextOrderCount = 5;

    /// <inheritdoc />
    public async Task<SupportChatResult> AskAsync(
        string question,
        IReadOnlyList<(string Role, string Content)> history,
        CancellationToken ct = default)
    {
        var relatedOrders = await orderSearch.SearchAsync(question, ContextOrderCount, ct);

        var chat = new ChatHistory(BuildSystemPrompt(relatedOrders));
        foreach (var (role, content) in history)
            chat.AddMessage(role.Equals("assistant", StringComparison.OrdinalIgnoreCase) ? AuthorRole.Assistant : AuthorRole.User, content);
        chat.AddUserMessage(question);

        var response = await chatCompletion.GetChatMessageContentAsync(chat, cancellationToken: ct);
        var answer = response.Content ?? "Sorry, I couldn't come up with an answer.";

        return new SupportChatResult(answer, relatedOrders);
    }

    private static string BuildSystemPrompt(IReadOnlyList<OrderDto> relatedOrders)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are the internal Support Desk assistant for order management staff.");
        sb.AppendLine("Answer questions using ONLY the order data below. If the data doesn't cover the question, say so plainly.");
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
