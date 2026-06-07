namespace Chatbot.Models;

public class ChatPageViewModel
{
    public List<ChatSession> Sessions { get; set; } = new();
    public ChatSession? ActiveSession { get; set; }
}

public class SendMessageRequest
{
    public int SessionId { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class SendMessageResponse
{
    public string Reply { get; set; } = string.Empty;
}

public class RenameSessionRequest
{
    public int SessionId { get; set; }
    public string Title { get; set; } = string.Empty;
}

public class DeleteSessionRequest
{
    public int SessionId { get; set; }
}
