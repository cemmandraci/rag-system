using System.Text;
using RAG.Domain.Entities;

namespace RAG.Application.Services;

public class ContextBuilderService
{
    private const int MaxContextChunks = 5;

    public string BuildSystemPrompt(IReadOnlyList<DocumentChunk> relevantChunks)
    {
        var contextBuilder = new StringBuilder();
        
        contextBuilder.AppendLine("You are a helpful assistant that answers questions based ONLY on the provided context.");
        contextBuilder.AppendLine("If the answer cannot be found in the context, say 'I don't have enough information to answer this question.'");
        contextBuilder.AppendLine("Do not use any prior knowledge outside of the provided context.");
        contextBuilder.AppendLine("Always be concise and accurate.");
        contextBuilder.AppendLine();
        contextBuilder.AppendLine("=== CONTEXT ===");
        
        var chunksToUse = relevantChunks.Take(MaxContextChunks).ToList();

        for (int i = 0; i < chunksToUse.Count; i++)
        {
            var chunk = chunksToUse[i];
            contextBuilder.AppendLine($"[Source {i + 1}: {chunk.DocumentTitle}]");
            contextBuilder.AppendLine(chunk.Content);
            contextBuilder.AppendLine();
        }

        contextBuilder.AppendLine("=== END OF CONTEXT ===");
        contextBuilder.AppendLine("Answer the user's question based solely on the context above.");
        
        return contextBuilder.ToString();
    }

    public IReadOnlyList<(string Role, string Content)> BuildConversationHistory(IReadOnlyList<Message> messages)
    {
        return messages.Select(m => (
            Role: m.Role == MessageRole.User ? "user" : "model",
            Content: m.Content
            )).ToList();
    }
}