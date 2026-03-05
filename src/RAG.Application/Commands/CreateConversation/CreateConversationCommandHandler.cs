using MediatR;
using RAG.Application.DTOs;
using RAG.Domain.Entities;
using RAG.Domain.Interfaces;

namespace RAG.Application.Commands.CreateConversation;

public class CreateConversationCommandHandler : IRequestHandler<CreateConversationCommand, ConversationDto>
{
    private readonly IConversationRepository _conversationRepository;

    public CreateConversationCommandHandler(IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task<ConversationDto> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        var conversation = Conversation.Create();
        await _conversationRepository.AddAsync(conversation, cancellationToken);

        return new ConversationDto
        {
            Id = conversation.Id,
            Title = conversation.Title,
            Messages = [],
            CreatedAt = conversation.CreatedAt
        };
    }
}