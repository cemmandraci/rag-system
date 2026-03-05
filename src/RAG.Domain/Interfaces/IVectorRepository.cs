using RAG.Domain.Entities;

namespace RAG.Domain.Interfaces;

public interface IVectorRepository
{
    Task EnsureCollectionExistsAsync(CancellationToken cancellationToken = default);
    Task UpsertChunkAsync(IReadOnlyList<DocumentChunk> chunks, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentChunk>> SearchAsync(float[] queryEmbedding, int limit = 5, CancellationToken cancellationToken = default);
    Task DeleteByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
}