using Docuvault.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Docuvault.Api.Controllers;

[Authorize(Policy ="AdminOnly")]
[ApiController]
[Route("api/[controller]")]

public class AdminDashboardController : ControllerBase
{
    private readonly IAdminDashboardService _dashboardService;

    public AdminDashboardController(IAdminDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var stats = await _dashboardService.GetDashboardStatsAsync();

        return Ok(stats);
    }

    [HttpGet("document-activity")]
    public async Task<IActionResult>GetDocumentActivity(
        [FromQuery] int days = 7
    )
    {
        if (days < 1 || days > 365)
        {
            return BadRequest(new
            {
                message = "Days must be between 1 and 365."
            });
        }
        var activity = await _dashboardService.GetDocumentActivityAsync(days);

        return Ok(activity);
    }

    [HttpGet("recent-activity")]
    public async Task<IActionResult> GetRecentActivity([FromQuery] int limit = 10)
    {
        if (limit < 1 || limit > 100)
        {
            return BadRequest(new
            {
                message = "Limit must be between 1 and 100."
            });
        }

        var activity = await _dashboardService.GetRecentActivityAsync(limit);
        return Ok(activity);
    }
    
}