using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Chatbot.Controllers;

public class ProductsController(IProductService productService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Search(string? q, int page = 1)
    {
        const int pageSize = 10;

        var request = new DataTableRequestDto
        {
            Start = (page - 1) * pageSize,
            Length = pageSize,
            SearchValue = q,
            SortColumn = "Name",
            SortDirection = "asc"
        };

        var result = await productService.GetPagedAsync(request);

        return Json(new
        {
            results = result.Data.Select(p => new { id = p.Id, text = p.Name, price = p.Price }),
            pagination = new { more = result.RecordsFiltered > page * pageSize }
        });
    }
}
