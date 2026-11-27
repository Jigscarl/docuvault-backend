using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace Docuvault.Api.DTOs;

public class UploadDocumentRequest
{
    [Required]
    public IFormFile File {get; set;} = null!;
    [StringLength(50)]
    public string Category{get;set;} = "Uncategorized";
    [StringLength(500)]
    public string? Description {get; set;}
    public Guid? FolderId {get; set;}
}