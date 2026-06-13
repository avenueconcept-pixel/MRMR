using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.CompanyBankAccounts;

public class CreateModel : AdminPageModel
{
    private readonly CompanyBankAccountDbHelper _dbHelper;
    private readonly CountryDbHelper            _countryDbHelper;
    private readonly TranslationService         _translation;

    [BindProperty] public string ddlCountryCode   { get; set; } = string.Empty;
    [BindProperty] public string txtBankName      { get; set; } = string.Empty;
    [BindProperty] public string txtAccountName   { get; set; } = string.Empty;
    [BindProperty] public string txtAccountNumber { get; set; } = string.Empty;
    [BindProperty] public string txtBranch        { get; set; } = string.Empty;
    [BindProperty] public string txtCurrency      { get; set; } = string.Empty;
    [BindProperty] public string txtRemarks       { get; set; } = string.Empty;
    [BindProperty] public string ddlStatus        { get; set; } = StatusConstants.Active;

    public List<SelectListItem> CountryOptions { get; set; } = new();
    public List<SelectListItem> StatusOptions  { get; set; } = new();

    public CreateModel(CompanyBankAccountDbHelper dbHelper, CountryDbHelper countryDbHelper, TranslationService translation)
    {
        _dbHelper        = dbHelper;
        _countryDbHelper = countryDbHelper;
        _translation     = translation;
    }

    public async Task OnGetAsync()
    {
        AlertMessageType = "";
        var langCode     = string.IsNullOrEmpty(CurrentLangCode) ? "en" : CurrentLangCode;
        CountryOptions   = await SelectListHelper.GetCountryOptions(_countryDbHelper, langCode);
        StatusOptions    = await SelectListHelper.GetStatusOptions(_translation);
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        var langCode   = string.IsNullOrEmpty(CurrentLangCode) ? "en" : CurrentLangCode;
        CountryOptions = await SelectListHelper.GetCountryOptions(_countryDbHelper, langCode);
        StatusOptions  = await SelectListHelper.GetStatusOptions(_translation);

        if (string.IsNullOrWhiteSpace(ddlCountryCode)  ||
            string.IsNullOrWhiteSpace(txtBankName)      ||
            string.IsNullOrWhiteSpace(txtAccountName)   ||
            string.IsNullOrWhiteSpace(txtAccountNumber) ||
            string.IsNullOrWhiteSpace(txtCurrency))
        {
            SetError(await _translation.GetAsync(MessageConstants.RequiredField));
            return Page();
        }

        var account = new CompanyBankAccount
        {
            CountryCode   = ddlCountryCode,
            BankName      = txtBankName.Trim(),
            AccountName   = txtAccountName.Trim(),
            AccountNumber = txtAccountNumber.Trim(),
            Branch        = string.IsNullOrWhiteSpace(txtBranch)  ? null : txtBranch.Trim(),
            Currency      = txtCurrency.Trim().ToUpper(),
            Remarks       = string.IsNullOrWhiteSpace(txtRemarks) ? null : txtRemarks.Trim(),
            Status        = ddlStatus
        };

        await _dbHelper.AddAsync(account, CurrentUsername);
        return RedirectToPage(Routes.AdminCompanyBankAccounts);
    }

    private void SetError(string message)
    {
        AlertMessageType    = MessageType.Error;
        AlertMessageTitle   = MessageTitle.Error;
        AlertMessageContent = message;
    }
}
