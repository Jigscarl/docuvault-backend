using Docuvault.Api.DTOs;
using Docuvault.Api.Models;

namespace Docuvault.Api.Services;

public class DocumentService : IDocumentService
{
    private readonly List<Document> _documents = new();

    public Task<IEnumerable<DocumentResponseDto>> GetAllAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var documents = _documents
            .Where(d => d.UserId == userId && d.DeletedAt == null)
            .Select(MapToDto)
            .ToList();

        return Task.FromResult<IEnumerable<DocumentResponseDto>>(documents);
    }

    public Task<DocumentResponseDto?> GetByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var document = _documents.FirstOrDefault(d =>
            d.Id == id &&
            d.UserId == userId &&
            d.DeletedAt == null);

        return Task.FromResult(document == null ? null : MapToDto(document));
    }

    public async Task<DocumentResponseDto> UploadAsync(
        IFormFile file,
        string category,
        string? description,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Please select a valid file.");

        var document = new Document
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = Path.GetFileName(file.FileName),
            Description = description,
            Category = category,
            ContentType = file.ContentType,
            SizeBytes = file.Length,
            StoragePath = $"temporary/{Guid.NewGuid()}",
            IsFavorite = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _documents.Add(document);
        await Task.CompletedTask;
        return MapToDto(document);
    }

    public Task<bool> ToggleFavoriteAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var document = FindDocument(id, userId);
        if (document == null)
            return Task.FromResult(false);

        document.IsFavorite = !document.IsFavorite;
        document.UpdatedAt = DateTimeOffset.UtcNow;
        return Task.FromResult(true);
    }

    public Task<bool> MoveToTrashAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var document = FindDocument(id, userId);
        if (document == null)
            return Task.FromResult(false);

        document.DeletedAt = DateTimeOffset.UtcNow;
        document.UpdatedAt = DateTimeOffset.UtcNow;
        return Task.FromResult(true);
    }

    public Task<bool> RestoreAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var document = _documents.FirstOrDefault(d =>
            d.Id == id &&
            d.UserId == userId &&
            d.DeletedAt != null
        );

        if (document == null)
            return Task.FromResult(false);

        document.DeletedAt = null;
        document.UpdatedAt = DateTimeOffset.UtcNow;
        return Task.FromResult(true);
    }

    public Task<bool> PermanentlyDeleteAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var document = _documents.FirstOrDefault(d =>
            d.Id == id &&
            d.UserId == userId
        );

        if (document == null)
            return Task.FromResult(false);

        _documents.Remove(document);
        return Task.FromResult(true);
    }

    private Document? FindDocument(Guid id, Guid userId)
    {
        return _documents.FirstOrDefault(d =>
            d.Id == id &&
            d.UserId == userId &&
            d.DeletedAt == null
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
}