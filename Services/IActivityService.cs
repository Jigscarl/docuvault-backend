using Docuvault.Api.DTOs;

namespace Docuvault.Api.Services;

public interface IActivityService
{
    Task LogAsync(
        Guid userId,
        Guid? documentId,
        string documentName,
        string action,
        string? details = null,
        CancellationToken cancellationToken = default);

      Task<IEnumerable<ActivityResponseDto>> GetRecentAsync(
        Guid userId,
        int limit = 20,
        CancellationToken cancellationToken = default
      );

      Task<ActivitySummaryDto> GetSummaryAsync(
        Guid userId,
        CancellationToken cancellationToken = default
      )  ;
}