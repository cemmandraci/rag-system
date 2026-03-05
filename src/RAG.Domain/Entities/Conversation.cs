namespace RAG.Domain.Entities;

public class Conversation
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public IReadOnlyList<Message> Messages => _messages.AsReadOnly();
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    
    private readonly List<Message> _messages = new();
    
    private Conversation() { }

    public static Conversation Create(string title = "New Conversation")
    {
        return new Conversation
        {
            Id = Guid.NewGuid(),
            Title = title,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
    
    public void AddMessage(MessageRole role, string content, IReadOnlyList<Guid>? sourceChunkIds = null)
    {
        var message = Message.Create(Id, role, content, sourceChunkIds);
        _messages.Add(message);
        UpdatedAt = DateTime.UtcNow;

        // İlk kullanıcı mesajından conversation title oluştur
        if (_messages.Count == 1 && role == MessageRole.User)
        {
            Title = content.Length > 50
                ? content[..50] + "..."
                : content;
        }
    }

    public IReadOnlyList<Message> GetRecentMessages(int count = 10)
    {
        return _messages.TakeLast(count).ToList();
    }
}