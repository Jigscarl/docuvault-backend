namespace Docuvault.Api.DTOs;
public class DocumentResponseDto
{
    public Guid Id {get; set;}
    public string Name {get; set;} = string.Empty;
    public string? Description {get; set;}
    public string Category {get; set;} = string.Empty;
    public string ContentType {get; set;} = string.Empty;
    public long SizeBytes {get; set;}
    public bool IsFavorite {get; set;}
    public Guid FolderId {get; set;}
    public DateTimeOffset? DeletedAt {get; set;}
    public DateTimeOffset CreatedAt {get; set;}
}