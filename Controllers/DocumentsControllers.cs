using Docuvault.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Docuvault.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }
    [HttpGet]
    public IActionResult GetAll()
    {
        var userId = GetDevelopmentUserId();
        var documents = _documentService
            .GetAllAsync(userId)
            .GetAwaiter()
            .GetResult();

        return Ok(documents);
    }
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetDevelopmentUserId();
        var document = await _documentService.GetByIdAsync(id, userId);

        if(document == null)
        return NotFound(new {message = "Document not found."});
        return Ok(document);
    }
    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile file,
        [FromForm] string category,
        [FromForm] string? description
    )
    {
        var userId = GetDevelopmentUserId();

        if (file == null || file.Length == 0)
           return BadRequest(new {message = "Please select a valid file."});
        try
        {
            var document = await _documentService.UploadAsync(file,category,description, userId);
            return CreatedAtAction(
                nameof(GetById),
                new {id = document.Id},
                document
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new {message = ex.Message});
        }
    }
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> MoveToTrash(Guid id)
    {
        var userId = GetDevelopmentUserId();
        var result = await _documentService.MoveToTrashAsync(id, userId);
        if (!result)
            return NotFound(new { message = "Document not found." });

        return Ok(new { message = "Document Moved to trash." });
    }

    [HttpPatch("{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id)
    {
        var userId = GetDevelopmentUserId();
        var result = await _documentService.RestoreAsync(id,userId);
        if(!result)
        return NotFound(new {message = "Deleted Document not found."});
        return Ok(new {message = "Document restored successfully."});
    }

    [HttpDelete("{id:guid}/permanent")]
    public async Task<IActionResult> PermanentlyDelete(Guid id)
    {
        var userId = GetDevelopmentUserId();
        var result = await _documentService.PermanentlyDeleteAsync(id, userId);
        if(!result)
        return NotFound(new {message = "Document not found."});
        return Ok(new {message = "Document permanently deleted."});
    }
    private static Guid GetDevelopmentUserId()
    {
        return Guid.Parse("11111111-1111-1111-1111-111111111111");
    }
}