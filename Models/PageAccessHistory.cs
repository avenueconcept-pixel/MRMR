namespace MyApp.Models;

public class PageAccessHistory
{
  public long     Id            { get; set; }
  public string   SystemType    { get; set; } = string.Empty;
  public int      UserId        { get; set; }
  public string   Username      { get; set; } = string.Empty;
  public string?  FullName      { get; set; }
  public string?  SessionToken  { get; set; }
  public string   PageUrl       { get; set; } = string.Empty;
  public string   HttpMethod    { get; set; } = string.Empty;
  public string?  QueryString   { get; set; }
  public int      ResponseTime  { get; set; }
  public DateTime AccessedAt    { get; set; }
}
