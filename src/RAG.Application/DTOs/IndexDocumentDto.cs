namespace RAG.Application.DTOs;

public class IndexDocumentDto
{
    public Guid DocumentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ChunkCount { get; set; }
    public string Status { get; set; } = string.Empty;
}