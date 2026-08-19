namespace Application.Dtos;

/// <summary>Request body for <c>POST /Chat/DeleteSession</c>.</summary>
public class DeleteSessionRequestDto
{
    /// <summary>Id of the session to delete.</summary>
    public int SessionId { get; set; }
}
