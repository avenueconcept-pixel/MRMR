using MyApp.Constants;

namespace MyApp.Models;

public class Announcement
{
  public int    Id               { get; set; }
  public string AnnouncementCode { get; set; } = string.Empty;
  public string Audience         { get; set; } = AnnouncementConstants.AudienceAll;
  public DateTime StartAt        { get; set; }
  public DateTime EndAt          { get; set; }
  public int    SortOrder        { get; set; }
  public string Status           { get; set; } = StatusConstants.Active;
  public string CreatedBy        { get; set; } = string.Empty;
  public string UpdatedBy        { get; set; } = string.Empty;
  public DateTime CreatedAt      { get; set; }
  public DateTime UpdatedAt      { get; set; }

  public ICollection<AnnouncementTranslation> Translations { get; set; } = new List<AnnouncementTranslation>();
  public ICollection<AnnouncementAttachment>  Attachments  { get; set; } = new List<AnnouncementAttachment>();
}
