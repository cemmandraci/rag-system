namespace RAG.API.Models;

public class IndexDocumentRequest
{
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
}