using Docuvault.Api.DTOs;
namespace Docuvault.Api.Services;

public interface IFolderService
{
    Task<IEnumerable<FolderResponseDto>>GetAllAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task<FolderResponseDto> CreateAsync(
        string name,
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task<FolderResponseDto?> RenameAsync(
        Guid folderId,
        string name,
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task<bool>DeleteAsync(
        Guid folderId,
        Guid userId,
        CancellationToken cancellationToken = default
    );
}