namespace Application.Dtos;

[Serializable]
public class DocumentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public int ChunkCount { get; set; }
}
