using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RAG.Domain.Interfaces;
using RAG.Infrastructure.Models;

namespace RAG.Infrastructure.Services;

public class GeminiLanguageModelService : ILanguageModelService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiKey;
    private readonly JsonSerializerOptions _jsonOptions;
    private const string ChatModel = "gemini-2.0-flash";

    public GeminiLanguageModelService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _apiKey = configuration["Gemini:ApiKey"]
                  ?? throw new InvalidOperationException("Gemini API key is not configured.");
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<string> GenerateResponseAsync(
        string systemPrompt,
        IReadOnlyList<(string Role, string Content)> conversationHistory,
        string userMessage,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Gemini");

        var contents = BuildContents(conversationHistory, userMessage);
        
        var requestBody = new GeminiChatRequest
        {
            SystemInstruction = new GeminiSystemInstruction
            {
                Parts = new GeminiPart { Text = systemPrompt }
            },
            Contents = contents
        };

        var json = JsonSerializer.Serialize(requestBody, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var url = $"v1beta/models/{ChatModel}:generateContent?key={_apiKey}";
        var response = await client.PostAsync(url, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<GeminiChatResponse>(responseBody, _jsonOptions)
                     ?? throw new InvalidOperationException("Could not parse chat response.");

        return result.Candidates.FirstOrDefault()?.Content.Parts.FirstOrDefault()?.Text
               ?? throw new InvalidOperationException("No response from Gemini.");
    }

    public async IAsyncEnumerable<string> GenerateResponseStreamAsync(
        string systemPrompt,
        IReadOnlyList<(string Role, string Content)> conversationHistory,
        string userMessage,
        [EnumeratorCancellation]CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Gemini");

        var contents = BuildContents(conversationHistory, userMessage);

        var requestBody = new GeminiChatRequest
        {
            SystemInstruction = new GeminiSystemInstruction
            {
                Parts = new GeminiPart { Text = systemPrompt }
            },
            Contents = contents
        };

        var json = JsonSerializer.Serialize(requestBody, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var url = $"v1beta/models/{ChatModel}:streamGenerateContent?alt=sse&key={_apiKey}";
        var response = await client.PostAsync(url, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync(cancellationToken);

            // SSE format: "data: {...json...}"
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                continue;

            var jsonData = line["data: ".Length..];

            if (jsonData == "[DONE]")
                break;

            GeminiStreamChunk? chunk;
            try
            {
                chunk = JsonSerializer.Deserialize<GeminiStreamChunk>(jsonData, _jsonOptions);
            }
            catch
            {
                continue;
            }

            var text = chunk?.Candidates.FirstOrDefault()
                ?.Content.Parts.FirstOrDefault()?.Text;

            if (!string.IsNullOrEmpty(text))
                yield return text;
        }
    }
    
    private static List<GeminiContent> BuildContents(
        IReadOnlyList<(string Role, string Content)> conversationHistory,
        string userMessage)
    {
        var contents = conversationHistory.Select(m => new GeminiContent
        {
            Role = m.Role,
            Parts = [new GeminiPart { Text = m.Content }]
        }).ToList();

        // Yeni kullanıcı mesajını ekle
        contents.Add(new GeminiContent
        {
            Role = "user",
            Parts = [new GeminiPart { Text = userMessage }]
        });

        return contents;
    }
}