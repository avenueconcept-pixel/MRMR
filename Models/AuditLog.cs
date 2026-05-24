namespace MyApp.Models;

public class AuditLog
{
  public long     Id        { get; set; }
  public string   TableName { get; set; } = string.Empty;
  public string   RecordId  { get; set; } = string.Empty;
  public string   Action    { get; set; } = string.Empty;
  public string?  FieldName { get; set; }
  public string?  OldValue  { get; set; }
  public string?  NewValue  { get; set; }
  public string   ChangedBy { get; set; } = string.Empty;
  public DateTime ChangedAt { get; set; }
  public string?  IpAddress { get; set; }
  public string?  Remarks   { get; set; }
}
