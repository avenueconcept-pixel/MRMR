using Microsoft.AspNetCore.Mvc;
using MyApp.Constants;
using MyApp.Constants.MRMR;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Helper.DB.MRMR;
using MyApp.Models;
using MyApp.Models.MRMR;
using MyApp.Services;

namespace MyApp.Areas.Applicant.Pages;

public class RegisterModel : PublicPageModel
{
    private readonly RegistrationDbHelper       _dbHelper;
    private readonly TranslationService         _translation;
    private readonly SystemSettingService       _systemSetting;
    private readonly CompanyBankAccountDbHelper _bankAccountDbHelper;

    public RegisterModel(
        RegistrationDbHelper       dbHelper,
        TranslationService         translation,
        SystemSettingService       systemSetting,
        CompanyBankAccountDbHelper bankAccountDbHelper)
    {
        _dbHelper            = dbHelper;
        _translation         = translation;
        _systemSetting       = systemSetting;
        _bankAccountDbHelper = bankAccountDbHelper;
    }

    [BindProperty] public string  ddlTitle                { get; set; } = string.Empty;
    [BindProperty] public string  txtFullName             { get; set; } = string.Empty;
    [BindProperty] public string  txtNric                 { get; set; } = string.Empty;
    [BindProperty] public string  txtContactNo            { get; set; } = string.Empty;
    [BindProperty] public string  txtEmail                { get; set; } = string.Empty;
    [BindProperty] public string? txtCompanyName          { get; set; }
    [BindProperty] public string? txtSsmRegNo             { get; set; }
    [BindProperty] public string? txtCompanyAddress       { get; set; }
    [BindProperty] public string? txtWebsite              { get; set; }
    [BindProperty] public string? ddlIndustry             { get; set; }
    [BindProperty] public string? txtBusinessNature       { get; set; }
    [BindProperty] public string  ddlApplicationType      { get; set; } = string.Empty;
    [BindProperty] public int     ddlAwardCategory        { get; set; }
    [BindProperty] public string  ddlPaymentMethod        { get; set; } = string.Empty;
    [BindProperty] public bool    chkDeclInfoAccurate     { get; set; }
    [BindProperty] public bool    chkDeclFeeNonrefundable { get; set; }

    public List<CategorySummaryDto>    IndividualCategories      { get; set; } = new();
    public List<CategorySummaryDto>    CorporateCategories       { get; set; } = new();
    public bool                        ManualBankTransferEnabled  { get; set; }
    public List<CompanyBankAccount>    BankAccounts              { get; set; } = new();

    public string MsgDuplicateEmail           { get; set; } = string.Empty;
    public string MsgDuplicateNric            { get; set; } = string.Empty;
    public string MsgDuplicateApplicationType { get; set; } = string.Empty;
    public string MsgAlreadyVerified          { get; set; } = string.Empty;
    public string MsgError                    { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        await LoadPageDataAsync();
    }

    public async Task<IActionResult> OnPostRegisterAsync()
    {
        await LoadPageDataAsync();

        if (string.IsNullOrWhiteSpace(ddlTitle)           || string.IsNullOrWhiteSpace(txtFullName) ||
            string.IsNullOrWhiteSpace(txtNric)            || string.IsNullOrWhiteSpace(txtContactNo) ||
            string.IsNullOrWhiteSpace(txtEmail)           || string.IsNullOrWhiteSpace(ddlApplicationType) ||
            ddlAwardCategory == 0                         || string.IsNullOrWhiteSpace(ddlPaymentMethod) ||
            !chkDeclInfoAccurate || !chkDeclFeeNonrefundable)
        {
            AlertMessageContent = await _translation.GetAsync(MessageConstants.RequiredField);
            AlertMessageType    = MessageType.Error;
            return Page();
        }

        if (ddlApplicationType == "Corporate" &&
            (string.IsNullOrWhiteSpace(txtCompanyName) || string.IsNullOrWhiteSpace(ddlIndustry)))
        {
            AlertMessageContent = await _translation.GetAsync(MessageConstants.RequiredField);
            AlertMessageType    = MessageType.Error;
            return Page();
        }

        if (ddlPaymentMethod == nameof(MyApp.Constants.MRMR.PaymentMethod.ManualBankTransfer) && !ManualBankTransferEnabled)
        {
            AlertMessageContent = await _translation.GetAsync(MessageConstants.RequiredField);
            AlertMessageType    = MessageType.Error;
            return Page();
        }

        var tempPassword = GenerateTempPassword();
        var nric         = txtNric.Trim();

        var registrant = new Registrant
        {
            Title                = ddlTitle,
            FullName             = txtFullName.Trim(),
            NricPassport         = nric,
            ContactNo            = txtContactNo.Trim(),
            Email                = txtEmail.Trim().ToLower(),
            CompanyName          = txtCompanyName?.Trim(),
            SsmRegNo             = txtSsmRegNo?.Trim(),
            CompanyAddress       = txtCompanyAddress?.Trim(),
            Website              = txtWebsite?.Trim(),
            Industry             = ddlIndustry,
            BusinessNature       = txtBusinessNature?.Trim(),
            DeclInfoAccurate     = chkDeclInfoAccurate,
            DeclFeeNonrefundable = chkDeclFeeNonrefundable,
            Username             = nric.Length > 25 ? nric[..25] : nric,
            PasswordHash         = BCrypt.Net.BCrypt.HashPassword(tempPassword),
            TempPassword         = tempPassword,
            Status               = "inactive",
            IsFirstLogin         = true,
            IsActive             = false
        };

        var (result, applicationDbId) = await _dbHelper.RegisterAsync(
            registrant, ddlApplicationType, ddlAwardCategory, ddlPaymentMethod);

        switch (result)
        {
            case RegistrationResult.Success:
                TempData["RegisteredApplicationId"] = applicationDbId;
                TempData["RegisteredPaymentMethod"]  = ddlPaymentMethod;
                return RedirectToPage("/Payment/Select", new { area = "Applicant" });

            case RegistrationResult.DuplicateEmail:
                AlertMessageContent = MsgDuplicateEmail;
                AlertMessageType    = MessageType.Error;
                break;

            case RegistrationResult.DuplicateNric:
                AlertMessageContent = MsgDuplicateNric;
                AlertMessageType    = MessageType.Error;
                break;

            case RegistrationResult.AlreadyVerified:
                AlertMessageContent = MsgAlreadyVerified;
                AlertMessageType    = MessageType.Error;
                break;

            default:
                AlertMessageContent = MsgError;
                AlertMessageType    = MessageType.Error;
                break;
        }

        return Page();
    }

    public async Task<IActionResult> OnGetCategoryDetailsAsync(int id)
    {
        var category = await _dbHelper.GetCategoryByIdAsync(id);
        if (category == null) return new JsonResult(new { success = false });

        return new JsonResult(new
        {
            success      = true,
            name         = category.Name,
            price        = category.Price,
            priceDisplay = $"RM {category.Price:N2}"
        });
    }

    private static string GenerateTempPassword()
    {
        const string chars = "ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
        var rng = System.Security.Cryptography.RandomNumberGenerator.GetBytes(10);
        return new string(rng.Select(b => chars[b % chars.Length]).ToArray());
    }

    private async Task LoadPageDataAsync()
    {
        IndividualCategories       = await _dbHelper.GetActiveCategoriesAsync("Individual");
        CorporateCategories        = await _dbHelper.GetActiveCategoriesAsync("Corporate");
        ManualBankTransferEnabled  = await _systemSetting.GetAsBoolAsync("Registration.ManualBankTransferEnabled", false);
        BankAccounts               = await _bankAccountDbHelper.GetAllActiveByCountryAsync("MY", _translation.CurrentLanguage);
        MsgDuplicateEmail           = await _translation.GetAsync("Registration.DuplicateEmail");
        MsgDuplicateNric            = await _translation.GetAsync("Registration.DuplicateNric");
        MsgDuplicateApplicationType = await _translation.GetAsync("Registration.DuplicateApplicationType");
        MsgAlreadyVerified          = await _translation.GetAsync("Registration.AlreadyVerified");
        MsgError                    = await _translation.GetAsync(MessageConstants.SaveError);
    }
}
