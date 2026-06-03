using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.PriceTiers;

public class CreateModel : AdminPageModel
{
  private readonly PriceTierDbHelper  _dbHelper;
  private readonly TranslationService _translation;

  [BindProperty] public string txtTierCode  { get; set; } = string.Empty;
  [BindProperty] public string txtTierName  { get; set; } = string.Empty;
  [BindProperty] public int    txtSortOrder { get; set; } = 0;
  [BindProperty] public string ddlStatus    { get; set; } = StatusConstants.Active;

  public List<SelectListItem> StatusOptions { get; set; } = new();

  public CreateModel(PriceTierDbHelper dbHelper, TranslationService translation)
  {
    _dbHelper    = dbHelper;
    _translation = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";
    StatusOptions    = await SelectListHelper.GetStatusOptions(_translation);
  }

  public async Task<IActionResult> OnPostCreateAsync()
  {
    StatusOptions = await SelectListHelper.GetStatusOptions(_translation);

    if (string.IsNullOrWhiteSpace(txtTierCode))
    {
      SetError(await _translation.GetAsync(MessageConstants.RequiredField));
      return Page();
    }

    if (string.IsNullOrWhiteSpace(txtTierName))
    {
      SetError(await _translation.GetAsync(MessageConstants.RequiredField));
      return Page();
    }

    var tier = new PriceTier
    {
      TierCode  = txtTierCode.Trim().ToUpper(),
      TierName  = txtTierName.Trim(),
      SortOrder = txtSortOrder,
      Status    = ddlStatus
    };

    var result = await _dbHelper.AddAsync(tier, CurrentUsername);

    if (result == PriceTierAddResult.DuplicateActive)
    {
      SetError(await _translation.GetAsync(MessageConstants.DuplicateError));
      return Page();
    }

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(
        result == PriceTierAddResult.Restored ? MessageConstants.RestoreSuccess : MessageConstants.SaveSuccess);

    return RedirectToPage(Routes.AdminPriceTier);
  }

  private void SetError(string message)
  {
    AlertMessageType    = MessageType.Error;
    AlertMessageTitle   = MessageTitle.Error;
    AlertMessageContent = message;
  }
}
