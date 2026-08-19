using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Chatbot.Controllers;

/// <summary>
/// General-purpose pages: landing page, privacy notice, and the generic
/// error page used by the exception handling middleware.
/// </summary>
public class HomeController : Controller
{
    /// <summary>Landing/home page.</summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>Static privacy policy page.</summary>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Generic error page shown for unhandled exceptions (wired up via
    /// <c>app.UseExceptionHandler("/Home/Error")</c> in Program.cs).
    /// Disables response caching so stale error pages are never served, and
    /// surfaces the current request's diagnostic id for troubleshooting.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new Models.ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
