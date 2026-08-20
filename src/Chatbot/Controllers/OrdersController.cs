using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Chatbot.Controllers;

public class OrdersController(IOrderService orderService, IOrderIngestionService orderIngestion, IOrderSemanticSearchService orderSearch) : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> IndexData()
    {
        var form = Request.Form;

        var request = new DataTableRequestDto
        {
            Draw = int.TryParse(form["draw"], out var draw) ? draw : 0,
            Start = int.TryParse(form["start"], out var start) ? start : 0,
            Length = int.TryParse(form["length"], out var length) && length > 0 ? length : 10,
            SearchValue = form["search[value]"],
            SortColumn = form[$"columns[{form["order[0][column]"]}][data]"],
            SortDirection = form["order[0][dir]"]
        };

        var result = await orderService.GetPagedAsync(request);
        return Json(result);
    }

    public async Task<IActionResult> Details(long id)
    {
        var order = await orderService.GetByIdAsync(id);
        return order is null ? NotFound() : View(order);
    }

    public IActionResult Create()
    {
        return View(new OrderDto { OrderDate = DateTime.UtcNow });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrderDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        await orderService.CreateAsync(dto);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(long id)
    {
        var order = await orderService.GetByIdAsync(id);
        return order is null ? NotFound() : View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, OrderDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var updated = await orderService.UpdateAsync(id, dto);
        return updated ? RedirectToAction(nameof(Index)) : NotFound();
    }

    public async Task<IActionResult> Delete(long id)
    {
        var order = await orderService.GetByIdAsync(id);
        return order is null ? NotFound() : View(order);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        await orderService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Search()
    {
        return View(new List<OrderDto>());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Search(string query, CancellationToken ct)
    {
        ViewBag.Query = query;

        var results = string.IsNullOrWhiteSpace(query)
            ? []
            : await orderSearch.SearchAsync(query, 10, ct);

        return View(results);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReindexAll(CancellationToken ct)
    {
        await orderIngestion.ReindexAllAsync(ct);
        TempData["Message"] = "Order semantic index rebuilt.";
        return RedirectToAction(nameof(Index));
    }
}
