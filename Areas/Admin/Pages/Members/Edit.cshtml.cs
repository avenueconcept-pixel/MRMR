using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Members;

public class EditModel : AdminPageModel
{
  private readonly MemberDbHelper     _memberDbHelper;
  private readonly CountryDbHelper    _countryDbHelper;
  private readonly TranslationService _translation;
  private readonly IWebHostEnvironment _env;
  private readonly IConfiguration      _config;

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
  [BindProperty] public IFormFile? fileProfileImage  { get; set; }

  public int      MemberId         { get; set; }
  public string   Username         { get; set; } = string.Empty;
  public string?  ProfileImage     { get; set; }
  public string   SponsorUsername  { get; set; } = string.Empty;
  public string   BinaryParentUsername { get; set; } = string.Empty;
  public string?  BinaryPosition   { get; set; }
  public bool     IsActivated      { get; set; }
  public string?  CurrentRankCode  { get; set; }
  public string?  HighestRankCode  { get; set; }
  public DateTime JoinedAt         { get; set; }
  public string   Status           { get; set; } = string.Empty;
  public string   CreatedBy        { get; set; } = string.Empty;
  public DateTime CreatedAt        { get; set; }
  public string   UpdatedBy        { get; set; } = string.Empty;
  public DateTime UpdatedAt        { get; set; }

  public List<SelectListItem> ddlCountry { get; set; } = new();
  public List<SelectListItem> ddlIdTypes { get; set; } = new();

  public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
  public string MsgDeleteConfirmText  { get; set; } = string.Empty;
  public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
  public string MsgCancelBtn          { get; set; } = string.Empty;
  public string MsgDeleteSuccess      { get; set; } = string.Empty;
  public string MsgDeleteError        { get; set; } = string.Empty;
  public string LabelDelete           { get; set; } = string.Empty;

  public EditModel(
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

  public async Task<IActionResult> OnGetAsync(int id)
  {
    AlertMessageType = "";

    var member = await _memberDbHelper.GetByIdAsync(id);
    if (member == null)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.NotFound);
      return RedirectToPage(Routes.AdminMembers);
    }

    await PopulateAsync(member);
    await LoadDeleteMessagesAsync(member.FullName);

    return Page();
  }

  public async Task<IActionResult> OnPostUpdateAsync(int id)
  {
    await PopulateDropdownsAsync();

    var member = new Member
    {
      Id              = id,
      FullName        = txtFullName.Trim(),
      IdType          = ddlIdType,
      IdNo            = txtIdNo.Trim(),
      Email           = txtEmail.Trim(),
      PhoneCountryCode = txtPhoneCountryCode.Trim(),
      PhoneNumber     = txtPhoneNumber.Trim(),
      AddressLine1    = txtAddressLine1.Trim(),
      AddressLine2    = string.IsNullOrWhiteSpace(txtAddressLine2) ? null : txtAddressLine2.Trim(),
      City            = txtCity.Trim(),
      State           = txtState.Trim(),
      Postcode        = txtPostcode.Trim(),
      CountryCode     = ddlCountryCode,
      BankName        = string.IsNullOrWhiteSpace(txtBankName) ? null : txtBankName.Trim(),
      BankAccountName = string.IsNullOrWhiteSpace(txtBankAccountName) ? null : txtBankAccountName.Trim(),
      BankAccountNo   = string.IsNullOrWhiteSpace(txtBankAccountNo) ? null : txtBankAccountNo.Trim()
    };

    await _memberDbHelper.UpdateAsync(member, CurrentUsername);

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = await _translation.GetAsync(MessageConstants.UpdateSuccess);

    return RedirectToPage(Routes.AdminMembersEdit, new { id });
  }

  public async Task<IActionResult> OnPostUploadImageAsync(int id)
  {
    if (fileProfileImage == null || fileProfileImage.Length == 0)
    {
      var msg = await _translation.GetAsync(MessageConstants.RequiredField);
      return new JsonResult(new { success = false, message = msg });
    }

    try
    {
      var existing = await _memberDbHelper.GetByIdAsync(id);
      if (existing == null)
        return new JsonResult(new { success = false });

      var relPath  = _config["UploadPaths:MemberProfile"] ?? "uploads/member-profiles";
      var fullPath = Path.Combine(_env.WebRootPath, relPath.Replace('/', Path.DirectorySeparatorChar));

      if (!string.IsNullOrEmpty(existing.ProfileImage))
      {
        var oldFile = Path.Combine(fullPath, existing.ProfileImage);
        if (System.IO.File.Exists(oldFile))
          System.IO.File.Delete(oldFile);
      }

      var filename = await ProfileImageHelper.SaveProfileImageAsync(fileProfileImage, existing.Username, fullPath);
      await _memberDbHelper.UpdateProfileImageAsync(id, filename, CurrentUsername);

      var successMsg = await _translation.GetAsync(MessageConstants.UpdateSuccess);
      return new JsonResult(new { success = true, message = successMsg, filename });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.SaveError);
      return new JsonResult(new { success = false, message = msg });
    }
  }

  public async Task<IActionResult> OnPostSoftDeleteAsync(int id)
  {
    try
    {
      await _memberDbHelper.UpdateStatusAsync(id, StatusConstants.Deleted, CurrentUsername);
      var msg = await _translation.GetAsync(MessageConstants.DeleteSuccess);
      return new JsonResult(new { success = true, message = msg });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.DeleteError);
      return new JsonResult(new { success = false, message = msg });
    }
  }

  private async Task PopulateAsync(Member member)
  {
    MemberId              = member.Id;
    Username              = member.Username;
    ProfileImage          = member.ProfileImage;
    txtFullName           = member.FullName;
    ddlIdType             = member.IdType;
    txtIdNo               = member.IdNo;
    txtEmail              = member.Email;
    txtPhoneCountryCode   = member.PhoneCountryCode;
    txtPhoneNumber        = member.PhoneNumber;
    ddlCountryCode        = member.CountryCode;
    txtAddressLine1       = member.AddressLine1;
    txtAddressLine2       = member.AddressLine2;
    txtCity               = member.City;
    txtState              = member.State;
    txtPostcode           = member.Postcode;
    txtBankName           = member.BankName;
    txtBankAccountName    = member.BankAccountName;
    txtBankAccountNo      = member.BankAccountNo;
    SponsorUsername       = member.Sponsor?.Username ?? "-";
    BinaryParentUsername  = member.BinaryParent?.Username ?? "-";
    BinaryPosition        = member.BinaryPosition;
    IsActivated           = member.IsActivated;
    CurrentRankCode       = member.CurrentRankCode;
    HighestRankCode       = member.HighestRankCode;
    JoinedAt              = member.JoinedAt;
    Status                = member.Status;
    CreatedBy             = member.CreatedBy;
    CreatedAt             = member.CreatedAt;
    UpdatedBy             = member.UpdatedBy;
    UpdatedAt             = member.UpdatedAt;

    await PopulateDropdownsAsync();
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

  private async Task LoadDeleteMessagesAsync(string displayName)
  {
    MsgDeleteConfirmTitle = $"{await _translation.GetAsync("Confirm.DeleteTitle")} {displayName}";
    MsgDeleteConfirmText  = await _translation.GetAsync("Confirm.DeleteText");
    MsgDeleteConfirmBtn   = await _translation.GetAsync("Btn.YesDelete");
    MsgCancelBtn          = await _translation.GetAsync("Btn.Cancel");
    MsgDeleteSuccess      = await _translation.GetAsync(MessageConstants.DeleteSuccess);
    MsgDeleteError        = await _translation.GetAsync(MessageConstants.DeleteError);
    LabelDelete           = await _translation.GetAsync("Btn.Delete");
  }
}
