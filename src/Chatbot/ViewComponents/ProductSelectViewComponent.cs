using Application.Interfaces;
using Chatbot.Models;
using Microsoft.AspNetCore.Mvc;

namespace Chatbot.ViewComponents;

public class ProductSelectViewComponent(IProductService productService) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(long? selectedId = null,
        string fieldName = "ProductId", string elementId = "productSelect")
    {
        var products = await productService.GetAllAsync();

        var model = new ProductSelectViewModel
        {
            FieldName = fieldName,
            ElementId = elementId,
            SelectedId = selectedId,
            Products = products
        };

        return View(model);
    }
}
