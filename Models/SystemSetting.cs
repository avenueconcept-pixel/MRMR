namespace MyApp.Models;

public class SystemSetting
{
    public int      Id           { get; set; }
    public string   SettingKey   { get; set; } = string.Empty;
    public string   SettingValue { get; set; } = string.Empty;
    public string   KeyType      { get; set; } = string.Empty;
    public string   Description  { get; set; } = string.Empty;
    public string   UpdatedBy    { get; set; } = string.Empty;
    public DateTime UpdatedAt    { get; set; }
}
