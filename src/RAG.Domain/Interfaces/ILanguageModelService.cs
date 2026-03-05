namespace RAG.Domain.Interfaces;

public interface ILanguageModelService
{
    Task<string> GenerateResponseAsync(
        string systemPrompt,
        IReadOnlyList<(string Role, string Content)> conversationHistory,
        string userMessage,
        CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> GenerateResponseStreamAsync(
        string systemPrompt,
        IReadOnlyList<(string Role, string Content)> conversationHistory,
        string userMessage,
        CancellationToken cancellationToken = default);
}