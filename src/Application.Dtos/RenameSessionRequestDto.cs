namespace Application.Dtos;

/// <summary>Request body for <c>POST /Chat/RenameSession</c>.</summary>
public class RenameSessionRequestDto
{
    /// <summary>Id of the session to rename.</summary>
    public int SessionId { get; set; }

    /// <summary>New title for the session (truncated to 60 chars server-side).</summary>
    public string Title { get; set; } = string.Empty;
}
