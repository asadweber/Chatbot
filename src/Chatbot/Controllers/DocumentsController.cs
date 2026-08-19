using Application.Interfaces;
using Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Chatbot.Controllers;

/// <summary>
/// Manages the document library used for retrieval-augmented generation:
/// lists ingested documents and handles new file uploads, validating type
/// and size before delegating to <see cref="IDocumentIngestionService"/>.
/// </summary>
public class DocumentsController : Controller
{
    /// <summary>File extensions accepted for upload/ingestion.</summary>
    private static readonly string[] AllowedExtensions = { ".pdf", ".txt", ".md" };

    private readonly IDocumentRepository _documents;
    private readonly IDocumentIngestionService _ingestion;

    public DocumentsController(IDocumentRepository documents, IDocumentIngestionService ingestion)
    {
        _documents = documents;
        _ingestion = ingestion;
    }

    /// <summary>Lists all ingested documents (with their chunks loaded), newest first.</summary>
    public async Task<IActionResult> Index()
    {
        var documents = await _documents.GetAllWithChunksAsync();
        return View(documents.ToList());
    }

    /// <summary>
    /// Validates and ingests an uploaded file (PDF/TXT/MD, up to 50 MB),
    /// surfacing success/validation errors via <c>TempData</c> for display
    /// after the redirect back to <see cref="Index"/>.
    /// </summary>
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
