using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Ranks;

public class CreateModel : AdminPageModel
{
  private readonly RankDbHelper       _dbHelper;
  private readonly TranslationService _translation;

  [BindProperty] public string txtRankCode  { get; set; } = string.Empty;
  [BindProperty] public string txtRankName  { get; set; } = string.Empty;
  [BindProperty] public int    txtSortOrder { get; set; } = 0;
  [BindProperty] public string ddlStatus    { get; set; } = StatusConstants.Active;

  public List<SelectListItem> StatusOptions { get; set; } = new();

  public CreateModel(RankDbHelper dbHelper, TranslationService translation)
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

    if (string.IsNullOrWhiteSpace(txtRankCode))
    {
      SetError(await _translation.GetAsync(MessageConstants.RequiredField));
      return Page();
    }

    if (string.IsNullOrWhiteSpace(txtRankName))
    {
      SetError(await _translation.GetAsync(MessageConstants.RequiredField));
      return Page();
    }

    var rank = new Rank
    {
      RankCode  = txtRankCode.Trim().ToUpper(),
      RankName  = txtRankName.Trim(),
      SortOrder = txtSortOrder,
      Status    = ddlStatus
    };

    var result = await _dbHelper.AddAsync(rank, CurrentUsername);

    if (result == RankAddResult.DuplicateActive)
    {
      SetError(await _translation.GetAsync(MessageConstants.DuplicateError));
      return Page();
    }

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(
        result == RankAddResult.Restored ? MessageConstants.RestoreSuccess : MessageConstants.SaveSuccess);

    return RedirectToPage(Routes.AdminRank);
  }

  private void SetError(string message)
  {
    AlertMessageType    = MessageType.Error;
    AlertMessageTitle   = MessageTitle.Error;
    AlertMessageContent = message;
  }
}
