namespace MyApp.Models;

public class MemberRankHistory
{
  public int      Id          { get; set; }
  public int      MemberId    { get; set; }
  public string   RankCode    { get; set; } = string.Empty;
  public decimal  Pv          { get; set; }
  public int      PeriodYear  { get; set; }
  public int      PeriodMonth { get; set; }
  public DateTime CreatedAt   { get; set; }

  public Member? Member { get; set; }
}
