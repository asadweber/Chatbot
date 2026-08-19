namespace Application.Dtos;

/// <summary>Response body for <c>POST /Chat/Send</c>: the assistant's reply.</summary>
public class SendMessageResponseDto
{
    /// <summary>The assistant-generated reply text.</summary>
    public string Reply { get; set; } = string.Empty;
}
