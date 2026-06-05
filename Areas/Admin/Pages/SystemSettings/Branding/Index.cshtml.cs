using Microsoft.AspNetCore.Mvc;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.SystemSettings.Branding;

public class IndexModel : AdminPageModel
{
  private readonly AppSettingsDbHelper _appSettingsDbHelper;
  private readonly AppSettingsService  _appSettingsService;
  private readonly IWebHostEnvironment _env;
  private readonly IConfiguration      _config;
  private readonly TranslationService  _translation;

  public IndexModel(
      AppSettingsDbHelper appSettingsDbHelper,
      AppSettingsService  appSettingsService,
      IWebHostEnvironment env,
      IConfiguration      config,
      TranslationService  translation)
  {
    _appSettingsDbHelper = appSettingsDbHelper;
    _appSettingsService  = appSettingsService;
    _env                 = env;
    _config              = config;
    _translation         = translation;
  }

  [BindProperty] public string     txtAdminSystemName    { get; set; } = string.Empty;
  [BindProperty] public string     txtCustomerSystemName { get; set; } = string.Empty;
  [BindProperty] public string     txtAdminFooter        { get; set; } = string.Empty;
  [BindProperty] public string     txtCustomerFooter     { get; set; } = string.Empty;
  [BindProperty] public IFormFile? fileLogoImage         { get; set; }

  public string CurrentLogoPath { get; set; } = string.Empty;

  public async Task OnGetAsync()
  {
    AlertMessageType = string.Empty;

    var branding          = await _appSettingsDbHelper.GetBrandingAsync();
    txtAdminSystemName    = branding.AdminSystemName;
    txtCustomerSystemName = branding.CustomerSystemName;
    txtAdminFooter        = branding.AdminFooterText;
    txtCustomerFooter     = branding.CustomerFooterText;
    CurrentLogoPath       = branding.LogoPath;
  }

  public async Task<IActionResult> OnPostSaveAsync()
  {
    if (string.IsNullOrWhiteSpace(txtAdminSystemName) || string.IsNullOrWhiteSpace(txtCustomerSystemName))
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.RequiredField);
      CurrentLogoPath     = (await _appSettingsDbHelper.GetBrandingAsync()).LogoPath;
      return Page();
    }

    if (fileLogoImage != null)
    {
      var ext = Path.GetExtension(fileLogoImage.FileName).ToLowerInvariant();
      if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
      {
        AlertMessageType    = MessageType.Error;
        AlertMessageContent = await _translation.GetAsync("SystemSettings.Branding.Error.InvalidFileType");
        CurrentLogoPath     = (await _appSettingsDbHelper.GetBrandingAsync()).LogoPath;
        return Page();
      }

      if (fileLogoImage.Length > 2 * 1024 * 1024)
      {
        AlertMessageType    = MessageType.Error;
        AlertMessageContent = await _translation.GetAsync("SystemSettings.Branding.Error.FileTooLarge");
        CurrentLogoPath     = (await _appSettingsDbHelper.GetBrandingAsync()).LogoPath;
        return Page();
      }

      var relPath  = _config["UploadPaths:Branding"] ?? "uploads/branding";
      var fullPath = Path.Combine(_env.WebRootPath, relPath.Replace('/', Path.DirectorySeparatorChar));
      Directory.CreateDirectory(fullPath);

      var oldLogo = await _appSettingsDbHelper.GetAsync("global", "logo_path");
      if (!string.IsNullOrEmpty(oldLogo))
      {
        var oldFile = Path.Combine(fullPath, oldLogo);
        if (System.IO.File.Exists(oldFile))
          System.IO.File.Delete(oldFile);
      }

      var filename = $"{Guid.NewGuid()}{ext}";
      using (var stream = new FileStream(Path.Combine(fullPath, filename), FileMode.Create))
        await fileLogoImage.CopyToAsync(stream);

      await _appSettingsDbHelper.SetAsync("global", "logo_path", filename, CurrentUsername);
    }

    await _appSettingsDbHelper.SetAsync("admin",    "system_name", txtAdminSystemName.Trim(),    CurrentUsername);
    await _appSettingsDbHelper.SetAsync("customer", "system_name", txtCustomerSystemName.Trim(), CurrentUsername);
    await _appSettingsDbHelper.SetAsync("admin",    "footer_text", txtAdminFooter,               CurrentUsername);
    await _appSettingsDbHelper.SetAsync("customer", "footer_text", txtCustomerFooter,            CurrentUsername);

    _appSettingsService.InvalidateCache();

    AlertMessageType    = MessageType.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.SaveSuccess);

    return RedirectToPage();
  }
}
