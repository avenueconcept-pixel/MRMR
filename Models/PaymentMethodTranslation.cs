namespace MyApp.Models;

public class PaymentMethodTranslation
{
  public string        PaymentCode  { get; set; } = string.Empty;
  public string        LanguageCode { get; set; } = string.Empty;
  public string        PaymentName  { get; set; } = string.Empty;

  public PaymentMethod PaymentMethod { get; set; } = null!;
}
