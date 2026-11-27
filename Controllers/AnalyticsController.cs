using Docuvault.Api.DTOs;
using Docuvault.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Docuvault.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]

public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardAnalyticsDto>>GetDashboardAnalytics(CancellationToken cancellationToken)
    {
        var UserIdValue = User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(UserIdValue, out var userId))
        {
            return Unauthorized();
        }
        var analytics = await _analyticsService.GetDashboardAnalyticsAsync(
            userId,
            cancellationToken
        );

        return Ok(analytics);
    }
}