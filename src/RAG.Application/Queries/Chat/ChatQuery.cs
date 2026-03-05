using MediatR;
using RAG.Application.DTOs;

namespace RAG.Application.Queries.Chat;

public record ChatQuery(Guid ConversationId, string Question) : IRequest<ChatResponseDto>;