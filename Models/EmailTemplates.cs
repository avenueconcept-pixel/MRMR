namespace MyApp.Models
{
  public class EmailTemplates
  {
    public int Id { get; set; }
    public string CultureCode { get; set; }
    public string TemplateKey { get; set; }
    public string Subject { get; set; }
    public string BodyHtml { get; set; }
  }

}
