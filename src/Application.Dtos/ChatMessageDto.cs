namespace Application.Dtos;

/// <summary>A single message within a <see cref="ChatSessionDto"/>.</summary>
[Serializable]
public class ChatMessageDto
{
    public int Id { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
