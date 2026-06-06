using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Members;

public class CreateModel : AdminPageModel
{
  private readonly MemberDbHelper     _memberDbHelper;
  private readonly CountryDbHelper    _countryDbHelper;
  private readonly TranslationService _translation;
  private readonly IWebHostEnvironment _env;
  private readonly IConfiguration      _config;

  [BindProperty] public string   txtUsername         { get; set; } = string.Empty;
  [BindProperty] public string   txtFullName         { get; set; } = string.Empty;
  [BindProperty] public string   ddlIdType           { get; set; } = string.Empty;
  [BindProperty] public string   txtIdNo             { get; set; } = string.Empty;
  [BindProperty] public string   txtEmail            { get; set; } = string.Empty;
  [BindProperty] public string   txtPhoneCountryCode { get; set; } = string.Empty;
  [BindProperty] public string   txtPhoneNumber      { get; set; } = string.Empty;
  [BindProperty] public string   ddlCountryCode      { get; set; } = string.Empty;
  [BindProperty] public string   txtAddressLine1     { get; set; } = string.Empty;
  [BindProperty] public string?  txtAddressLine2     { get; set; }
  [BindProperty] public string   txtCity             { get; set; } = string.Empty;
  [BindProperty] public string   txtState            { get; set; } = string.Empty;
  [BindProperty] public string   txtPostcode         { get; set; } = string.Empty;
  [BindProperty] public string?  txtBankName         { get; set; }
  [BindProperty] public string?  txtBankAccountName  { get; set; }
  [BindProperty] public string?  txtBankAccountNo    { get; set; }
  [BindProperty] public string   txtJoinedAt         { get; set; } = string.Empty;
  [BindProperty] public string   txtPassword         { get; set; } = string.Empty;
  [BindProperty] public string   txtConfirmPassword  { get; set; } = string.Empty;
  [BindProperty] public int?     SponsorId           { get; set; }
  [BindProperty] public string   ddlBinaryPlacement  { get; set; } = "auto";
  [BindProperty] public int?     BinaryParentId      { get; set; }
  [BindProperty] public string?  ddlBinaryPosition   { get; set; }
  [BindProperty] public IFormFile? fileProfileImage  { get; set; }

  public List<SelectListItem> ddlCountry { get; set; } = new();
  public List<SelectListItem> ddlIdTypes { get; set; } = new();

  public CreateModel(
      MemberDbHelper      memberDbHelper,
      CountryDbHelper     countryDbHelper,
      TranslationService  translation,
      IWebHostEnvironment env,
      IConfiguration      config)
  {
    _memberDbHelper  = memberDbHelper;
    _countryDbHelper = countryDbHelper;
    _translation     = translation;
    _env             = env;
    _config          = config;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";
    await PopulateDropdownsAsync();
  }

  public async Task<IActionResult> OnPostCreateAsync()
  {
    await PopulateDropdownsAsync();

    if (!await _memberDbHelper.IsUsernameUniqueAsync(txtUsername.Trim()))
    {
      SetError(await _translation.GetAsync("Members.Error.UsernameTaken"));
      return Page();
    }

    if (txtPassword != txtConfirmPassword)
    {
      SetError(await _translation.GetAsync("Members.Error.PasswordMismatch"));
      return Page();
    }

    int? binaryParentId   = BinaryParentId;
    string? binaryPosition = ddlBinaryPosition;

    if (ddlBinaryPlacement == "auto" && SponsorId.HasValue)
    {
      var slot = await _memberDbHelper.FindNextBinarySlotAsync(SponsorId.Value);
      if (slot != null)
      {
        binaryParentId   = slot.MemberId;
        binaryPosition   = slot.Position;
      }
    }

    string? profileImage = null;
    if (fileProfileImage != null && fileProfileImage.Length > 0)
    {
      var relPath  = _config["UploadPaths:MemberProfile"] ?? "uploads/member-profiles";
      var fullPath = Path.Combine(_env.WebRootPath, relPath.Replace('/', Path.DirectorySeparatorChar));
      profileImage = await ProfileImageHelper.SaveProfileImageAsync(fileProfileImage, txtUsername.Trim(), fullPath);
    }

    DateTime joinedAt = DateTime.UtcNow;
    if (!string.IsNullOrEmpty(txtJoinedAt) &&
        DateTime.TryParseExact(txtJoinedAt, AppConstants.DateInputFormat,
            null, System.Globalization.DateTimeStyles.None, out var parsedJoined))
    {
      joinedAt = parsedJoined;
    }

    var member = new Member
    {
      Username         = txtUsername.Trim(),
      FullName         = txtFullName.Trim(),
      IdType           = ddlIdType,
      IdNo             = txtIdNo.Trim(),
      Email            = txtEmail.Trim(),
      PhoneCountryCode = txtPhoneCountryCode.Trim(),
      PhoneNumber      = txtPhoneNumber.Trim(),
      ProfileImage     = profileImage,
      AddressLine1     = txtAddressLine1.Trim(),
      AddressLine2     = string.IsNullOrWhiteSpace(txtAddressLine2) ? null : txtAddressLine2.Trim(),
      City             = txtCity.Trim(),
      State            = txtState.Trim(),
      Postcode         = txtPostcode.Trim(),
      CountryCode      = ddlCountryCode,
      BankName         = string.IsNullOrWhiteSpace(txtBankName) ? null : txtBankName.Trim(),
      BankAccountName  = string.IsNullOrWhiteSpace(txtBankAccountName) ? null : txtBankAccountName.Trim(),
      BankAccountNo    = string.IsNullOrWhiteSpace(txtBankAccountNo) ? null : txtBankAccountNo.Trim(),
      SponsorId        = SponsorId,
      BinaryParentId   = binaryParentId,
      BinaryPosition   = binaryPosition,
      IsActivated      = false,
      JoinedAt         = joinedAt,
      PasswordHash     = PasswordCryptoHelper.Encrypt(txtPassword),
      Status           = StatusConstants.Active,
      CreatedBy        = CurrentUsername,
      UpdatedBy        = CurrentUsername
    };

    await _memberDbHelper.AddAsync(member);

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.SaveSuccess);

    return RedirectToPage(Routes.AdminMembersEdit, new { id = member.Id });
  }

  private async Task PopulateDropdownsAsync()
  {
    var langCode = string.IsNullOrEmpty(CurrentLangCode) ? "en" : CurrentLangCode;
    ddlCountry = await SelectListHelper.GetCountryOptions(_countryDbHelper, langCode);
    ddlIdTypes = new List<SelectListItem>
    {
      new() { Value = MemberConstants.IdTypeIc,       Text = MemberConstants.IdTypeIc },
      new() { Value = MemberConstants.IdTypePassport, Text = MemberConstants.IdTypePassport }
    };
  }

  private void SetError(string message)
  {
    AlertMessageType    = MessageType.Error;
    AlertMessageTitle   = MessageTitle.Error;
    AlertMessageContent = message;
  }
}
