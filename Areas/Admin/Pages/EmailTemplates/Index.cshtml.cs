using Microsoft.AspNetCore.Mvc;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.EmailTemplates;

public class IndexModel : AdminPageModel
{
  private readonly EmailTemplateDbHelper _dbHelper;
  private readonly TranslationService    _translation;

  public Dictionary<string, List<EmailTemplate>> GroupedTemplates { get; set; } = new();

  public string MsgToggleConfirmTitle { get; set; } = string.Empty;
  public string MsgToggleConfirmText  { get; set; } = string.Empty;
  public string MsgToggleConfirmBtn   { get; set; } = string.Empty;
  public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
  public string MsgDeleteConfirmText  { get; set; } = string.Empty;
  public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
  public string MsgCancelBtn          { get; set; } = string.Empty;
  public string MsgSuccess            { get; set; } = string.Empty;
  public string MsgError              { get; set; } = string.Empty;
  public string LabelDelete           { get; set; } = string.Empty;

  public IndexModel(EmailTemplateDbHelper dbHelper, TranslationService translation)
  {
    _dbHelper    = dbHelper;
    _translation = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";
    var all = await _dbHelper.GetAllAsync();
    GroupedTemplates = all
        .GroupBy(e => e.TemplateKey)
        .ToDictionary(g => g.Key, g => g.ToList());

    MsgToggleConfirmTitle = await _translation.GetAsync("EmailTemplate.ToggleStatusTitle");
    MsgToggleConfirmText  = await _translation.GetAsync("ToggleStatusConfirm");
    MsgToggleConfirmBtn   = await _translation.GetAsync("ToggleStatusYes");
    MsgDeleteConfirmTitle = await _translation.GetAsync("Confirm.DeleteTitle");
    MsgDeleteConfirmText  = await _translation.GetAsync("Confirm.DeleteText");
    MsgDeleteConfirmBtn   = await _translation.GetAsync("Btn.YesDelete");
    MsgCancelBtn          = await _translation.GetAsync("Btn.Cancel");
    MsgSuccess            = await _translation.GetAsync(MessageConstants.SaveSuccess);
    MsgError              = await _translation.GetAsync(MessageConstants.SaveError);
    LabelDelete           = await _translation.GetAsync("Btn.Delete");
  }

  public async Task<IActionResult> OnPostToggleStatusAsync([FromForm] int id)
  {
    var template = await _dbHelper.GetByIdAsync(id);
    if (template == null)
      return new JsonResult(new { success = false });

    var newStatus = template.Status == StatusConstants.Active
        ? StatusConstants.Inactive
        : StatusConstants.Active;

    await _dbHelper.UpdateStatusAsync(id, newStatus, CurrentUsername);
    return new JsonResult(new { success = true, newStatus });
  }

  public async Task<IActionResult> OnPostSoftDeleteAsync([FromForm] int id)
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
}
