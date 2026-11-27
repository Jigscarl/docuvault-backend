using Docuvault.Api.DTOs;
using Docuvault.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Docuvault.Api.Controllers;
[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/[controller]")]

public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAllUsers(CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllUsersAsync(cancellationToken);
        return Ok(users);
    }
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetUserById(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var users = await _userService.GetUserByIdAsync(id, cancellationToken);
        if(users == null) return NotFound(new {message = "User not found."});
        return Ok(users);
    }

    [HttpPatch("{id:guid}/role")]
    public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] UpdateUserRoleDto request, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _userService.UpdateUserRoleAsync(id, request.Role, cancellationToken);
            if (!updated) return NotFound(new {message = "User not found"});
            return  Ok (new {message = "User Role updated succeesfully."});
        } catch (ArgumentException ex)
        {
            return BadRequest(new {message = ex.Message});
        }
    }
}