namespace Chatbot.Models;

/// <summary>
/// A single message within a <see cref="ChatSession"/>, authored either by
/// the user or the assistant.
/// </summary>
public class ChatMessage
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>Foreign key to the owning <see cref="ChatSession"/>.</summary>
    public int SessionId { get; set; }

    /// <summary>Navigation property to the owning chat session.</summary>
    public ChatSession? Session { get; set; }

    /// <summary>Author of the message: "user" or "assistant".</summary>
    public string Role { get; set; } = string.Empty; // "user" | "assistant"

    /// <summary>The message text.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>UTC timestamp at which the message was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
