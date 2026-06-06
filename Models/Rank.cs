namespace MyApp.Models;

public class Rank
{
  public int    Id        { get; set; }
  public string RankCode  { get; set; } = string.Empty;
  public string RankName  { get; set; } = string.Empty;
  public int    SortOrder { get; set; }
  public string Status    { get; set; } = string.Empty;
}
