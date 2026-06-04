using MyApp.Constants;

namespace MyApp.Models;

public class MaintenanceSchedule
{
  public int      Id        { get; set; }
  public string   Title     { get; set; } = string.Empty;
  public DateTime StartAt   { get; set; }
  public DateTime EndAt     { get; set; }
  public bool     IsActive  { get; set; } = true;
  public string   Status    { get; set; } = StatusConstants.Active;
  public string   CreatedBy { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
  public string   UpdatedBy { get; set; } = string.Empty;
  public DateTime UpdatedAt { get; set; }

  public IList<MaintenanceScheduleSystem>  Systems  { get; set; } = new List<MaintenanceScheduleSystem>();
  public IList<MaintenanceScheduleMessage> Messages { get; set; } = new List<MaintenanceScheduleMessage>();
}
