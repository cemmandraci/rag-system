using MediatR;
using RAG.Application.DTOs;

namespace RAG.Application.Queries.GetConversation;

public record GetConversationQuery(Guid ConversationId) : IRequest<ConversationDto>;