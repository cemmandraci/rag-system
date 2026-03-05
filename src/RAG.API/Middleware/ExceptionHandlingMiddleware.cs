using System.Net;
using System.Text.Json;
using RAG.Application.Exceptions;

namespace RAG.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DocumentNotFoundException ex)
        {
            _logger.LogWarning("Document not found: {Message}", ex.Message);
            await WriteErrorResponse(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (ConversationNotFoundException ex)
        {
            _logger.LogWarning("Conversation not found: {Message}", ex.Message);
            await WriteErrorResponse(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("429"))
        {
            _logger.LogWarning("Gemini API rate limit exceeded");
            await WriteErrorResponse(context, HttpStatusCode.TooManyRequests,
                "Rate limit exceeded. Please wait a moment and try again.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "External service call failed");
            await WriteErrorResponse(context, HttpStatusCode.ServiceUnavailable,
                "An external service is unavailable. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred");
            await WriteErrorResponse(context, HttpStatusCode.InternalServerError,
                "An unexpected error occurred.");
        }
    }

    private static async Task WriteErrorResponse(
        HttpContext context,
        HttpStatusCode statusCode,
        string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        var response = new { error = message, statusCode = (int)statusCode };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}