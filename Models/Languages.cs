namespace MyApp.Models
{
  public class Languages
  {
    public int Id { get; set; }
    public string CultureCode { get; set; }
    public string Name { get; set; }
  }

  public class LanguagesWord
  {
    public int Id { get; set; }
    public string Code { get; set; }
    public string Word { get; set; }
    public string CultureCode { get; set; }

    public int LanguageId { get; set; }

    public Languages Language { get; set; }
  }

}
