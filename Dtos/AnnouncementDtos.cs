namespace MyApp.Dtos;

public enum AnnouncementAddResult { Created, DuplicateActive }

public class AnnouncementAttachmentDto
{
  public int    Id           { get; set; }
  public string FileName     { get; set; } = string.Empty;
  public string OriginalName { get; set; } = string.Empty;
  public string FileType     { get; set; } = string.Empty;
}

public class AnnouncementTranslationInputDto
{
  public string LanguageCode { get; set; } = string.Empty;
  public string Label        { get; set; } = string.Empty;
  public string Title        { get; set; } = string.Empty;
  public string Body         { get; set; } = string.Empty;
}
