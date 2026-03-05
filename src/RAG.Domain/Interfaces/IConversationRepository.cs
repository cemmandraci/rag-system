using RAG.Domain.Entities;

namespace RAG.Domain.Interfaces;

public interface IConversationRepository
{
    Task<Conversation> AddAsync(Conversation conversation, CancellationToken cancellationToken = default);
    Task<Conversation> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Conversation conversation, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Conversation>> GetAllAsync(CancellationToken cancellationToken = default);
}