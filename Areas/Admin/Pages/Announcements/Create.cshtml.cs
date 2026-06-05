using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Announcements;

public class CreateModel : AdminPageModel
{
  private readonly AnnouncementDbHelper _dbHelper;
  private readonly LanguageDbHelper     _languageDbHelper;
  private readonly TranslationService   _translation;
  private readonly IWebHostEnvironment  _env;
  private readonly IConfiguration       _config;

  [BindProperty] public string            txtAnnouncementCode { get; set; } = string.Empty;
  [BindProperty] public string            ddlAudience         { get; set; } = AnnouncementConstants.AudienceAll;
  [BindProperty] public string            txtStartAt          { get; set; } = string.Empty;
  [BindProperty] public string            txtEndAt            { get; set; } = string.Empty;
  [BindProperty] public int               txtSortOrder        { get; set; } = 0;
  [BindProperty] public string            ddlStatus           { get; set; } = StatusConstants.Active;
  [BindProperty] public List<IFormFile>?  fileAttachments     { get; set; }

  public List<AnnouncementTranslationInputDto> TranslationInputs { get; set; } = new();
  public List<SelectListItem>                  AudienceOptions   { get; set; } = new();
  public List<SelectListItem>                  StatusOptions      { get; set; } = new();

  public CreateModel(
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

  public async Task OnGetAsync()
  {
    AlertMessageType  = "";
    TranslationInputs = await BuildInputsAsync(null);
    await BuildSelectsAsync();
  }

  public async Task<IActionResult> OnPostCreateAsync()
  {
    TranslationInputs = await BuildInputsAsync(null);
    await BuildSelectsAsync();

    if (string.IsNullOrWhiteSpace(txtAnnouncementCode))
    {
      SetError(await _translation.GetAsync(MessageConstants.RequiredField));
      return Page();
    }

    if (!DateTime.TryParseExact(txtStartAt, AppConstants.DateTimeInputFormat,
            null, System.Globalization.DateTimeStyles.None, out var startLocal) ||
        !DateTime.TryParseExact(txtEndAt, AppConstants.DateTimeInputFormat,
            null, System.Globalization.DateTimeStyles.None, out var endLocal))
    {
      SetError(await _translation.GetAsync(MessageConstants.RequiredField));
      return Page();
    }

    if (startLocal >= endLocal)
    {
      SetError(await _translation.GetAsync("Announcement.DateRangeError"));
      return Page();
    }

    var startUtc = startLocal.ToUtcFromUserTimezone(UserTimezone);
    var endUtc   = endLocal.ToUtcFromUserTimezone(UserTimezone);

    var translations = TranslationInputs.Select(i => new AnnouncementTranslation
    {
      LanguageCode = i.LanguageCode,
      Title        = Request.Form[$"txtTitle_{i.LanguageCode}"].ToString().Trim(),
      Body         = Request.Form[$"hdnBody_{i.LanguageCode}"].ToString()
    }).ToList();

    var ann = new Announcement
    {
      AnnouncementCode = txtAnnouncementCode.Trim().ToUpper(),
      Audience         = ddlAudience,
      StartAt          = startUtc,
      EndAt            = endUtc,
      SortOrder        = txtSortOrder,
      Status           = ddlStatus
    };

    var result = await _dbHelper.AddAsync(ann, translations, CurrentUsername);
    if (result == AnnouncementAddResult.DuplicateActive)
    {
      SetError(await _translation.GetAsync("Announcement.DuplicateCode"));
      return Page();
    }

    await SaveAttachmentsAsync(ann.AnnouncementCode);

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.SaveSuccess);
    return RedirectToPage(Routes.AdminAnnouncements);
  }

  private async Task SaveAttachmentsAsync(string announcementCode)
  {
    if (fileAttachments == null || fileAttachments.Count == 0) return;

    var relPath  = _config["UploadPaths:Announcement"] ?? "uploads/announcements";
    var fullPath = Path.Combine(_env.WebRootPath, relPath.Replace('/', Path.DirectorySeparatorChar));
    var order    = 0;

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
      IList<AnnouncementTranslation>? existing)
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
    AudienceOptions = await GetAudienceOptions();
    StatusOptions   = await SelectListHelper.GetStatusOptions(_translation);
  }

  private async Task<List<SelectListItem>> GetAudienceOptions() => new()
  {
    new() { Value = AnnouncementConstants.AudienceAll,      Text = await _translation.GetAsync("Announcement.AudienceAll") },
    new() { Value = AnnouncementConstants.AudienceAdmin,    Text = await _translation.GetAsync("Announcement.AudienceAdmin") },
    new() { Value = AnnouncementConstants.AudienceCustomer, Text = await _translation.GetAsync("Announcement.AudienceCustomer") }
  };

  private void SetError(string message)
  {
    AlertMessageType    = MessageType.Error;
    AlertMessageTitle   = MessageTitle.Error;
    AlertMessageContent = message;
  }
}
