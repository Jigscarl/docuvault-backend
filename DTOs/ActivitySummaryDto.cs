namespace Docuvault.Api.DTOs;

public class ActivitySummaryDto
{
    public int TotalActivities { get; set; }
    public int TotalUploads { get; set; }
    public int TotalDownloads { get; set; }
    public int TotalFavorites { get; set; }
    public int TotalTrashActions { get; set; }
}
