using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Docuvault.Api.DTOs;
using Docuvault.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Docuvault.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]

public class FoldersController : ControllerBase
{
    private readonly IFolderService _folderService;
    public FoldersController(IFolderService folderService)
    {
        _folderService = folderService;
    }

    private bool TryGetUserId(out Guid userId)
    {
        return Guid.TryParse(
            User.FindFirstValue("sub"),
            out userId
        );
    }
    [HttpGet]
    public async Task<IActionResult>GetAll(
        CancellationToken cancellationToken
    )
    {
        if(!TryGetUserId(out var userId))
        return Unauthorized();

        var folders = await _folderService.GetAllAsync(userId, cancellationToken);
        return Ok(folders);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateFolderRequest request, CancellationToken cancellationToken
    )
    {
        if(!TryGetUserId(out var userId))
        return Unauthorized();

        try
        {
            var folder = await _folderService.CreateAsync(request.Name, userId, cancellationToken);

            return CreatedAtAction(nameof(GetAll),new {id = folder});
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new {message = ex.Message});
        }
    }
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Rename(
        Guid id,
        [FromBody] CreateFolderRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        return Unauthorized();

        try
        {
            var folder = await _folderService.RenameAsync(
                id, request.Name, userId, cancellationToken
            );
            if(folder == null)
            return NotFound();
            return Ok(folder);
        }
        catch(InvalidOperationException ex)
        {
            return Conflict(new {message = ex.Message});
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult>Delete(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        if (!TryGetUserId(out var userId))
        return Unauthorized();
        var deleted = await _folderService.DeleteAsync(id,userId,cancellationToken);
        if(!deleted)
        return NotFound();
        return NoContent();
    }
}
