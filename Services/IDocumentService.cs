using Docuvault.Api.DTOs;
namespace Docuvault.Api.Services;
public interface IDocumentService
{
    Task < IEnumerable<DocumentResponseDto>> GetAllAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task<IEnumerable<DocumentResponseDto>> GetTrashAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task<DocumentResponseDto?> GetByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task <DocumentResponseDto> UploadAsync(
        IFormFile file,
        string category,
        string? description,
        Guid? folderId,
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task<bool> ToggleFavoriteAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task<bool> MoveToTrashAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task<bool>RestoreAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task<bool> PermanentlyDeleteAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task <string?> GetDownloadUrlAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task<bool>MoveToFolderAsync(
        Guid documentId,
        Guid? folderId,
        Guid userId,
        CancellationToken cancellationToken = default
    );
}