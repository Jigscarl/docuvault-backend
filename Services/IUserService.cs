using Docuvault.Api.DTOs;
namespace Docuvault.Api.Services;

public interface IUserService
{
    Task<List<UserDto>> GetAllUsersAsync(
        CancellationToken cancellationToken = default
    );

    Task<UserDto?> GetUserByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task<bool>UpdateUserStatusAsync(
        Guid userId,
        bool isActive,
        CancellationToken cancellationToken = default
    );
    Task<bool> UpdateUserRoleAsync(
        Guid userId,
        string role,
        CancellationToken cancellationToken = default
    );
}