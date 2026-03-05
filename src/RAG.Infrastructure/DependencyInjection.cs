using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Qdrant.Client;
using RAG.Application.Services;
using RAG.Domain.Interfaces;
using RAG.Infrastructure.Repositories;
using RAG.Infrastructure.Services;

namespace RAG.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient("Gemini", client =>
        {
            client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
            client.Timeout = TimeSpan.FromSeconds(60);
        });
        
        // Qdrant
        var qdrantHost = configuration["Qdrant:Host"] ?? "localhost";
        var qdrantPort = int.Parse(configuration["Qdrant:Port"] ?? "6334");
        services.AddSingleton(new QdrantClient(qdrantHost, qdrantPort));
        
        //Repositories
        services.AddSingleton<IDocumentRepository, InMemoryDocumentRepository>();
        services.AddSingleton<IConversationRepository, InMemoryConversationRepository>();
        services.AddScoped<IVectorRepository, QdrantVectorRepository>();
        
        //Services
        services.AddScoped<IEmbeddingService, GeminiEmbeddingService>();
        services.AddScoped<ILanguageModelService, GeminiLanguageModelService>();
        
        // Application Services
        services.AddSingleton(new ChunkingService(chunkSize: 500, overlapSize: 50));
        services.AddSingleton<ContextBuilderService>();
        
        return services;
    }
        
}