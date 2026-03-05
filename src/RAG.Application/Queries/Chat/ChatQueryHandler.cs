using MediatR;
using RAG.Application.DTOs;
using RAG.Application.Exceptions;
using RAG.Application.Services;
using RAG.Domain.Entities;
using RAG.Domain.Interfaces;

namespace RAG.Application.Queries.Chat;

public class ChatQueryHandler : IRequestHandler<ChatQuery, ChatResponseDto>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IVectorRepository _vectorRepository;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILanguageModelService _languageModelService;
    private readonly ContextBuilderService _contextBuilderService;


    public ChatQueryHandler(
        IConversationRepository conversationRepository,
        IVectorRepository vectorRepository,
        IEmbeddingService embeddingService,
        ILanguageModelService languageModelService, 
        ContextBuilderService contextBuilderService)
    {
        _conversationRepository = conversationRepository;
        _vectorRepository = vectorRepository;
        _embeddingService = embeddingService;
        _languageModelService = languageModelService;
        _contextBuilderService = contextBuilderService;
    }

    public async Task<ChatResponseDto> Handle(ChatQuery request, CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken)
                           ?? throw new ConversationNotFoundException(request.ConversationId);

        var questionEmbedding = await _embeddingService.GenerateEmbeddingAsync(request.Question, cancellationToken);

        var relevantChunks = await _vectorRepository.SearchAsync(questionEmbedding, limit: 5, cancellationToken);

        var systemPrompt = _contextBuilderService.BuildSystemPrompt(relevantChunks);

        var recentMessages = conversation.GetRecentMessages(10);
        var history = _contextBuilderService.BuildConversationHistory(recentMessages);
        
        var answer = await _languageModelService.GenerateResponseAsync(systemPrompt, history, request.Question, cancellationToken);

        var sourceChunkIds = relevantChunks.Select(c => c.Id).ToList();
        conversation.AddMessage(MessageRole.User, request.Question);
        conversation.AddMessage(MessageRole.Assistant, answer, sourceChunkIds);
        await _conversationRepository.UpdateAsync(conversation, cancellationToken);

        var sources = relevantChunks.Select(c => new SourceDto
        {
            DocumentId = c.DocumentId,
            DocumentTitle = c.DocumentTitle,
            Content = c.Content,
            Score = 0
        }).ToList();

        return new ChatResponseDto
        {
            ConversationId = conversation.Id,
            Answer = answer,
            Sources = sources
        };

    }
}