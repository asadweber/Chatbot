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
        // like "large orders" or "orders with multiple products" that don't
        // map to any single field above.
        if (details.Count > 0)
        {
            var totalQty = details.Sum(d => d.OrderQty);
            var avgUnitPrice = details.Average(d => d.UnitPrice);
            sb.AppendLine();
            sb.AppendLine("Summary:");
            sb.AppendLine(string.Create(CultureInfo.InvariantCulture,
                $"{details.Count} distinct product(s), {totalQty} total item(s), average unit price {avgUnitPrice:F2}."));
        }

        return sb.ToString();
    }
}
