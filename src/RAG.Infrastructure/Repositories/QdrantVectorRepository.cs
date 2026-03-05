using Qdrant.Client;
using Qdrant.Client.Grpc;
using RAG.Domain.Entities;
using RAG.Domain.Interfaces;

namespace RAG.Infrastructure.Repositories;

public class QdrantVectorRepository : IVectorRepository
{
    private readonly QdrantClient _client;

    public QdrantVectorRepository(QdrantClient client)
    {
        _client = client;
    }

    private const string CollectionName = "rag_documents";
    private const ulong VectorSize = 3072;
    
    public async Task EnsureCollectionExistsAsync(CancellationToken cancellationToken = default)
    {
        var collections = await _client.ListCollectionsAsync(cancellationToken);
        var exists = collections.Any(c => c == CollectionName);

        if (!exists)
        {
            await _client.CreateCollectionAsync(
                CollectionName,
                new VectorParams
                {
                    Size = VectorSize,
                    Distance = Distance.Cosine
                },
                cancellationToken: cancellationToken
            );
        }
    }

    public async Task UpsertChunkAsync(IReadOnlyList<DocumentChunk> chunks, CancellationToken cancellationToken = default)
    {
        var points = chunks.Select(chunk =>
        {
            var point = new PointStruct
            {
                Id = (ulong)Math.Abs(chunk.Id.GetHashCode()),
                Vectors = chunk.Embedding!.ToArray()
            };

            point.Payload.Add("chunkId", chunk.Id.ToString());
            point.Payload.Add("documentId", chunk.DocumentId.ToString());
            point.Payload.Add("documentTitle", chunk.DocumentTitle);
            point.Payload.Add("content", chunk.Content);
            point.Payload.Add("chunkIndex", chunk.ChunkIndex);

            return point;
        }).ToList();

        await _client.UpsertAsync(CollectionName, points, cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentChunk>> SearchAsync(float[] queryEmbedding, int limit = 5, CancellationToken cancellationToken = default)
    {
        var results = await _client.SearchAsync(
            CollectionName,
            queryEmbedding,
            limit: (ulong)limit,
            cancellationToken: cancellationToken
        );

        return results.Select(r =>
        {
            var chunk = DocumentChunk.Create(
                Guid.Parse(r.Payload["documentId"].StringValue),
                r.Payload["documentTitle"].StringValue,
                r.Payload["content"].StringValue,
                (int)r.Payload["chunkIndex"].IntegerValue
            );
            return chunk;
        }).ToList();
    }

    public async Task DeleteByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        await _client.DeleteAsync(
            CollectionName,
            new Filter
            {
                Must =
                {
                    new Condition
                    {
                        Field = new FieldCondition
                        {
                            Key = "documentId",
                            Match = new Match { Keyword = documentId.ToString() }
                        }
                    }
                }
            },
            cancellationToken: cancellationToken
        );
    }
}