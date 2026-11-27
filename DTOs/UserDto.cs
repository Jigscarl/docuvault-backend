namespace Docuvault.Api.DTOs;

public class UserDto
{
    public Guid Id {get; set;}
    public string Email {get; set;} = string.Empty;
    public string? FullName {get; set;}
    public string Role {get; set;} = "User";
    public bool IsActive {get; set;}
    public DateTime CreatedAt {get; set;}
    public int TotalDocuments {get; set;}
    public long StorageUsedBytes {get; set;}
}