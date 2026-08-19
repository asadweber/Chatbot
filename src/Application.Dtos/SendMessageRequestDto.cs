namespace Application.Dtos;

/// <summary>Request body for <c>POST /Chat/Send</c>: a user message to send within a session.</summary>
public class SendMessageRequestDto
{
    /// <summary>Id of the target <see cref="ChatSessionDto"/>.</summary>
    public int SessionId { get; set; }

    /// <summary>The user's message text.</summary>
    public string Message { get; set; } = string.Empty;
}
