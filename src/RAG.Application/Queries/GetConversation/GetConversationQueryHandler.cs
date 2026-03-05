using MediatR;
using RAG.Application.DTOs;
using RAG.Application.Exceptions;
using RAG.Domain.Interfaces;

namespace RAG.Application.Queries.GetConversation;

public class GetConversationQueryHandler : IRequestHandler<GetConversationQuery, ConversationDto>
{
    private readonly IConversationRepository _conversationRepository;

    public GetConversationQueryHandler(IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task<ConversationDto> Handle(GetConversationQuery request, CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken)
                           ?? throw new ConversationNotFoundException(request.ConversationId);

        return new ConversationDto
        {
            Id = conversation.Id,
            Title = conversation.Title,
            Messages = conversation.Messages.Select(m => new MessageDto
            {
                Id = m.Id,
                Role = m.Role.ToString().ToLower(),
                Content = m.Content,
                CreatedAt = m.CreatedAt
            }).ToList(),
            CreatedAt = conversation.CreatedAt
        };
    }
}