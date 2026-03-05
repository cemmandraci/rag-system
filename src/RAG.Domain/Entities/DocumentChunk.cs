namespace RAG.Domain.Entities;

public class DocumentChunk
{
    public Guid Id { get; private set; }
    public Guid DocumentId { get; private set; }
    public string DocumentTitle { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public int ChunkIndex { get; private set; }
    public float[]? Embedding { get; private set; }
    
    private DocumentChunk() { }
    
    public static DocumentChunk Create(
        Guid documentId,
        string documentTitle,
        string content,
        int chunkIndex)
    {
        return new DocumentChunk
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            DocumentTitle = documentTitle,
            Content = content,
            ChunkIndex = chunkIndex
        };
    }
    
    public void SetEmbedding(float[] embedding)
    {
        if (embedding == null || embedding.Length == 0)
            throw new ArgumentException("Embedding cannot be empty");

        Embedding = embedding;
    }
}