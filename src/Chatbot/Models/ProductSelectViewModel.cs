using Application.Dtos;

namespace Chatbot.Models;

public class ProductSelectViewModel
{
    public string FieldName { get; set; } = "ProductId";
    public string ElementId { get; set; } = "productSelect";
    public long? SelectedId { get; set; }
    public List<ProductDto> Products { get; set; } = [];
}
