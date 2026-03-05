namespace RAG.Application.Exceptions;

public class DocumentNotFoundException : Exception
{
    public DocumentNotFoundException(Guid id) : base($"Document with id {id} was not found") {}
}