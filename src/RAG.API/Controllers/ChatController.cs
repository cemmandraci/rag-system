using MediatR;
using Microsoft.AspNetCore.Mvc;
using RAG.API.Models;
using RAG.Application.Commands.CreateConversation;
using RAG.Application.DTOs;
using RAG.Application.Queries.Chat;
using RAG.Application.Queries.GetConversation;

namespace RAG.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost("conversations")]
    [ProducesResponseType(typeof(ConversationDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateConversation(CancellationToken cancellationToken)
    {
        var command = new CreateConversationCommand();
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
    
    [HttpPost("conversations/{conversationId:guid}/messages")]
    [ProducesResponseType(typeof(ChatResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Chat(
        Guid conversationId,
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        var query = new ChatQuery(conversationId, request.Question);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
    
    [HttpGet("conversations/{conversationId:guid}")]
    [ProducesResponseType(typeof(ConversationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConversation(
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        var query = new GetConversationQuery(conversationId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}