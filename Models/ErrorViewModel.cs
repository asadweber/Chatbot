namespace Chatbot.Models;

/// <summary>
/// View model for the generic error page (<c>Home/Error</c>), carrying the
/// current request's diagnostic identifier for display/troubleshooting.
/// </summary>
public class ErrorViewModel
{
    /// <summary>
    /// Diagnostic identifier for the failed request, sourced from
    /// <c>Activity.Current?.Id</c> or <c>HttpContext.TraceIdentifier</c>.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>True when <see cref="RequestId"/> has a value worth displaying.</summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
