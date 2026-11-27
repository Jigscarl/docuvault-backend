namespace Docuvault.Api.Services;

public interface ISupabaseStorageService
{
    Task <string> UploadAsync(
        IFormFile file,
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task DeleteAsync (string storagePath, CancellationToken cancellationToken = default);

    Task<string> CreateSignedUrlAsync(
        string storagePath,
        int expiresInSeconds = 300,
        CancellationToken cancellationToken = default
    );
}