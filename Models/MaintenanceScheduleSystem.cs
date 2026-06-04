namespace MyApp.Models;

public class MaintenanceScheduleSystem
{
  public int    Id            { get; set; }
  public int    MaintenanceId { get; set; }
  public string SystemCode    { get; set; } = string.Empty;

  public MaintenanceSchedule? Maintenance { get; set; }
  public AppSystem?           System      { get; set; }
}
