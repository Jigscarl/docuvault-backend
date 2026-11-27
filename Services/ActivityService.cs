using Docuvault.Api.Data;
using Docuvault.Api.Models;
using Docuvault.Api.DTOs;
using Docuvault.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Docuvault.Api.Services;

public class ActivityService : IActivityService
{
    private readonly DocuVaultDbContext _context;
    public ActivityService(DocuVaultDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(
        Guid userId,
        Guid? documentId,
        string documentName,
        string action,
        string? details = null,
        CancellationToken cancellationToken = default
    )
    {
        var activity = new DocumentActivity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DocumentId = documentId,
            DocumentName = documentName,
            Action = action,
            Details = details,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.DocumentActivities.Add(activity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<ActivityResponseDto>>
    GetRecentAsync(
        Guid userId,
        int limit = 20,
        CancellationToken cancellationToken = default
    )
    {
        limit = Math.Clamp(limit, 1, 100);
        return await _context.DocumentActivities.AsNoTracking().Where(a => a.UserId == userId).OrderByDescending(a => a.CreatedAt).Take(limit).Select(a => new ActivityResponseDto
        {
            Id = a.Id,
            DocumentId = a.DocumentId,
            DocumentName = a.DocumentName,
            Action = a.Action,
            Details = a.Details,
            CreatedAt = a.CreatedAt
        }).ToListAsync(cancellationToken);
    }

    public async Task<ActivitySummaryDto> GetSummaryAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var userActivities = _context.DocumentActivities.AsNoTracking().Where(a => a.UserId == userId);
        var summary = new ActivitySummaryDto
        {
            TotalActivities = await userActivities.CountAsync(cancellationToken),
            TotalUploads =  await  userActivities.CountAsync(a => a.Action == "Uploaded", cancellationToken),
            TotalDownloads = await userActivities.CountAsync(a => a.Action == "Download Link Created", cancellationToken),
            TotalFavorites = await userActivities.CountAsync(a => a.Action == "Favorited",cancellationToken),
            TotalTrashActions = await userActivities.CountAsync(a => a.Action == "Moved to Trash", cancellationToken)
        };
        return summary;
    }
}