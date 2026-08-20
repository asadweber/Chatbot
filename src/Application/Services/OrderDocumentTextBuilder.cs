using System.Globalization;
using System.Text;
using Application.Dtos;
using Application.Interfaces;

namespace Application.Services;

/// <inheritdoc cref="IOrderDocumentTextBuilder" />
public class OrderDocumentTextBuilder : IOrderDocumentTextBuilder
{
    /// <inheritdoc />
    public string Build(OrderDto order)
    {
        var details = order.OrderDetails ?? [];

        var sb = new StringBuilder();
        sb.AppendLine($"Order ID: {order.Id}");
        sb.AppendLine();
        sb.AppendLine("Customer:");
        sb.AppendLine(string.IsNullOrWhiteSpace(order.CustomerName) ? "Unknown" : order.CustomerName);
        sb.AppendLine();
        sb.AppendLine("Order Date:");
        sb.AppendLine(order.OrderDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        sb.AppendLine();
        sb.AppendLine("Status:");
        sb.AppendLine(string.IsNullOrWhiteSpace(order.Status) ? "Unknown" : order.Status);
        sb.AppendLine();
        sb.AppendLine("Total Amount:");
        sb.AppendLine(order.TotalAmount.ToString("F2", CultureInfo.InvariantCulture));
        sb.AppendLine();
        sb.AppendLine("Products:");
        foreach (var detail in details)
            sb.AppendLine($"- {detail.ProductName}, quantity {detail.OrderQty}, price {detail.UnitPrice.ToString("F2", CultureInfo.InvariantCulture)}");

        // Derived summary line: gives the embedding extra signal for queries
        // that don't map to any single field above — order size, value tier,
        // price spread, how recent the order is.
        if (details.Count > 0)
        {
            var totalQty = details.Sum(d => d.OrderQty);
            var avgUnitPrice = details.Average(d => d.UnitPrice);
            var minUnitPrice = details.Min(d => d.UnitPrice);
            var maxUnitPrice = details.Max(d => d.UnitPrice);
            var valueTier = ValueTier(order.TotalAmount);
            var recency = RecencyPhrase(order.OrderDate);

            sb.AppendLine();
            sb.AppendLine("Summary:");
            sb.AppendLine(string.Create(CultureInfo.InvariantCulture,
                $"This is a {valueTier}-value order placed {recency}, containing {details.Count} distinct product(s) and {totalQty} total item(s). Unit prices range from {minUnitPrice:F2} to {maxUnitPrice:F2}, averaging {avgUnitPrice:F2}."));

            if (details.Count > 1)
                sb.AppendLine("The customer purchased multiple products in this single order.");
        }

        return sb.ToString();
    }

    private static string ValueTier(decimal totalAmount) => totalAmount switch
    {
        >= 1000 => "high",
        >= 300 => "mid",
        _ => "low"
    };

    private static string RecencyPhrase(DateTime orderDate)
    {
        var days = (DateTime.UtcNow.Date - orderDate.Date).Days;
        return days switch
        {
            <= 7 => "recently (within the last week)",
            <= 30 => "recently (within the last month)",
            <= 90 => "in the last few months",
            _ => "some time ago"
        };
    }
}
