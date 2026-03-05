namespace RAG.Application.DTOs;

public class ChatResponseDto
{
    public Guid ConversationId { get; set; }
    public string Answer { get; set; } = string.Empty;
    public IReadOnlyList<SourceDto> Sources { get; set; } = [];
}

public class SourceDto
{
    public Guid DocumentId { get; set; }
    public string DocumentTitle { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public float Score { get; set; }
}