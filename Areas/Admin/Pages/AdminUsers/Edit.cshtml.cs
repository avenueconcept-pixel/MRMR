using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.AdminUsers;

public class EditModel : AdminPageModel
{
  private readonly AdminUserDbHelper   _adminUserDbHelper;
  private readonly RoleDbHelper        _roleDbHelper;
  private readonly CountryDbHelper     _countryDbHelper;
  private readonly TranslationService  _translation;
  private readonly IWebHostEnvironment _env;
  private readonly IConfiguration      _config;

  [BindProperty] public string    txtFullName              { get; set; } = string.Empty;
  [BindProperty] public string    txtEmail                 { get; set; } = string.Empty;
  [BindProperty] public int       ddlRoleId                { get; set; }
  [BindProperty] public string    ddlCountryCode           { get; set; } = "MY";
  [BindProperty] public string    txtMobileCountryCode     { get; set; } = string.Empty;
  [BindProperty] public string    txtMobileNo              { get; set; } = string.Empty;
  [BindProperty] public bool      chkIsForceChangePassword { get; set; }
  [BindProperty] public string    ddlStatus                { get; set; } = StatusConstants.Active;
  [BindProperty] public IFormFile? fileProfileImage        { get; set; }

  public int       Id            { get; set; }
  public string    Username      { get; set; } = string.Empty;
  public string?   ProfileImage  { get; set; }
  public string    CreatedBy     { get; set; } = string.Empty;
  public DateTime  CreatedAt     { get; set; }
  public string    UpdatedBy     { get; set; } = string.Empty;
  public DateTime  UpdatedAt     { get; set; }
  public DateTime? LastLoginAt   { get; set; }
  public string    LastLoginLang { get; set; } = string.Empty;

  public List<SelectListItem> RoleOptions    { get; set; } = new();
  public List<SelectListItem> CountryOptions { get; set; } = new();
  public List<SelectListItem> StatusOptions  { get; set; } = new();

  public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
  public string MsgDeleteConfirmText  { get; set; } = string.Empty;
  public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
  public string MsgCancelBtn          { get; set; } = string.Empty;
  public string MsgDeleteSuccess      { get; set; } = string.Empty;
  public string MsgDeleteError        { get; set; } = string.Empty;
  public string LabelDelete           { get; set; } = string.Empty;

  public EditModel(
      AdminUserDbHelper   adminUserDbHelper,
      RoleDbHelper        roleDbHelper,
      CountryDbHelper     countryDbHelper,
      TranslationService  translation,
      IWebHostEnvironment env,
      IConfiguration      config)
  {
    _adminUserDbHelper = adminUserDbHelper;
    _roleDbHelper      = roleDbHelper;
    _countryDbHelper   = countryDbHelper;
    _translation       = translation;
    _env               = env;
    _config            = config;
  }

  public async Task<IActionResult> OnGetAsync(int id)
  {
    AlertMessageType = "";

    var user = await _adminUserDbHelper.GetByIdAsync(id);
    if (user == null)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.NotFound);
      return RedirectToPage(Routes.AdminAdminUsers);
    }

    Id                       = user.Id;
    Username                 = user.Username;
    txtFullName              = user.FullName;
    txtEmail                 = user.Email;
    ddlRoleId                = user.RoleId;
    ddlCountryCode           = user.CountryCode;
    txtMobileCountryCode     = user.MobileCountryCode ?? string.Empty;
    txtMobileNo              = user.MobileNo          ?? string.Empty;
    chkIsForceChangePassword = user.IsForceChangePassword;
    ddlStatus                = user.Status;
    ProfileImage             = user.ProfileImage;
    CreatedBy                = user.CreatedBy;
    CreatedAt                = user.CreatedAt;
    UpdatedBy                = user.UpdatedBy;
    UpdatedAt                = user.UpdatedAt;
    LastLoginAt              = user.LastLoginAt;
    LastLoginLang            = user.LastLoginLang ?? string.Empty;

    await PopulateDropdownsAsync();

    MsgDeleteConfirmTitle = $"{await _translation.GetAsync("Confirm.DeleteTitle")} {user.FullName}";
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
    await PopulateDropdownsAsync();

    if (await _adminUserDbHelper.IsEmailExistsAsync(txtEmail.Trim(), id))
    {
      SetError(await _translation.GetAsync("AdminUser.DuplicateEmail"));
      var existing2 = await _adminUserDbHelper.GetByIdAsync(id);
      Id           = id;
      Username     = existing2?.Username    ?? string.Empty;
      ProfileImage = existing2?.ProfileImage;
      return Page();
    }

    var existing = await _adminUserDbHelper.GetByIdAsync(id);
    if (existing == null)
      return RedirectToPage(Routes.AdminAdminUsers);

    string? newProfileImage = existing.ProfileImage;

    if (fileProfileImage != null && fileProfileImage.Length > 0)
    {
      var relPath  = _config["UploadPaths:AdminProfile"] ?? "uploads/admin-profiles";
      var fullPath = Path.Combine(_env.WebRootPath, relPath.Replace('/', Path.DirectorySeparatorChar));

      if (!string.IsNullOrEmpty(existing.ProfileImage))
      {
        var oldFile = Path.Combine(fullPath, existing.ProfileImage);
        if (System.IO.File.Exists(oldFile))
          System.IO.File.Delete(oldFile);
      }

      newProfileImage = await ProfileImageHelper.SaveProfileImageAsync(fileProfileImage, existing.Username, fullPath);
    }

    var updated = new AdminUser
    {
      Id                     = id,
      FullName               = txtFullName.Trim(),
      Email                  = txtEmail.Trim(),
      RoleId                 = ddlRoleId,
      CountryCode            = ddlCountryCode,
      MobileCountryCode      = string.IsNullOrWhiteSpace(txtMobileCountryCode) ? null : txtMobileCountryCode.Trim(),
      MobileNo               = string.IsNullOrWhiteSpace(txtMobileNo) ? null : txtMobileNo.Trim(),
      IsForceChangePassword  = chkIsForceChangePassword,
      ProfileImage           = newProfileImage,
      Status                 = ddlStatus,
      UpdatedBy              = CurrentUsername
    };

    await _adminUserDbHelper.UpdateAsync(updated);

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.UpdateSuccess);
    return RedirectToPage(Routes.AdminAdminUsers);
  }

  public async Task<IActionResult> OnPostSoftDeleteAsync(int id)
  {
    try
    {
      await _adminUserDbHelper.SoftDeleteAsync(id, CurrentUsername);
      var msg = await _translation.GetAsync(MessageConstants.DeleteSuccess);
      return new JsonResult(new { success = true, message = msg });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.DeleteError);
      return new JsonResult(new { success = false, message = msg });
    }
  }

  private async Task PopulateDropdownsAsync()
  {
    var langCode   = string.IsNullOrEmpty(CurrentLangCode) ? "en" : CurrentLangCode;
    var roles      = await _roleDbHelper.GetAllActiveAsync();
    RoleOptions    = roles.Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.RoleName }).ToList();
    CountryOptions = await SelectListHelper.GetCountryOptions(_countryDbHelper, langCode);
    StatusOptions  = await SelectListHelper.GetStatusOptions(_translation);
  }

  private void SetError(string message)
  {
    AlertMessageType    = MessageType.Error;
    AlertMessageTitle   = MessageTitle.Error;
    AlertMessageContent = message;
  }
}
