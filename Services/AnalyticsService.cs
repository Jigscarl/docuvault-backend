using System.Text.RegularExpressions;
using Docuvault.Api.Data;
using Docuvault.Api.DTOs;
using Docuvault.Api.Services;
using Microsoft.EntityFrameworkCore;

public class AnalyticsService : IAnalyticsService
{
    private readonly DocuVaultDbContext _context;
    public AnalyticsService(DocuVaultDbContext context)
    {
        _context = context;
    }
    public async Task<DashboardAnalyticsDto> GetDashboardAnalyticsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var documentsQuery = _context.Documents.AsNoTracking().Where(d => d.UserId == userId && d.DeletedAt == null);
        var totalDocuments = await documentsQuery.CountAsync(cancellationToken);
        var totalFavorites = await documentsQuery.CountAsync(d => d.IsFavorite, cancellationToken);
        var totalStorage = await documentsQuery.SumAsync(d => (long?) d.SizeBytes, cancellationToken) ?? 0;
        var totalFolders = await _context.Folders.AsNoTracking().CountAsync(f => f.UserId == userId, cancellationToken);
        var categories = await documentsQuery.GroupBy(d => d.Category).Select(group => new CategoryAnalyticsDto
        {
            Category = group.Key ?? "Uncategorized",
            Count = group.Count()
        }).ToListAsync(cancellationToken);

        var fileInformation = await documentsQuery.Select(d => new {d.Name, d.SizeBytes}).ToListAsync(cancellationToken);
        var fileTypes = fileInformation.GroupBy(d => GetExtension(d.Name)).Select(group => new FileTypeAnalyticsDto{
            Extension = group.Key,
            Count = group.Count(),
            TotalBytes = group.Sum(d =>(long)d.SizeBytes)
        }).OrderByDescending(group => group.TotalBytes).ToList();

        var today = DateTime.UtcNow.Date;
        var startDate = today.AddDays(-6);
        var dailyUploadRecords = await documentsQuery.Where(d => d.CreatedAt >= startDate).Select(d => d.CreatedAt).ToListAsync(cancellationToken);
        var dailyUploads = Enumerable.Range(0, 7).Select(offset =>
        {
            var date = startDate.AddDays(offset);
            var count =dailyUploadRecords.Count(createdAt => createdAt.Date == date);
            return new DailyUploadAnalyticsDto
            {
                Date = date,
                Count = count
            };
        }).ToList();
        return new DashboardAnalyticsDto
        {
            TotalDocuments = totalDocuments,
            TotalFolders = totalFolders,
            TotalFavorites = totalFavorites,
            TotalStorageBytes = totalStorage,
            Categories = categories,
            FileTypes = fileTypes,
            dailyUploads = dailyUploads
        };
    }
    private static string GetExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension))
        return "OTHER";
        return extension.TrimStart('.').ToUpperInvariant();
    }
}