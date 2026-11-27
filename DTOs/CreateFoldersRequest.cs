using System.ComponentModel.DataAnnotations;
namespace Docuvault.Api.DTOs;

public class CreateFolderRequest
{
    [Required]
    [StringLength(100, MinimumLength =2)]
    public string Name{get; set;} = string.Empty;
}