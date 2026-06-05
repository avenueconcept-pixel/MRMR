namespace MyApp.Models;

public class AnnouncementTranslation
{
  public int    Id               { get; set; }
  public string AnnouncementCode { get; set; } = string.Empty;
  public string LanguageCode     { get; set; } = string.Empty;
  public string Title            { get; set; } = string.Empty;
  public string Body             { get; set; } = string.Empty;

  public Announcement Announcement { get; set; } = null!;
}
