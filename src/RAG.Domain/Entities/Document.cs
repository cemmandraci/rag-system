namespace RAG.Domain.Entities;

public class Document
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string Content { get; private set; }
    public DocumentStatus Status { get; private set; }
    public int ChunkCount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    private Document(){ }

    public static Document Create(string title, string content)
    {
        return new Document
        {
            Id = Guid.NewGuid(),
            Title = title,
            Content = content,
            Status = DocumentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    public void MarkAsProcessing() => Status = DocumentStatus.Processing;

    public void MarkAsCompleted(int chunkCount)
    {
        Status = DocumentStatus.Completed;
        ChunkCount = chunkCount;
    }

    public void MarkAsFailed() => Status = DocumentStatus.Failed;
    
}

public enum DocumentStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}