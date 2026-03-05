namespace RAG.Infrastructure.Models;

// Embedding modelleri
public class GeminiEmbeddingResponse
{
    public EmbeddingValues Embedding { get; init; } = new();
}

public class EmbeddingValues
{
    public float[] Values { get; init; } = [];
}

public class GeminiBatchEmbeddingResponse
{
    public List<EmbeddingValues> Embeddings { get; init; } = [];
}

// Chat modelleri
public class GeminiChatRequest
{
    public List<GeminiContent> Contents { get; init; } = [];
    public GeminiSystemInstruction? SystemInstruction { get; init; }
}

public class GeminiSystemInstruction
{
    public GeminiPart Parts { get; init; } = new();
}

public class GeminiContent
{
    public string Role { get; init; } = string.Empty;
    public List<GeminiPart> Parts { get; init; } = [];
}

public class GeminiPart
{
    public string Text { get; init; } = string.Empty;
}

public class GeminiChatResponse
{
    public List<GeminiCandidate> Candidates { get; init; } = [];
}

public class GeminiCandidate
{
    public GeminiContent Content { get; init; } = new();
}

// Streaming modelleri
public class GeminiStreamChunk
{
    public List<GeminiCandidate> Candidates { get; init; } = [];
}