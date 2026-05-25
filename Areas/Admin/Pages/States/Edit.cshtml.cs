using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.States;

public class EditModel : AdminPageModel
{
  private readonly StateDbHelper      _stateDbHelper;
  private readonly LanguageDbHelper   _languageDbHelper;
  private readonly TranslationService _translation;

  [BindProperty] public string ddlStatus    { get; set; } = StatusConstants.Active;
  [BindProperty] public string txtSortOrder { get; set; } = "0";

  public int      Id          { get; set; }
  public string   StateCode   { get; set; } = string.Empty;
  public string   CountryCode { get; set; } = string.Empty;
  public string   CountryName { get; set; } = string.Empty;
  public string   CreatedBy   { get; set; } = string.Empty;
  public DateTime CreatedAt   { get; set; }
  public string   UpdatedBy   { get; set; } = string.Empty;
  public DateTime UpdatedAt   { get; set; }

  public List<TranslationInputDto> TranslationInputs { get; set; } = new();
  public List<SelectListItem>      StatusOptions     { get; set; } = new();

  public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
  public string MsgDeleteConfirmText  { get; set; } = string.Empty;
  public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
  public string MsgCancelBtn          { get; set; } = string.Empty;
  public string MsgDeleteSuccess      { get; set; } = string.Empty;
  public string MsgDeleteError        { get; set; } = string.Empty;
  public string LabelDelete           { get; set; } = string.Empty;

  public EditModel(StateDbHelper stateDbHelper, LanguageDbHelper languageDbHelper, TranslationService translation)
  {
    _stateDbHelper    = stateDbHelper;
    _languageDbHelper = languageDbHelper;
    _translation      = translation;
  }

  public async Task<IActionResult> OnGetAsync(int id)
  {
    AlertMessageType = "";

    var entity = await _stateDbHelper.GetByIdAsync(id);
    if (entity == null)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.NotFound);
      return RedirectToPage(Routes.AdminState);
    }

    var langCode = string.IsNullOrEmpty(CurrentLangCode) ? "en" : CurrentLangCode;

    Id          = entity.Id;
    StateCode   = entity.StateCode;
    CountryCode = entity.CountryCode;
    CountryName = entity.Country.Translations.FirstOrDefault(t => t.LanguageCode == langCode)?.CountryName
               ?? entity.Country.Translations.FirstOrDefault()?.CountryName
               ?? entity.CountryCode;
    ddlStatus   = entity.Status;
    txtSortOrder = entity.SortOrder.ToString();
    CreatedBy   = entity.CreatedBy;
    CreatedAt   = entity.CreatedAt;
    UpdatedBy   = entity.UpdatedBy;
    UpdatedAt   = entity.UpdatedAt;

    StatusOptions     = await SelectListHelper.GetStatusOptions(_translation);
    TranslationInputs = await BuildInputsAsync(entity.Translations.ToList());

    var entityName        = entity.Translations.FirstOrDefault(t => t.LanguageCode == "en")?.StateName ?? entity.StateCode;
    MsgDeleteConfirmTitle = $"{await _translation.GetAsync("Confirm.DeleteTitle")} {entityName}";
    MsgDeleteConfirmText  = await _translation.GetAsync("Confirm.DeleteText");
    MsgDeleteConfirmBtn   = await _translation.GetAsync("Btn.YesDelete");
    MsgCancelBtn          = await _translation.GetAsync("Btn.Cancel");
    MsgDeleteSuccess      = await _translation.GetAsync(MessageConstants.DeleteSuccess);
    MsgDeleteError        = await _translation.GetAsync(MessageConstants.DeleteError);
    LabelDelete           = await _translation.GetAsync("Btn.Delete");

    return Page();
  }

  public async Task<IActionResult> OnPostUpdateAsync(int id)
  {
    int.TryParse(txtSortOrder, out var sortOrder);

    Id            = id;
    StatusOptions     = await SelectListHelper.GetStatusOptions(_translation);
    TranslationInputs = await BuildInputsAsync(null);

    var state = new State
    {
      Id        = id,
      Status    = ddlStatus,
      SortOrder = sortOrder
    };

    var languages    = await _languageDbHelper.GetAllActiveAsync();
    var translations = languages
        .Select(l => new StateTranslation
        {
          StateId      = id,
          LanguageCode = l.LanguageCode,
          StateName    = Request.Form[$"txtName_{l.LanguageCode}"].ToString().Trim()
        })
        .Where(t => !string.IsNullOrEmpty(t.StateName))
        .ToList();

    await _stateDbHelper.UpdateAsync(state, translations, CurrentUsername);

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.UpdateSuccess);

    return RedirectToPage(Routes.AdminState);
  }

  public async Task<IActionResult> OnPostSoftDeleteAsync(int id)
  {
    try
    {
      await _stateDbHelper.UpdateStatusAsync(id, StatusConstants.Deleted, CurrentUsername);
      var msg = await _translation.GetAsync(MessageConstants.DeleteSuccess);
      return new JsonResult(new { success = true, message = msg });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.DeleteError);
      return new JsonResult(new { success = false, message = msg });
    }
  }

  private async Task<List<TranslationInputDto>> BuildInputsAsync(IList<StateTranslation>? existing)
  {
    var languages   = await _languageDbHelper.GetAllActiveAsync();
    var placeholder = await _translation.GetAsync("State.NamePlaceholder");
    return languages.Select(l => new TranslationInputDto
    {
      LanguageCode = l.LanguageCode,
      Label        = l.LanguageName,
      Value        = existing != null
          ? existing.FirstOrDefault(t => t.LanguageCode == l.LanguageCode)?.StateName ?? string.Empty
          : Request.Form[$"txtName_{l.LanguageCode}"].ToString(),
      Placeholder  = placeholder
    }).ToList();
  }
}
