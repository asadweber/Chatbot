namespace Application.Dtos;

/// <summary>A chat session: id, title, and (when loaded) its messages.</summary>
[Serializable]
public class ChatSessionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "New chat";
    public DateTime CreatedAt { get; set; }
    public List<ChatMessageDto> Messages { get; set; } = new();
}

/// <summary>A single message within a <see cref="ChatSessionDto"/>.</summary>
[Serializable]
public class ChatMessageDto
{
    public int Id { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// View model for the main chat page: the list of sessions for the sidebar
/// plus the currently active session (if any) with its messages loaded.
/// </summary>
public class ChatPageViewModel
{
    /// <summary>All chat sessions, newest first, shown in the sidebar.</summary>
    public List<ChatSessionDto> Sessions { get; set; } = new();

    /// <summary>The session currently open in the main pane, with messages loaded, or null if none selected.</summary>
    public ChatSessionDto? ActiveSession { get; set; }
}

/// <summary>Request body for <c>POST /Chat/Send</c>: a user message to send within a session.</summary>
public class SendMessageRequest
{
    /// <summary>Id of the target <see cref="ChatSessionDto"/>.</summary>
    public int SessionId { get; set; }

    /// <summary>The user's message text.</summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>Response body for <c>POST /Chat/Send</c>: the assistant's reply.</summary>
public class SendMessageResponse
{
    /// <summary>The assistant-generated reply text.</summary>
    public string Reply { get; set; } = string.Empty;
}

/// <summary>Request body for <c>POST /Chat/RenameSession</c>.</summary>
public class RenameSessionRequest
{
    /// <summary>Id of the session to rename.</summary>
    public int SessionId { get; set; }

    /// <summary>New title for the session (truncated to 60 chars server-side).</summary>
    public string Title { get; set; } = string.Empty;
}

/// <summary>Request body for <c>POST /Chat/DeleteSession</c>.</summary>
public class DeleteSessionRequest
{
    /// <summary>Id of the session to delete.</summary>
    public int SessionId { get; set; }
}
