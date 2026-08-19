namespace Chatbot.Models;

public class CustomerSelectViewModel
{
    public string FieldName { get; set; } = "CustomerId";
    public string ElementId { get; set; } = "customerSelect";
    public int? SelectedId { get; set; }
    public string? SelectedText { get; set; }
}
