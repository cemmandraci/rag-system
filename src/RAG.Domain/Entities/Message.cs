namespace RAG.Domain.Entities;

public class Message
{
    public Guid Id { get; private set; }
    public Guid ConversationId { get; private set; }
    public MessageRole Role { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public IReadOnlyList<Guid> SourceChunkIds { get; private set; } = [];
    public DateTime CreatedAt { get; private set; }
    
    private Message() { }

    public static Message Create(
        Guid conversationId,
        MessageRole role,
        string content,
        IReadOnlyList<Guid>? sourceChunkIds = null)
    {
        return new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            Role = role,
            Content = content,
            SourceChunkIds = sourceChunkIds ?? [],
            CreatedAt = DateTime.UtcNow
        };
    }
}

public enum MessageRole
{
    User,
    Assistant
}