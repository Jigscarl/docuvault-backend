namespace Docuvault.Api.DTOs;
public class DashboardAnalyticsDto
{
    public int TotalDocuments {get; set;}
    public int TotalFolders {get; set;}
    public int TotalFavorites{get; set;}
    public long TotalStorageBytes{get; set;}

    public List <CategoryAnalyticsDto> Categories {get; set; } = new();
    public List <FileTypeAnalyticsDto>FileTypes {get; set;} = new();
    public List<DailyUploadAnalyticsDto> dailyUploads {get; set;} = new();
}
public class CategoryAnalyticsDto
{
    public string Category {get; set;} = string.Empty;
    public int Count {get; set;}
}

public class FileTypeAnalyticsDto
{
    public string Extension {get; set;} = string.Empty;
    public long TotalBytes {get; set;}
    public int Count {get; set;}
}
public class DailyUploadAnalyticsDto
{
    public DateTime Date {get; set;}
    public int Count {get; set;}
}