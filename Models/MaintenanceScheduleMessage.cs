namespace MyApp.Models;

public class MaintenanceScheduleMessage
{
  public int    Id             { get; set; }
  public int    MaintenanceId  { get; set; }
  public string LanguageCode   { get; set; } = string.Empty;
  public string Message        { get; set; } = string.Empty;

  public MaintenanceSchedule? Maintenance { get; set; }
}
