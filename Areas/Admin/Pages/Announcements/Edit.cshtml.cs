using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Announcements;

public class EditModel : AdminPageModel
{
  private readonly AnnouncementDbHelper _dbHelper;
  private readonly LanguageDbHelper     _languageDbHelper;
  private readonly TranslationService   _translation;
  private readonly IWebHostEnvironment  _env;
  private readonly IConfiguration       _config;

  [BindProperty] public string           ddlAudience     { get; set; } = AnnouncementConstants.AudienceAll;
  [BindProperty] public string           txtStartAt      { get; set; } = string.Empty;
  [BindProperty] public string           txtEndAt        { get; set; } = string.Empty;
  [BindProperty] public int              txtSortOrder    { get; set; } = 0;
  [BindProperty] public string           ddlStatus       { get; set; } = StatusConstants.Active;
  [BindProperty] public List<IFormFile>? fileAttachments { get; set; }

  public string AnnouncementCode  { get; set; } = string.Empty;
  public string CreatedBy         { get; set; } = string.Empty;
  public DateTime CreatedAt       { get; set; }
  public string UpdatedBy         { get; set; } = string.Empty;
  public DateTime UpdatedAt       { get; set; }

  public List<AnnouncementTranslationInputDto> TranslationInputs { get; set; } = new();
  public List<AnnouncementAttachment>          Attachments       { get; set; } = new();
  public List<SelectListItem>                  AudienceOptions   { get; set; } = new();
  public List<SelectListItem>                  StatusOptions      { get; set; } = new();

  public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
  public string MsgDeleteConfirmText  { get; set; } = string.Empty;
  public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
  public string MsgCancelBtn          { get; set; } = string.Empty;
  public string MsgDeleteSuccess      { get; set; } = string.Empty;
  public string MsgDeleteError        { get; set; } = string.Empty;
  public string LabelDelete           { get; set; } = string.Empty;

  public EditModel(
      AnnouncementDbHelper dbHelper,
      LanguageDbHelper     languageDbHelper,
      TranslationService   translation,
      IWebHostEnvironment  env,
      IConfiguration       config)
  {
    _dbHelper         = dbHelper;
    _languageDbHelper = languageDbHelper;
    _translation      = translation;
    _env              = env;
    _config           = config;
  }

  public async Task<IActionResult> OnGetAsync(string announcementCode)
  {
    AlertMessageType = "";
    var ann = await _dbHelper.GetByCodeAsync(announcementCode);
    if (ann == null)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.NotFound);
      return RedirectToPage(Routes.AdminAnnouncements);
    }

    AnnouncementCode  = ann.AnnouncementCode;
    ddlAudience       = ann.Audience;
    txtStartAt        = ann.StartAt.ToUserLocalTime(UserTimezone, AppConstants.DateTimeInputFormat);
    txtEndAt          = ann.EndAt.ToUserLocalTime(UserTimezone, AppConstants.DateTimeInputFormat);
    txtSortOrder      = ann.SortOrder;
    ddlStatus         = ann.Status;
    Attachments       = ann.Attachments.OrderBy(a => a.SortOrder).ToList();
    CreatedBy         = ann.CreatedBy;
    CreatedAt         = ann.CreatedAt;
    UpdatedBy         = ann.UpdatedBy;
    UpdatedAt         = ann.UpdatedAt;

    TranslationInputs = await BuildInputsAsync(ann.Translations);
    await BuildSelectsAsync();
    await LoadDeleteMessagesAsync(ann.Translations.FirstOrDefault(t => t.LanguageCode == "en")?.Title ?? announcementCode);
    return Page();
  }

  public async Task<IActionResult> OnPostUpdateAsync(string announcementCode)
  {
    TranslationInputs = await BuildInputsAsync(null);
    await BuildSelectsAsync();
    AnnouncementCode  = announcementCode;

    if (!DateTime.TryParseExact(txtStartAt, AppConstants.DateTimeInputFormat,
            null, System.Globalization.DateTimeStyles.None, out var startLocal) ||
        !DateTime.TryParseExact(txtEndAt, AppConstants.DateTimeInputFormat,
            null, System.Globalization.DateTimeStyles.None, out var endLocal))
    {
      SetError(await _translation.GetAsync(MessageConstants.RequiredField));
      Attachments = await _dbHelper.GetAttachmentsAsync(announcementCode);
      return Page();
    }

    if (startLocal >= endLocal)
    {
      SetError(await _translation.GetAsync("Announcement.DateRangeError"));
      Attachments = await _dbHelper.GetAttachmentsAsync(announcementCode);
      return Page();
    }

    var startUtc = startLocal.ToUtcFromUserTimezone(UserTimezone);
    var endUtc   = endLocal.ToUtcFromUserTimezone(UserTimezone);

    var translations = TranslationInputs.Select(i => new AnnouncementTranslation
    {
      AnnouncementCode = announcementCode,
      LanguageCode     = i.LanguageCode,
      Title            = Request.Form[$"txtTitle_{i.LanguageCode}"].ToString().Trim(),
      Body             = Request.Form[$"hdnBody_{i.LanguageCode}"].ToString()
    }).ToList();

    var ann = new Announcement
    {
      AnnouncementCode = announcementCode,
      Audience         = ddlAudience,
      StartAt          = startUtc,
      EndAt            = endUtc,
      SortOrder        = txtSortOrder,
      Status           = ddlStatus
    };

    await _dbHelper.UpdateAsync(ann, translations, CurrentUsername);
    await SaveAttachmentsAsync(announcementCode);

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.UpdateSuccess);
    return RedirectToPage(Routes.AdminAnnouncements);
  }

  public async Task<IActionResult> OnPostSoftDeleteAsync(string announcementCode)
  {
    try
    {
      await _dbHelper.UpdateStatusAsync(announcementCode, StatusConstants.Deleted, CurrentUsername);
      var msg = await _translation.GetAsync(MessageConstants.DeleteSuccess);
      return new JsonResult(new { success = true, message = msg });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.DeleteError);
      return new JsonResult(new { success = false, message = msg });
    }
  }

  public async Task<IActionResult> OnPostDeleteAttachmentAsync(int attachmentId)
  {
    try
    {
      var att = await _dbHelper.DeleteAttachmentAsync(attachmentId);
      if (att != null)
      {
        var relPath  = _config["UploadPaths:Announcement"] ?? "uploads/announcements";
        var fullPath = Path.Combine(_env.WebRootPath, relPath.Replace('/', Path.DirectorySeparatorChar), att.FileName);
        if (System.IO.File.Exists(fullPath))
          System.IO.File.Delete(fullPath);
      }
      return new JsonResult(new { success = true, fileName = att?.FileName });
    }
    catch
    {
      return new JsonResult(new { success = false });
    }
  }

  private async Task SaveAttachmentsAsync(string announcementCode)
  {
    if (fileAttachments == null || fileAttachments.Count == 0) return;

    var relPath  = _config["UploadPaths:Announcement"] ?? "uploads/announcements";
    var fullPath = Path.Combine(_env.WebRootPath, relPath.Replace('/', Path.DirectorySeparatorChar));
    var existing = await _dbHelper.GetAttachmentsAsync(announcementCode);
    var order    = existing.Count;

    foreach (var file in fileAttachments)
    {
      if (file.Length == 0) continue;
      try
      {
        var (fileName, fileType) = await AnnouncementFileHelper.SaveAsync(file, fullPath);
        await _dbHelper.AddAttachmentAsync(new AnnouncementAttachment
        {
          AnnouncementCode = announcementCode,
          FileName         = fileName,
          OriginalName     = file.FileName,
          FileType         = fileType,
          SortOrder        = order++,
          CreatedBy        = CurrentUsername,
          CreatedAt        = DateTime.UtcNow
        });
      }
      catch { /* skip invalid files silently */ }
    }
  }

  private async Task<List<AnnouncementTranslationInputDto>> BuildInputsAsync(
      IEnumerable<AnnouncementTranslation>? existing)
  {
    var languages = await _languageDbHelper.GetAllActiveAsync();
    return languages.Select(l => new AnnouncementTranslationInputDto
    {
      LanguageCode = l.LanguageCode,
      Label        = l.LanguageName,
      Title        = existing?.FirstOrDefault(t => t.LanguageCode == l.LanguageCode)?.Title ?? string.Empty,
      Body         = existing?.FirstOrDefault(t => t.LanguageCode == l.LanguageCode)?.Body  ?? string.Empty
    }).ToList();
  }

  private async Task BuildSelectsAsync()
  {
    AudienceOptions = new List<SelectListItem>
    {
      new() { Value = AnnouncementConstants.AudienceAll,      Text = await _translation.GetAsync("Announcement.AudienceAll") },
      new() { Value = AnnouncementConstants.AudienceAdmin,    Text = await _translation.GetAsync("Announcement.AudienceAdmin") },
      new() { Value = AnnouncementConstants.AudienceCustomer, Text = await _translation.GetAsync("Announcement.AudienceCustomer") }
    };
    StatusOptions = await SelectListHelper.GetStatusOptions(_translation);
  }

  private async Task LoadDeleteMessagesAsync(string entityName)
  {
    MsgDeleteConfirmTitle = $"{await _translation.GetAsync("Confirm.DeleteTitle")} {entityName}";
    MsgDeleteConfirmText  = await _translation.GetAsync("Confirm.DeleteText");
    MsgDeleteConfirmBtn   = await _translation.GetAsync("Btn.YesDelete");
    MsgCancelBtn          = await _translation.GetAsync("Btn.Cancel");
    MsgDeleteSuccess      = await _translation.GetAsync(MessageConstants.DeleteSuccess);
    MsgDeleteError        = await _translation.GetAsync(MessageConstants.DeleteError);
    LabelDelete           = await _translation.GetAsync("Btn.Delete");
  }

  private void SetError(string message)
  {
    AlertMessageType    = MessageType.Error;
    AlertMessageTitle   = MessageTitle.Error;
    AlertMessageContent = message;
  }
}
