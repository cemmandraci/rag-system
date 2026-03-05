using RAG.Domain.Entities;

namespace RAG.Domain.Interfaces;

public interface IDocumentRepository
{
    Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default);
    Task<Document> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Document document, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken cancellationToken = default);
}