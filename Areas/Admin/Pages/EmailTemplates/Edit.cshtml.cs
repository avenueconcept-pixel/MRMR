using Microsoft.AspNetCore.Mvc;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.EmailTemplates;

public class EditModel : AdminPageModel
{
  private readonly EmailTemplateDbHelper _dbHelper;
  private readonly TranslationService    _translation;

  [BindProperty] public string txtSubject  { get; set; } = string.Empty;
  [BindProperty] public string txtBodyHtml { get; set; } = string.Empty;

  public int      TemplateId   { get; set; }
  public string   TemplateKey  { get; set; } = string.Empty;
  public string   LanguageCode { get; set; } = string.Empty;
  public string   CreatedBy    { get; set; } = string.Empty;
  public DateTime CreatedAt    { get; set; }
  public string   UpdatedBy    { get; set; } = string.Empty;
  public DateTime UpdatedAt    { get; set; }

  public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
  public string MsgDeleteConfirmText  { get; set; } = string.Empty;
  public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
  public string MsgCancelBtn          { get; set; } = string.Empty;
  public string MsgDeleteSuccess      { get; set; } = string.Empty;
  public string MsgDeleteError        { get; set; } = string.Empty;
  public string LabelDelete           { get; set; } = string.Empty;

  public EditModel(EmailTemplateDbHelper dbHelper, TranslationService translation)
  {
    _dbHelper    = dbHelper;
    _translation = translation;
  }

  public async Task<IActionResult> OnGetAsync(int id)
  {
    AlertMessageType = "";
    var tmpl = await _dbHelper.GetByIdAsync(id);
    if (tmpl == null)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.NotFound);
      return RedirectToPage(Routes.AdminEmailTemplate);
    }

    TemplateId   = tmpl.Id;
    TemplateKey  = tmpl.TemplateKey;
    LanguageCode = tmpl.LanguageCode;
    txtSubject   = tmpl.Subject;
    txtBodyHtml  = tmpl.BodyHtml;
    CreatedBy    = tmpl.CreatedBy;
    CreatedAt    = tmpl.CreatedAt;
    UpdatedBy    = tmpl.UpdatedBy;
    UpdatedAt    = tmpl.UpdatedAt;

    await LoadDeleteMessagesAsync(tmpl);
    return Page();
  }

  public async Task<IActionResult> OnPostUpdateAsync(int id)
  {
    var tmpl = await _dbHelper.GetByIdAsync(id);
    if (tmpl == null)
      return RedirectToPage(Routes.AdminEmailTemplate);

    TemplateId   = id;
    TemplateKey  = tmpl.TemplateKey;
    LanguageCode = tmpl.LanguageCode;
    CreatedBy    = tmpl.CreatedBy;
    CreatedAt    = tmpl.CreatedAt;
    UpdatedBy    = tmpl.UpdatedBy;
    UpdatedAt    = tmpl.UpdatedAt;

    if (string.IsNullOrWhiteSpace(txtSubject) || string.IsNullOrWhiteSpace(txtBodyHtml))
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.RequiredField);
      await LoadDeleteMessagesAsync(tmpl);
      return Page();
    }

    var updated = new EmailTemplate
    {
      Id       = id,
      Subject  = txtSubject.Trim(),
      BodyHtml = txtBodyHtml
    };

    await _dbHelper.UpdateAsync(updated, CurrentUsername);

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.UpdateSuccess);
    return RedirectToPage(Routes.AdminEmailTemplate);
  }

  public async Task<IActionResult> OnPostSoftDeleteAsync(int id)
  {
    try
    {
      await _dbHelper.UpdateStatusAsync(id, StatusConstants.Deleted, CurrentUsername);
      var msg = await _translation.GetAsync(MessageConstants.DeleteSuccess);
      return new JsonResult(new { success = true, message = msg });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.DeleteError);
      return new JsonResult(new { success = false, message = msg });
    }
  }

  private async Task LoadDeleteMessagesAsync(EmailTemplate tmpl)
  {
    var entityLabel       = $"{tmpl.TemplateKey} ({tmpl.LanguageCode})";
    MsgDeleteConfirmTitle = $"{await _translation.GetAsync("Confirm.DeleteTitle")} {entityLabel}";
    MsgDeleteConfirmText  = await _translation.GetAsync("Confirm.DeleteText");
    MsgDeleteConfirmBtn   = await _translation.GetAsync("Btn.YesDelete");
    MsgCancelBtn          = await _translation.GetAsync("Btn.Cancel");
    MsgDeleteSuccess      = await _translation.GetAsync(MessageConstants.DeleteSuccess);
    MsgDeleteError        = await _translation.GetAsync(MessageConstants.DeleteError);
    LabelDelete           = await _translation.GetAsync("Btn.Delete");
  }
}
