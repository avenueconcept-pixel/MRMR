using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Systems;

public class CreateModel : AdminPageModel
{
  private readonly SystemDbHelper  _dbHelper;
  private readonly TranslationService _translation;

  [BindProperty] public string txtSystemName { get; set; } = string.Empty;
  [BindProperty] public string txtSystemCode { get; set; } = string.Empty;
  [BindProperty] public int    txtSortOrder  { get; set; } = 0;
  [BindProperty] public string ddlStatus     { get; set; } = StatusConstants.Active;

  public List<SelectListItem> StatusOptions { get; set; } = new();

  public CreateModel(SystemDbHelper dbHelper, TranslationService translation)
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

    if (string.IsNullOrWhiteSpace(txtSystemName) || string.IsNullOrWhiteSpace(txtSystemCode))
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.RequiredField);
      return Page();
    }

    var system = new AppSystem
    {
      SystemCode = txtSystemCode.Trim().ToLower(),
      SystemName = txtSystemName.Trim(),
      SortOrder  = txtSortOrder,
      Status     = ddlStatus
    };

    var result = await _dbHelper.AddAsync(system, CurrentUsername);

    if (result == SystemAddResult.DuplicateActive)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.DuplicateError);
      return Page();
    }

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.SaveSuccess);
    return RedirectToPage(Routes.AdminSystems);
  }
}
