using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.AdminUsers;

public class CreateModel : AdminPageModel
{
  private readonly AdminUserDbHelper   _adminUserDbHelper;
  private readonly RoleDbHelper        _roleDbHelper;
  private readonly DepartmentDbHelper  _deptDbHelper;
  private readonly CountryDbHelper     _countryDbHelper;
  private readonly RegionDbHelper      _regionDbHelper;
  private readonly TranslationService  _translation;
  private readonly IWebHostEnvironment _env;
  private readonly IConfiguration      _config;

  [BindProperty] public string    txtFullName              { get; set; } = string.Empty;
  [BindProperty] public string    txtUsername              { get; set; } = string.Empty;
  [BindProperty] public string    txtEmail                 { get; set; } = string.Empty;
  [BindProperty] public string    txtPassword              { get; set; } = string.Empty;
  [BindProperty] public string    txtConfirmPassword       { get; set; } = string.Empty;
  [BindProperty] public int       ddlRoleId                { get; set; }
  [BindProperty] public int?      ddlDeptId                { get; set; }
  [BindProperty] public string    ddlCountryCode           { get; set; } = "MY";
  [BindProperty] public int?      ddlRegionId              { get; set; }
  [BindProperty] public string    txtMobileCountryCode     { get; set; } = string.Empty;
  [BindProperty] public string    txtMobileNo              { get; set; } = string.Empty;
  [BindProperty] public bool      chkIsForceChangePassword { get; set; }
  [BindProperty] public string    ddlStatus                { get; set; } = StatusConstants.Active;
  [BindProperty] public IFormFile? fileProfileImage        { get; set; }

  public List<SelectListItem> RoleOptions    { get; set; } = new();
  public List<SelectListItem> DeptOptions    { get; set; } = new();
  public List<SelectListItem> CountryOptions { get; set; } = new();
  public List<SelectListItem> RegionOptions  { get; set; } = new();
  public List<SelectListItem> StatusOptions  { get; set; } = new();

  public CreateModel(
      AdminUserDbHelper   adminUserDbHelper,
      RoleDbHelper        roleDbHelper,
      DepartmentDbHelper  deptDbHelper,
      CountryDbHelper     countryDbHelper,
      RegionDbHelper      regionDbHelper,
      TranslationService  translation,
      IWebHostEnvironment env,
      IConfiguration      config)
  {
    _adminUserDbHelper = adminUserDbHelper;
    _roleDbHelper      = roleDbHelper;
    _deptDbHelper      = deptDbHelper;
    _countryDbHelper   = countryDbHelper;
    _regionDbHelper    = regionDbHelper;
    _translation       = translation;
    _env               = env;
    _config            = config;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";
    await PopulateDropdownsAsync();
  }

  public async Task<IActionResult> OnPostCreateAsync()
  {
    await PopulateDropdownsAsync();

    if (string.IsNullOrWhiteSpace(txtFullName) || string.IsNullOrWhiteSpace(txtUsername) ||
        string.IsNullOrWhiteSpace(txtEmail)    || string.IsNullOrWhiteSpace(txtPassword)  || ddlRoleId == 0)
    {
      SetError(await _translation.GetAsync(MessageConstants.RequiredField));
      return Page();
    }

    if (await _adminUserDbHelper.IsUsernameExistsAsync(txtUsername.Trim()))
    {
      SetError(await _translation.GetAsync("AdminUser.DuplicateUsername"));
      return Page();
    }

    if (await _adminUserDbHelper.IsEmailExistsAsync(txtEmail.Trim()))
    {
      SetError(await _translation.GetAsync("AdminUser.DuplicateEmail"));
      return Page();
    }

    string? profileImage = null;
    if (fileProfileImage != null && fileProfileImage.Length > 0)
    {
      var relPath  = _config["UploadPaths:AdminProfile"] ?? "uploads/admin-profiles";
      var fullPath = Path.Combine(_env.WebRootPath, relPath.Replace('/', Path.DirectorySeparatorChar));
      profileImage = await ProfileImageHelper.SaveProfileImageAsync(fileProfileImage, txtUsername.Trim(), fullPath);
    }

    var user = new AdminUser
    {
      Username               = txtUsername.Trim(),
      PasswordHash           = PasswordCryptoHelper.Encrypt(txtPassword),
      FullName               = txtFullName.Trim(),
      Email                  = txtEmail.Trim(),
      RoleId                 = ddlRoleId,
      DeptId                 = ddlDeptId,
      CountryCode            = ddlCountryCode,
      RegionId               = ddlRegionId,
      MobileCountryCode      = string.IsNullOrWhiteSpace(txtMobileCountryCode) ? null : txtMobileCountryCode.Trim(),
      MobileNo               = string.IsNullOrWhiteSpace(txtMobileNo) ? null : txtMobileNo.Trim(),
      IsForceChangePassword  = chkIsForceChangePassword,
      ProfileImage           = profileImage,
      Status                 = ddlStatus,
      CreatedBy              = CurrentUsername,
      UpdatedBy              = CurrentUsername
    };

    await _adminUserDbHelper.AddAsync(user);

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.SaveSuccess);
    return RedirectToPage(Routes.AdminAdminUsers);
  }

  private async Task PopulateDropdownsAsync()
  {
    var langCode = string.IsNullOrEmpty(CurrentLangCode) ? "en" : CurrentLangCode;
    var roles    = await _roleDbHelper.GetAllActiveAsync();
    var depts    = await _deptDbHelper.GetAllActiveAsync();
    var regions  = await _regionDbHelper.GetAllActiveAsync();

    RoleOptions    = roles.Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.RoleName }).ToList();
    DeptOptions    = depts.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.DeptName }).ToList();
    CountryOptions = await SelectListHelper.GetCountryOptions(_countryDbHelper, langCode);
    RegionOptions  = regions.Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.RegionName }).ToList();
    StatusOptions  = await SelectListHelper.GetStatusOptions(_translation);
  }

  private void SetError(string message)
  {
    AlertMessageType    = MessageType.Error;
    AlertMessageTitle   = MessageTitle.Error;
    AlertMessageContent = message;
  }
}
