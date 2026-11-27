using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Docuvault.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Docuvault.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ActivitiesController : ControllerBase
{
    private readonly IActivityService _activityService;
    public ActivitiesController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(User.FindFirstValue("sub"), out var userId))
        {
            return Unauthorized(new {message = "Invalid user identity."});
        }
        var summary = await _activityService.GetSummaryAsync(
            userId,
            cancellationToken
        );

        return Ok(summary);
    }

    [HttpGet]
    public async Task<IActionResult> GetRecent(
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default
    )
    {
        if (!Guid.TryParse(User.FindFirstValue("sub"), out var userId))
        {
            return Unauthorized(new {message = "Invalid user identity"});
        }
        var activities = await _activityService.GetRecentAsync(userId,limit,cancellationToken);
        return Ok(activities);
    }
}