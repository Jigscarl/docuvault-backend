using Docuvault.Api.Data;
using Docuvault.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Docuvault.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly DocuVaultDbContext _context;
    public ProfileController(DocuVaultDbContext context)
    {
        _context = context;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userIdValue = User.FindFirst("sub")?.Value;
        if(!Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }
        var user = await _context.Users.AsNoTracking().Where(u => u.Id == userId).Select(u  => new
        {
            u.Id,
            u.Email,
            u.FullName,
            u.Role,
            u.IsActive
        } ).FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound(new
            {
                message = "User profile not found."
            });
        }
        if (!user.IsActive)
        {
            return StatusCode(403, new{message = "Your account is inactive."});
        }

        return Ok(user);
    }

    [HttpPost("me")]
    public async Task<IActionResult> SyncMyProfile(
        [FromBody] SyncProfileRequest request,
        CancellationToken cancellationToken
    )
    {
        var userIdValue = User.FindFirst("sub")?.Value;
        var email = User.FindFirst("email")?.Value ?? User.FindFirst(ClaimTypes.Email)?.Value;
        if (!Guid.TryParse(userIdValue, out var userId) || string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized();
        }

        var fullName = request.FullName?.Trim();
        if (fullName?.Length > 150)
        {
            return BadRequest(new { message = "Full name cannot exceed 150 characters." });
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            user = new Models.User
            {
                Id = userId,
                Email = email,
                FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName,
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
        }
        else
        {
            if (!user.IsActive)
            {
                return StatusCode(403, new { message = "Your account is inactive." });
            }

            user.Email = email;
            if (!string.IsNullOrWhiteSpace(fullName))
            {
                user.FullName = fullName;
            }
            user.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Ok(new
        {
            user.Id,
            user.Email,
            user.FullName,
            user.Role,
            user.IsActive
        });
    }
}