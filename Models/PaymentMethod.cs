using MyApp.Constants;

namespace MyApp.Models;

public class PaymentMethod
{
  public string   PaymentCode { get; set; } = string.Empty;
  public string   Status      { get; set; } = StatusConstants.Active;
  public DateTime CreatedAt   { get; set; }
  public string   CreatedBy   { get; set; } = string.Empty;
  public DateTime UpdatedAt   { get; set; }
  public string   UpdatedBy   { get; set; } = string.Empty;

  public ICollection<PaymentMethodTranslation> Translations { get; set; } = new List<PaymentMethodTranslation>();
}
