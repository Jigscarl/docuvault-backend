using Docuvault.Api.DTOs;
using Docuvault.Api.Data;
using Docuvault.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Docuvault.Api.Services;

public class FolderService : IFolderService
{
    private readonly DocuVaultDbContext _context;
    public FolderService(DocuVaultDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FolderResponseDto>>GetAllAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return  await _context.Folders.AsNoTracking().Where(f => f.UserId == userId).OrderBy(f => f.Name).Select(f => new FolderResponseDto
        {
            Id = f.Id,
            Name = f.Name,
            DocumentCount = f.Documents.Count(d => d.DeletedAt == null), CreatedAt = f.CreatedAt, UpdatedAt = f.UpdatedAt
        }).ToListAsync(cancellationToken);
    }
    public async Task<FolderResponseDto> CreateAsync(
        string name, Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var normalizedName = name.Trim();
        var exists = await _context.Folders.AnyAsync(f => f.UserId == userId && f.Name.ToLower() == normalizedName.ToLower(), cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("A folder with this name already exists.");
        }
        var folder = new Folder
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = normalizedName,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _context.Folders.Add(folder);

        await _context.SaveChangesAsync(cancellationToken);
        return new FolderResponseDto
        {
            Id = folder.Id,
            Name = folder.Name,
            DocumentCount = 0,
            CreatedAt = folder.CreatedAt,
            UpdatedAt = folder.UpdatedAt
        };
    }

    public async Task<FolderResponseDto?> RenameAsync(
        Guid folderId,
        string name,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var folder = await _context.Folders.FirstOrDefaultAsync(
            f => f.Id == folderId && f.UserId == userId, cancellationToken
        );
        if(folder == null)
        return null;

        var normalizedName = name.Trim();
        var duplicate = await _context.Folders.AnyAsync(
            f => f.UserId == userId && f.Id != folderId && f.Name.ToLower() == normalizedName.ToLower(), cancellationToken
        );

        if (duplicate)
        {
            throw new InvalidOperationException("A folder with this name already exists.");
        }

        folder.Name = normalizedName;
        folder.UpdatedAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return new FolderResponseDto
        {
            Id = folder.Id,
            Name = folder.Name,
            DocumentCount = await _context.Documents.CountAsync(
                d => d.FolderId == folder.Id && d.DeletedAt == null, cancellationToken
            ),
            CreatedAt = folder.CreatedAt,
            UpdatedAt = folder.UpdatedAt
        };
    }

    public async Task<bool> DeleteAsync(
        Guid folderId,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var folder = await _context.Folders.FirstOrDefaultAsync(
            f => f.Id == folderId && f.UserId == userId, cancellationToken
        );

        if (folder == null)
        return false;
        _context.Folders.Remove(folder);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

}