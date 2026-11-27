using Docuvault.Api.DTOs;
namespace Docuvault.Api.Services;
public interface IAnalyticsService
{
    Task<DashboardAnalyticsDto> GetDashboardAnalyticsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
}