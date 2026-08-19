using Chatbot.Models;
using Microsoft.AspNetCore.Mvc;

namespace Chatbot.ViewComponents;

public class CustomerSelectViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(int? selectedId = null, string? selectedText = null,
        string fieldName = "CustomerId", string elementId = "customerSelect")
    {
        var model = new CustomerSelectViewModel
        {
            FieldName = fieldName,
            ElementId = elementId,
            SelectedId = selectedId,
            SelectedText = selectedText
        };

        return View(model);
    }
}
