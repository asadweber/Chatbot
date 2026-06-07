namespace Chatbot.Models;

public class ChatMessage
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public ChatSession? Session { get; set; }

    public string Role { get; set; } = string.Empty; // "user" | "assistant"
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
