namespace RAG.Domain.Interfaces;

public interface IEmbeddingService
{
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<float[]>> GenerateEmbeddingAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default);
}