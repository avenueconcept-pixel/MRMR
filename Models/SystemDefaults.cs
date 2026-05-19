using System.ComponentModel.DataAnnotations;

namespace MyApp.Models
{
  public class SystemDefaults
  {
    [Key]
    public string KeyCode { get; set; }
    public string KeyValue { get; set; }
  }

  public class CountryState
  {
  
    public string StateCode { get; set; }
   
    public string CountryCode { get; set; }
    public string StateName { get; set; }
  }
}
