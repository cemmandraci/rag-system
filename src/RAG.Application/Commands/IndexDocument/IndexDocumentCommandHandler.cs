using MediatR;
using RAG.Application.DTOs;
using RAG.Application.Services;
using RAG.Domain.Entities;
using RAG.Domain.Interfaces;

namespace RAG.Application.Commands.IndexDocument;

public class IndexDocumentCommandHandler : IRequestHandler<IndexDocumentCommand, IndexDocumentDto>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IVectorRepository _vectorRepository;
    private readonly IEmbeddingService _embeddingService;
    private readonly ChunkingService _chunkingService;

    public IndexDocumentCommandHandler(IDocumentRepository documentRepository, IVectorRepository vectorRepository, IEmbeddingService embeddingService, ChunkingService chunkingService)
    {
        _documentRepository = documentRepository;
        _vectorRepository = vectorRepository;
        _embeddingService = embeddingService;
        _chunkingService = chunkingService;
    }

    public async Task<IndexDocumentDto> Handle(IndexDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = Document.Create(request.Title, request.Content);
        await _documentRepository.AddAsync(document, cancellationToken);

        try
        {
            document.MarkAsProcessing();
            await _documentRepository.UpdateAsync(document, cancellationToken);
            
            var chunkTexts = _chunkingService.Chunk(request.Content);
            
            var embeddings = await _embeddingService.GenerateEmbeddingsAsync(chunkTexts, cancellationToken);

            var chunks = chunkTexts.Select((text, index) =>
            {
                var chunk = DocumentChunk.Create(
                    document.Id,
                    document.Title,
                    text,
                    index);
                chunk.SetEmbedding(embeddings[index]);
                return chunk;
            }).ToList();
            
            await _vectorRepository.UpsertChunkAsync(chunks, cancellationToken);
            
            document.MarkAsCompleted(chunks.Count);
            await _documentRepository.UpdateAsync(document, cancellationToken);
        }
        catch (Exception e)
        {
           document.MarkAsFailed();
           await _documentRepository.UpdateAsync(document, cancellationToken);
           throw;
        }

        return new IndexDocumentDto
        {
            DocumentId = document.Id,
            Title = document.Title,
            ChunkCount = document.ChunkCount,
            Status = document.Status.ToString()
        };
    }
}