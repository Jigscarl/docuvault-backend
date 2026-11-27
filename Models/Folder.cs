using System.Reflection.Metadata;

namespace Docuvault.Api.Models;

public class Folder
{
    public Guid Id {get; set;}
    public Guid UserId {get; set;}
    public string Name {get; set;} = string.Empty;
    public DateTimeOffset CreatedAt {get; set;}
    public DateTimeOffset UpdatedAt {get; set;}

    public ICollection <Document>Documents{get; set;} = new List<Document>();
}