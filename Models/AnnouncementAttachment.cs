namespace MyApp.Models;

public class AnnouncementAttachment
{
  public int    Id               { get; set; }
  public string AnnouncementCode { get; set; } = string.Empty;
  public string FileName         { get; set; } = string.Empty;
  public string OriginalName     { get; set; } = string.Empty;
  public string FileType         { get; set; } = string.Empty;
  public int    SortOrder        { get; set; }
  public string CreatedBy        { get; set; } = string.Empty;
  public DateTime CreatedAt      { get; set; }

  public Announcement Announcement { get; set; } = null!;
}
