using Docuvault.Api.DTOs;
using Docuvault.Api.Models;
using Docuvault.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Docuvault.Api.Services;

public class DocumentService : IDocumentService
{
   private readonly DocuVaultDbContext _context;
   private readonly ISupabaseStorageService _storageService;
   private readonly IActivityService _activityService;

   private const long MaxFileSize = 10 * 1024 *1024;
   private static readonly Dictionary<string, string[]> AllowedFiles = new()
   {
       [".pdf"] = new[] {"application/pdf"},
       [".doc"] = new[] {"application/msword"},
       [".docx"] = new[] {"application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
       [".xls"] = new[] {"application/vnd.ms-excel"},
       [".xlsx"] = new[] {"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
       [".jpg"] = new[] {"image/jpeg"},
       [".jpeg"] = new[] {"images/jpeg"},
       [".png"] = new[] {"image/png"}

   };

   public DocumentService(DocuVaultDbContext context, ISupabaseStorageService storageService, IActivityService activityService)
    {
        _context = context;
        _storageService = storageService;
        _activityService = activityService;
    }

    public async Task<IEnumerable<DocumentResponseDto>>GetAllAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Documents.AsNoTracking().Where(d => d.UserId == userId && d.DeletedAt == null).OrderByDescending(d => d.CreatedAt).Select(d => new DocumentResponseDto
        {
            Id = d.Id,
            Name = d.Name,
            Description = d.Description,
            Category = d.Category,
            ContentType = d.ContentType,
            SizeBytes = d.SizeBytes,
            IsFavorite = d.IsFavorite,
            DeletedAt = d.DeletedAt,
            CreatedAt = d.CreatedAt
        }).ToListAsync(cancellationToken);
        
    }

    public async Task<DocumentResponseDto?>GetByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var document = await _context.Documents.AsNoTracking().FirstOrDefaultAsync(d=> d.Id == id && d.UserId == userId && d.DeletedAt == null,cancellationToken);
        return document == null ? null : MapToDto(document);
    }

    public async Task<DocumentResponseDto> UploadAsync(
        IFormFile file,
        string category,
        string? description,
        Guid? folderId,
        Guid userId,
        CancellationToken cancellationToken = default
    )

     {
        ValidatedFile(file);
    
        var storagePath = await _storageService.UploadAsync(file, userId, cancellationToken);
        var document = new Document
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = Path.GetFileName(file.FileName),
            Description = description,
            Category = string.IsNullOrWhiteSpace(category) ? "Uncategorized" : category,
            ContentType = file.ContentType,
            SizeBytes = file.Length,
            StoragePath = storagePath,
            FolderId = folderId,
            IsFavorite = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        if (folderId.HasValue)
        {
            var folderExists = await _context.Folders.AnyAsync(
                f => f.Id == folderId.Value && f.UserId == userId, cancellationToken
            );
            if (!folderExists)
            {
                throw new InvalidOperationException("The selected folder was not found.");
            }
        }

        _context.Documents.Add(document);
        await _context.SaveChangesAsync(cancellationToken);
        await _activityService.LogAsync(userId, document.Id,document.Name, "Uploaded", "Document uploaded successfully.", cancellationToken);
        return MapToDto(document);
    }

    public async Task<IEnumerable<DocumentResponseDto>>
    GetTrashAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var documents = await _context.Documents.AsNoTracking().Where(d => d.UserId == userId && d.DeletedAt != null).OrderByDescending(d => d.DeletedAt).ToListAsync(cancellationToken);

        return documents.Select(MapToDto);
    }
    public async Task<bool> ToggleFavoriteAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var document = await _context.Documents.FirstOrDefaultAsync(
            d => d.Id == id && d.UserId == userId && d.DeletedAt == null, cancellationToken
        );
        if(document == null)
        return false;

        document.IsFavorite = !document.IsFavorite;
        document.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        await _activityService.LogAsync(
            userId,
            document.Id,
            document.Name,
            document.IsFavorite ? "Favorited" : "Unfavorited",
            document.IsFavorite ? "Document added to favorites." : "Document removed from favorites.",
            cancellationToken);
        return document.IsFavorite;
    }

    public async Task<bool> MoveToTrashAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var document = await _context.Documents.FirstOrDefaultAsync(
            d => d.Id == id && d.UserId == userId && d.DeletedAt ==null, cancellationToken
        );

        if (document == null)
        return false;

        document.DeletedAt = DateTimeOffset.UtcNow;
        document.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        await _activityService.LogAsync(
            userId,
            document.Id,
            document.Name,
            "Moved to Trash",
            "Document moved to trash.",
            cancellationToken
        );

        return true;
    }

    public async Task<bool> RestoreAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId && d.DeletedAt != null, cancellationToken);
        if(document == null)
        return false;

        document.DeletedAt = null;
        document.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        await _activityService.LogAsync(
            userId,
            document.Id,
            document.Name,
            "Restored",
            "Document restored from trash.",
            cancellationToken
        );
        return true;
    }

    public async Task<bool> PermanentlyDeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId, cancellationToken);
        if(document == null)
        return false;

        await _storageService.DeleteAsync(document.StoragePath, cancellationToken);

        await _activityService.LogAsync(
            userId,
            document.Id,
            document.Name,
            "Permanently Deleted",
            "Document permanently deleted.",
            cancellationToken
        );

        _context.Documents.Remove(document);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
    private static void ValidatedFile(IFormFile file)
    {
        if(file == null || file.Length == 0)
        throw new ArgumentException("Please select a valid file.");

        if(file.Length > MaxFileSize)
        throw new ArgumentException("File size cannot exceed 10 MB.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if(!AllowedFiles.TryGetValue(extension, out var allowedTypes))
        throw new ArgumentException("The file is not supported.");
        if(!allowedTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        throw new ArgumentException("The file content type does not match its extension.");
    }

    private async Task<Document?> FindDocumentAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        return await _context.Documents.FirstOrDefaultAsync(
            d => d.Id == id && d.UserId == userId && d.DeletedAt == null, cancellationToken
        );
    }

    private static DocumentResponseDto MapToDto(Document document)
    {
        return new DocumentResponseDto
        {
            Id = document.Id,
            Name = document.Name,
            Description = document.Description,
            Category = document.Category,
            ContentType = document.ContentType,
            SizeBytes = document.SizeBytes,
            IsFavorite = document.IsFavorite,
            DeletedAt = document.DeletedAt,
            CreatedAt = document.CreatedAt
        };
    }

    public async Task<string?> GetDownloadUrlAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var document = await _context.Documents.AsNoTracking().FirstOrDefaultAsync(
            d => d.Id == id && d.UserId == userId && d.DeletedAt == null, cancellationToken
        );

        if (document == null)
            return null;

       var downloadUrl = await _storageService.CreateSignedUrlAsync(document.StoragePath, 300, cancellationToken);

       await _activityService.LogAsync(
        userId,
        document.Id,
        document.Name,
        "Download Link Created",
        "A secure download link was generated.", cancellationToken
       );
       return downloadUrl;
    }
    public async Task<bool> MoveToFolderAsync(
        Guid documentId,
        Guid? folderId,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var document = await _context.Documents.FirstOrDefaultAsync(
            d => d.Id == documentId && d.UserId == userId && d.DeletedAt == null, cancellationToken
        );

        if(document == null)
        {
            return false;
        }
        if (folderId.HasValue)
        {
            var folderExists = await _context.Folders.AnyAsync(
                f => f.Id == folderId.Value && f.UserId == userId, cancellationToken
            );

            if (!folderExists)
            {
                throw new InvalidCastException("The selected folder was not found.");
            }
        }
        document.FolderId = folderId;
        document.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        await _activityService.LogAsync(
            userId,
            document.Id,
            document.Name,
            "Moved",
            folderId.HasValue? "Document moved to another folder." : "Document removed form its folder.", cancellationToken
        );
        return true;
    }
    
    
}