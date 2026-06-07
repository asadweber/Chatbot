using Chatbot.Data;
using Chatbot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chatbot.Controllers;

public class DocumentsController : Controller
{
    private static readonly string[] AllowedExtensions = { ".pdf", ".txt", ".md" };

    private readonly ApplicationDbContext _db;
    private readonly IDocumentIngestionService _ingestion;

    public DocumentsController(ApplicationDbContext db, IDocumentIngestionService ingestion)
    {
        _db = db;
        _ingestion = ingestion;
    }

    public async Task<IActionResult> Index()
    {
        var documents = await _db.Documents
            .Include(d => d.Chunks)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();

        return View(documents);
    }

    [HttpPost]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            TempData["UploadError"] = "Please choose a file to upload.";
            return RedirectToAction(nameof(Index));
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            TempData["UploadError"] = $"Unsupported file type '{extension}'. Allowed: {string.Join(", ", AllowedExtensions)}.";
            return RedirectToAction(nameof(Index));
        }

        await using var stream = file.OpenReadStream();
        await _ingestion.IngestionAsync(file.FileName, stream, ct);

        TempData["UploadSuccess"] = $"'{file.FileName}' ingested successfully.";
        return RedirectToAction(nameof(Index));
    }
}
