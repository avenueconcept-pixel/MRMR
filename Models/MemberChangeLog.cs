namespace MyApp.Models;

public class MemberChangeLog
{
  public long     Id         { get; set; }
  public int      MemberId   { get; set; }
  public string   ChangeType { get; set; } = string.Empty;
  public string   OldValue   { get; set; } = string.Empty;
  public string   NewValue   { get; set; } = string.Empty;
  public string   ChangedBy  { get; set; } = string.Empty;
  public DateTime ChangedAt  { get; set; }

  public Member? Member { get; set; }
}
