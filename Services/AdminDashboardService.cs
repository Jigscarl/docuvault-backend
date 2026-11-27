using Docuvault.Api.DTOs;
using Docuvault.Api.Data;
using Docuvault.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Docuvault.Api.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly DocuVaultDbContext _context;

    public AdminDashboardService(DocuVaultDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDashboardDto> GetDashboardStatsAsync()
    {
        var totalUsers = await _context.Users.CountAsync();
        var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
        var totalAdmins = await _context.Users.CountAsync(u => u.Role == "Admin");
        var totalRegularUsers = await _context.Users.CountAsync(u => u.Role == "User");
        var totalDocuments = await _context.Documents.CountAsync();
        var totalFolders = await _context.Folders.CountAsync();

        var totalStorageBytes = await _context.Documents.SumAsync(d => (long?)d.SizeBytes) ?? 0;

        return new AdminDashboardDto
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            InactiveUsers = totalUsers - activeUsers,
            TotalDocuments = totalDocuments,
            TotalStorageBytes = totalStorageBytes,
            TotalFolders = totalFolders,
            TotalAdmins = totalAdmins,
            TotalRegularUsers = totalRegularUsers
        };
    }

    public async Task<List<AdminDocumentActivityDto>> GetDocumentActivityAsync(int days = 7)
    {
        days = Math.Clamp(days, 1, 365);
        var startDate = DateTimeOffset.UtcNow.Date.AddDays(-(days - 1));
        var uploads = await _context.Documents
            .AsNoTracking()
            .Where(d => d.CreatedAt >= startDate)
            .Select(d => d.CreatedAt)
            .ToListAsync();

        var groupedUploads = uploads
            .GroupBy(createdAt => createdAt.UtcDateTime.Date)
            .ToDictionary(group => group.Key, group => group.Count());

        var activity = Enumerable.Range(0, days).Select(i =>
        {
            var date = startDate.AddDays(i);
            groupedUploads.TryGetValue(date, out var uploadCount);

            return new AdminDocumentActivityDto
            {
                Date = date,
                Uploads = uploadCount
            };
        }).ToList();
        return activity;
    }

    public async Task<List<AdminRecentActivityDto>> GetRecentActivityAsync(
        int limit = 10
    )
    {
        limit = Math.Clamp(limit, 1, 100);
        var activities = await (
            from activity in _context.DocumentActivities.AsNoTracking()
            join user in _context.Users.AsNoTracking()
                on activity.UserId equals user.Id
            orderby activity.CreatedAt descending
            select new AdminRecentActivityDto
            {
                Id = activity.Id,
                UserId = activity.UserId,
                UserName = user.FullName ?? user.Email,
                UserEmail = user.Email,
                Action = activity.Action,
                DocumentName = activity.DocumentName,
                Details = activity.Details,
                CreatedAt = activity.CreatedAt
            })
        .Take(limit)
        .ToListAsync();

        return activities;
    }
}