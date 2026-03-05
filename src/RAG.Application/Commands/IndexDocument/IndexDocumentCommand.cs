using MediatR;
using RAG.Application.DTOs;

namespace RAG.Application.Commands.IndexDocument;

public record IndexDocumentCommand(string Title, string Content) : IRequest<IndexDocumentDto>;