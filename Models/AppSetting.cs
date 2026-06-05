namespace MyApp.Models;

public class AppSetting
{
  public int      Id           { get; set; }
  public string   SystemCode   { get; set; } = string.Empty;
  public string   SettingKey   { get; set; } = string.Empty;
  public string   SettingValue { get; set; } = string.Empty;
  public string   UpdatedBy    { get; set; } = string.Empty;
  public DateTime UpdatedAt    { get; set; }
}
