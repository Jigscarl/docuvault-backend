using Docuvault.Api.Data;
using Docuvault.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Docuvault.Api.Services;

public class UserService : IUserService
{
    private readonly DocuVaultDbContext _context;
    public UserService(DocuVaultDbContext context)
    {
        _context = context;
    }
    public async Task<List<UserDto>> GetAllUsersAsync(
        CancellationToken cancellationToken = default
    )
    {
        var users = await _context.Users.AsNoTracking().OrderByDescending(u => u.CreatedAt).ToListAsync(cancellationToken);

        var documentStats = await _context.Documents.AsNoTracking().Where(d => d.DeletedAt == null).GroupBy(d => d.UserId).Select(group => new
        {
            UserId = group.Key,
            TotalDocuments = group.Count(),
            StorageUsedBytes = group.Sum(d => (long)d.SizeBytes)
        }).ToListAsync(cancellationToken);

        var statsLookup = documentStats.ToDictionary(s => s.UserId, s=> s);

        return users.Select(user =>
        {
            statsLookup.TryGetValue(user.Id, out var stats);
            return MapToDto(
                user,
                stats?.TotalDocuments ?? 0,
                stats?.StorageUsedBytes ?? 0
            );
        }).ToList();
    }

    public async Task<UserDto?> GetUserByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null) return null;
        var stats = await _context.Documents.AsNoTracking().Where(d => d.UserId == userId && d.DeletedAt == null).GroupBy(d => d.UserId).Select(group => new
        {
            TotalDocuments = group.Count(),
            StorageUsedBytes = group.Sum(d => (long)d.SizeBytes)
        }).FirstOrDefaultAsync(cancellationToken);

        return MapToDto(
            user,
            stats?.TotalDocuments ?? 0,
            stats?.StorageUsedBytes ?? 0
        );
    }

    public async Task<bool> UpdateUserStatusAsync(
        Guid userId,
        bool isActive,
        CancellationToken cancellationToken = default
    )
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if(user == null)
        return false;
        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateUserRoleAsync(
        Guid userId,
        string role,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(role))
        throw new ArgumentException("Role cannot be empty.");

        var normalizedRole = role.Trim();

        if(!normalizedRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) && !normalizedRole.Equals("User", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Invalid role. Allowed roles are Admin and User.");
        }
        normalizedRole = normalizedRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "User";

        var user = await _context.Users.FirstOrDefaultAsync(u=> u.Id == userId, cancellationToken);
        if(user == null) return false;

        user.Role = normalizedRole;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static UserDto MapToDto(
        Models.User user,
        int totalDocuments,
        long storageUsedBytes
    )
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            TotalDocuments = totalDocuments,
            StorageUsedBytes = storageUsedBytes
        };
    }
}