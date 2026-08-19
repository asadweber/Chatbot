namespace Application.Dtos;

/// <summary>
/// View model for the main chat page: the list of sessions for the sidebar
/// plus the currently active session (if any) with its messages loaded.
/// </summary>
public class ChatPageViewModelDto
{
    /// <summary>All chat sessions, newest first, shown in the sidebar.</summary>
    public List<ChatSessionDto> Sessions { get; set; } = new();

    /// <summary>The session currently open in the main pane, with messages loaded, or null if none selected.</summary>
    public ChatSessionDto? ActiveSession { get; set; }
}
