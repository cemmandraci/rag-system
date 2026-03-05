namespace RAG.Application.Services;

public class ChunkingService
{
    private readonly int _chunkSize;
    private readonly int _overlapSize;

    public ChunkingService(int chunkSize, int overlapSize)
    {
        _chunkSize = chunkSize;
        _overlapSize = overlapSize;
    }

    public IReadOnlyList<string> Chunk(string text)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (words.Length <= _chunkSize)
            return [text];

        var chunks = new List<string>();
        var index = 0;

        while (index < words.Length)
        {
            var chunkWords = words.Skip(index).Take(_chunkSize);
            chunks.Add(string.Join(" ", chunkWords));
            index += _chunkSize - _overlapSize;
        }
        
        return chunks;
    }
}