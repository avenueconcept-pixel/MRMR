namespace MyApp.Dtos;

public class MaintenanceStatusDto
{
  public bool   IsUnderMaintenance { get; set; }
  public string Message            { get; set; } = string.Empty;
}
