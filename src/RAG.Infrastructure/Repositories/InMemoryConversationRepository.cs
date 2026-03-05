using System.Collections.Concurrent;
using RAG.Domain.Entities;
using RAG.Domain.Interfaces;

namespace RAG.Infrastructure.Repositories;

public class InMemoryConversationRepository : IConversationRepository
{
    private readonly ConcurrentDictionary<Guid, Conversation> _conversations = new();
    
    public Task<Conversation> AddAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        _conversations[conversation.Id] = conversation;
        return Task.FromResult(conversation);
    }

    public Task<Conversation> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _conversations.TryGetValue(id, out var conversation);
        return Task.FromResult(conversation);
    }

    public Task UpdateAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        _conversations[conversation.Id] = conversation;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Conversation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Conversation> result = _conversations.Values.ToList();
        return Task.FromResult(result);
    }
}