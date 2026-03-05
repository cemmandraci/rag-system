using MediatR;
using RAG.Application.DTOs;

namespace RAG.Application.Commands.CreateConversation;

public record CreateConversationCommand() : IRequest<ConversationDto>;