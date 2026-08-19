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
