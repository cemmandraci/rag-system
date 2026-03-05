using MediatR;
using Microsoft.AspNetCore.Mvc;
using RAG.API.Models;
using RAG.Application.Commands.IndexDocument;
using RAG.Application.DTOs;

namespace RAG.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController :ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(IndexDocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> IndexDocument(
        [FromBody] IndexDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new IndexDocumentCommand(request.Title, request.Content);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}