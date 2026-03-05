using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RAG.Domain.Interfaces;
using RAG.Infrastructure.Models;

namespace RAG.Infrastructure.Services;

public class GeminiEmbeddingService : IEmbeddingService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiKey;
    private readonly JsonSerializerOptions _jsonOptions;
    private const string EmbeddingModel = "gemini-embedding-001";


    public GeminiEmbeddingService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _apiKey = configuration["Gemini:ApiKey"]
            ?? throw new InvalidOperationException("Gemini Embedding API Key is not configured");
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
       var client = _httpClientFactory.CreateClient("Gemini");

       var requestBody = new
       {
           model = $"models/{EmbeddingModel}",
           content = new { parts = new[] { new { text } } }
       };
       
       var json = JsonSerializer.Serialize(requestBody, _jsonOptions);
       var content = new StringContent(json,Encoding.UTF8, "application/json");
       
       var url = $"v1beta/models/{EmbeddingModel}:embedContent?key={_apiKey}";
       var response = await client.PostAsync(url, content, cancellationToken);
       response.EnsureSuccessStatusCode();
       
       var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
       var result = JsonSerializer.Deserialize<GeminiEmbeddingResponse>(responseBody, _jsonOptions)
                    ?? throw new InvalidOperationException("Could not parse embedding response.");

       return result.Embedding.Values;
    }

    public async Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Gemini");

        var requests = texts.Select(text => new
        {
            model = $"models/{EmbeddingModel}",
            content = new { parts = new[] { new { text } } }
        });

        var requestBody = new { requests };
        var json = JsonSerializer.Serialize(requestBody, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var url = $"v1beta/models/{EmbeddingModel}:batchEmbedContents?key={_apiKey}";
        var response = await client.PostAsync(url, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<GeminiBatchEmbeddingResponse>(responseBody, _jsonOptions)
                     ?? throw new InvalidOperationException("Could not parse batch embedding response.");

        return result.Embeddings.Select(e => e.Values).ToList();
    }
}