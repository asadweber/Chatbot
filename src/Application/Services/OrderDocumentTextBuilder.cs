using Application.Dtos;
using Application.Interfaces;
using System.Globalization;
using System.Text;

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
        // price spread, how recent the order is, what dominates the order.
        if (details.Count > 0)
        {
            var totalQty = details.Sum(d => d.OrderQty);
            var avgUnitPrice = details.Average(d => d.UnitPrice);
            var minUnitPrice = details.Min(d => d.UnitPrice);
            var maxUnitPrice = details.Max(d => d.UnitPrice);
            var valueTier = ValueTier(order.TotalAmount);
            var recency = RecencyPhrase(order.OrderDate);
            var dominantProduct = details.OrderByDescending(d => d.Total).First();

            var itemWord = totalQty == 1 ? "item" : "items";
            var productWord = details.Count == 1 ? "product" : "products";
            var productPhrase = details.Count == 1
                ? dominantProduct.ProductName
                : string.Create(CultureInfo.InvariantCulture, $"{details.Count} {productWord}, the largest being {dominantProduct.ProductName} at {dominantProduct.Total:F2}");

            var priceSpreadPhrase = minUnitPrice == maxUnitPrice
                ? string.Create(CultureInfo.InvariantCulture, $"priced at {avgUnitPrice:F2} per unit")
                : string.Create(CultureInfo.InvariantCulture, $"priced between {minUnitPrice:F2} and {maxUnitPrice:F2} per unit (averaging {avgUnitPrice:F2})");

            sb.AppendLine();
            sb.AppendLine("Summary:");
            sb.AppendLine(string.Create(CultureInfo.InvariantCulture,
                $"This {valueTier}-value order was placed {recency}. It includes {productPhrase}, totaling {totalQty} {itemWord}, {priceSpreadPhrase}."));

            var bulkItems = details.Where(d => d.OrderQty >= 5).ToList();
            if (bulkItems.Count > 0)
                sb.AppendLine(string.Create(CultureInfo.InvariantCulture,
                    $"It looks like a bulk purchase, with large quantities of {string.Join(", ", bulkItems.Select(d => $"{d.ProductName} (x{d.OrderQty})"))}."));

            sb.AppendLine(StatusPhrase(order.Status));
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

    private static string StatusPhrase(string status) => status?.Trim().ToLowerInvariant() switch
    {
        "completed" => "This order has been completed and fulfilled.",
        "cancelled" => "This order was cancelled.",
        "pending" => "This order is still pending fulfillment.",
        _ => $"This order's current status is {status}."
    };
}
