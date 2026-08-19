using Chatbot.Models;
using Microsoft.AspNetCore.Mvc;

namespace Chatbot.ViewComponents;

public class ProductSelectViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(long? selectedId = null, string? selectedText = null,
        string fieldName = "ProductId", string elementId = "productSelect")
    {
        var model = new ProductSelectViewModel
        {
            FieldName = fieldName,
            ElementId = elementId,
            SelectedId = selectedId,
            SelectedText = selectedText
        };

        return View(model);
    }
}
