using Docuvault.Api.Authorization;
using Docuvault.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Docuvault.Api.Authorization;

public class AdminOnlyHandler : AuthorizationHandler<AdminOnlyRequirement>
{
    private readonly DocuVaultDbContext _context;
    public AdminOnlyHandler(DocuVaultDbContext context)
    {
        _context = context;
    }
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AdminOnlyRequirement requirement
    )
    {
        var userIdValue = context.User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(userIdValue, out var userId))
        return ;

        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if(user == null || !user.IsActive) return;

        if(user.Role == "Admin")
        {
            context.Succeed(requirement);
        }
    }
}