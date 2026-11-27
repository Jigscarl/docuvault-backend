namespace Docuvault.Api.DTOs;
public class ActivityResponseDto
{
    public Guid Id {get; set;}
    public Guid? DocumentId {get; set;}
    public string DocumentName{get; set;} = string.Empty;
    public string Action {get; set;} = string.Empty;
    public string? Details {get;  set;}
    public DateTimeOffset CreatedAt{get; set;}
}