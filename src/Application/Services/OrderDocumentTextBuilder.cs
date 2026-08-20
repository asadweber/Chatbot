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
        var sb = new StringBuilder();
        sb.AppendLine($"Order ID: {order.Id}");
        sb.AppendLine();
        sb.AppendLine("Customer:");
        sb.AppendLine(order.CustomerName);
        sb.AppendLine();
        sb.AppendLine("Order Date:");
        sb.AppendLine(order.OrderDate.ToString("yyyy-MM-dd"));
        sb.AppendLine();
        sb.AppendLine("Status:");
        sb.AppendLine(order.Status);
        sb.AppendLine();
        sb.AppendLine("Total Amount:");
        sb.AppendLine(order.TotalAmount.ToString("F2"));
        sb.AppendLine();
        sb.AppendLine("Products:");
        foreach (var detail in order.OrderDetails)
            sb.AppendLine($"- {detail.ProductName}, quantity {detail.OrderQty}, price {detail.UnitPrice:F2}");

        return sb.ToString();
    }
}
