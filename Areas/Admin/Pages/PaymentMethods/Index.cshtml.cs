using Microsoft.AspNetCore.Mvc;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.PaymentMethods;

public class IndexModel : AdminPageModel
{
  private readonly PaymentMethodDbHelper _pmDbHelper;
  private readonly TranslationService    _translation;

  public List<PaymentMethod> PaymentMethods { get; set; } = new();

  public IndexModel(PaymentMethodDbHelper pmDbHelper, TranslationService translation)
  {
    _pmDbHelper  = pmDbHelper;
    _translation = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";
    var langCode = string.IsNullOrEmpty(CurrentLangCode) ? "en" : CurrentLangCode;
    PaymentMethods = await _pmDbHelper.GetAllAsync(langCode);
  }

  public async Task<IActionResult> OnPostToggleStatusAsync([FromForm] string paymentCode)
  {
    var pm = await _pmDbHelper.GetByCodeAsync(paymentCode);
    if (pm == null)
      return new JsonResult(new { success = false });

    var newStatus = pm.Status == StatusConstants.Active
        ? StatusConstants.Inactive
        : StatusConstants.Active;

    await _pmDbHelper.UpdateStatusAsync(paymentCode, newStatus, CurrentUsername);
    return new JsonResult(new { success = true, newStatus });
  }
}
