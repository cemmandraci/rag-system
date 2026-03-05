namespace RAG.Application.Exceptions;

public class ConversationNotFoundException : Exception
{
    public ConversationNotFoundException(Guid id) : base($"Conversation with id {id} was not found") {}
}