using Docuvault.Api.DTOs;
namespace Docuvault.Api.Services;

public interface IAdminDashboardService
{
    Task<AdminDashboardDto> GetDashboardStatsAsync();
    Task<List<AdminDocumentActivityDto>> GetDocumentActivityAsync( int days = 7);
    Task<List<AdminRecentActivityDto>> GetRecentActivityAsync(int limit = 10);
}