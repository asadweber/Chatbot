using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Chatbot.Controllers;

public class CustomersController(ICustomerService customerService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var customers = await customerService.GetAllAsync();
        return View(customers);
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
