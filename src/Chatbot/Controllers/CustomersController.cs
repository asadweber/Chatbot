using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Chatbot.Controllers;

public class CustomersController(ICustomerService customerService) : Controller
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

        var result = await customerService.GetPagedAsync(request);
        return Json(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var customer = await customerService.GetByIdAsync(id);
        return customer is null ? NotFound() : View(customer);
    }

    public IActionResult Create()
    {
        return View(new CustomerDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        await customerService.CreateAsync(dto);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var customer = await customerService.GetByIdAsync(id);
        return customer is null ? NotFound() : View(customer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CustomerDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var updated = await customerService.UpdateAsync(id, dto);
        return updated ? RedirectToAction(nameof(Index)) : NotFound();
    }

    public async Task<IActionResult> Delete(int id)
    {
        var customer = await customerService.GetByIdAsync(id);
        return customer is null ? NotFound() : View(customer);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await customerService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
