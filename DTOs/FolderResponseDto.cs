namespace Docuvault.Api.DTOs;

public class FolderResponseDto
{
    public Guid Id {get; set;}
    public string Name {get; set;} = string.Empty;
    public int DocumentCount{get; set;}
    public DateTimeOffset CreatedAt {get; set;}
    public DateTimeOffset UpdatedAt {get; set;}
}