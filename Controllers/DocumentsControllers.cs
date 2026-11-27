using Docuvault.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Docuvault.Api.DTOs;

namespace Docuvault.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        var documents = await _documentService
            .GetAllAsync(userId, cancellationToken);
        return Ok(documents);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        var document = await _documentService.GetByIdAsync(id, userId, cancellationToken);

        if (document == null)
            return NotFound(new { message = "Document not found." });
        return Ok(document);
    }

    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile file,
        [FromForm] string category,
        [FromForm] string? description,
        [FromForm] Guid? folderId,
        CancellationToken cancellationToken
    )
    {
        var userId = GetAuthenticatedUserId();

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Please select a valid file." });
        try
        {
            var result = await _documentService.UploadAsync(
                file,
                category,
                description,
                folderId,
                userId,
                cancellationToken

            );
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new {message = ex.Message});
        }
    }

    [HttpPatch("{id:guid}/favorite")]
    public async Task<IActionResult> ToggleFavorite(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        var result = await _documentService.ToggleFavoriteAsync(id, userId, cancellationToken);
        if (result == false)
            return NotFound(new { message = "Document not found." });

        return Ok(new { message = "Favorite status updated.", isFavorite = result });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> MoveToTrash(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        var result = await _documentService.MoveToTrashAsync(id, userId, cancellationToken);
        if (!result)
            return NotFound(new { message = "Document not found." });

        return Ok(new { message = "Document Moved to trash." });
    }

    [HttpPatch("{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        var result = await _documentService.RestoreAsync(id, userId, cancellationToken);
        if (!result)
            return NotFound(new { message = "Deleted Document not found." });
        return Ok(new { message = "Document restored successfully." });
    }

    [HttpDelete("{id:guid}/permanent")]
    public async Task<IActionResult> PermanentlyDelete(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        var result = await _documentService.PermanentlyDeleteAsync(id, userId, cancellationToken);
        if (!result)
            return NotFound(new { message = "Document not found." });
        return Ok(new { message = "Document permanently deleted." });
    }

    private Guid GetAuthenticatedUserId()
    {
        var userId = User.FindFirstValue("sub");

        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            throw new UnauthorizedAccessException("Invalid authenticated user identity.");
        }
        return parsedUserId;
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();

        var downloadUrl = await _documentService.GetDownloadUrlAsync(id, userId, cancellationToken);

        if (downloadUrl == null)
        {
            return NotFound(new { message = "Document not found" });
        }
        return Ok(new { url = downloadUrl, expiresInSeconds = 300 });
    }

    [HttpGet("trash")]

    public async Task<IActionResult> GetTrash(
        CancellationToken cancellationToken
    )
    {
        var userId = GetAuthenticatedUserId();
        var documents = await _documentService.GetTrashAsync(userId, cancellationToken);
        return Ok(documents);
    }

    [HttpPatch("{id:guid}/folder")]
    public async Task<IActionResult> MoveToFolder(
        Guid id,
        [FromBody] MoveDocumentRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = GetAuthenticatedUserId();
        try
        {
            var success = await _documentService.MoveToFolderAsync(
                id,
                request.FolderId, userId, cancellationToken
            );
            if (!success)
            {
                return NotFound(new{ message = "Document not found."});
            }
            return Ok(new
            {
                message = "Document moved successfully."
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

}