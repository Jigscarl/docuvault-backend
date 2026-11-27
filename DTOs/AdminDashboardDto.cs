namespace Docuvault.Api.DTOs;
public class AdminDashboardDto
{
    public int TotalUsers {get; set;}
    public int ActiveUsers {get; set;}
    public int InactiveUsers {get; set;}
    public int TotalDocuments {get; set;}
    public long TotalStorageBytes {get; set;}

    public int TotalFolders {get; set;}
    public int TotalAdmins {get; set;}
    public int TotalRegularUsers {get; set;}
}