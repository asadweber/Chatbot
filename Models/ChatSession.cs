namespace Chatbot.Models;

/// <summary>
/// A chat conversation: an ordered collection of <see cref="ChatMessage"/>
/// entries shown together in the UI under a single title.
/// </summary>
public class ChatSession
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>
    /// Display title shown in the session list. Defaults to "New chat" until
    /// renamed by the user or auto-derived from the first user message.
    /// </summary>
    public string Title { get; set; } = "New chat";

    /// <summary>UTC timestamp at which the session was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Messages belonging to this session, in chronological order.</summary>
    public List<ChatMessage> Messages { get; set; } = new();
}
