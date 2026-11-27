using System.Reflection.Metadata;

namespace Docuvault.Api.Models;
public class DocumentActivity
{
    public Guid Id {get; set;}
    public Guid UserId {get; set;}
    public Guid? DocumentId {get; set;}
    public string DocumentName{get; set;} = string.Empty;
    public string Action {get; set;} = string.Empty;
    public string? Details {get; set;}
    public DateTimeOffset CreatedAt {get; set;}
    public Document? Document{get; set;}
}