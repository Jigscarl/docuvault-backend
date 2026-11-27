namespace Docuvault.Api.DTOs;
public class AdminRecentActivityDto
{
    public Guid Id {get; set;}
    public Guid UserId {get; set;}
    public string UserName {get; set;} = string.Empty;
    public string UserEmail {get; set;} = string.Empty;

    public string Action {get; set;} = string.Empty;
    public string DocumentName {get; set;} = string.Empty;
    public string? Details {get; set;}
    public DateTimeOffset CreatedAt {get; set;}
}