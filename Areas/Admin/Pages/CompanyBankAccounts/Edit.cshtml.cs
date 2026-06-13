using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.CompanyBankAccounts;

public class EditModel : AdminPageModel
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

    public int                  AccountId      { get; set; }
    public string               CreatedBy      { get; set; } = string.Empty;
    public DateTime             CreatedAt      { get; set; }
    public string               UpdatedBy      { get; set; } = string.Empty;
    public DateTime             UpdatedAt      { get; set; }
    public List<SelectListItem> CountryOptions { get; set; } = new();
    public List<SelectListItem> StatusOptions  { get; set; } = new();

    public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
    public string MsgDeleteConfirmText  { get; set; } = string.Empty;
    public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
    public string MsgCancelBtn          { get; set; } = string.Empty;
    public string MsgDeleteSuccess      { get; set; } = string.Empty;
    public string MsgDeleteError        { get; set; } = string.Empty;
    public string LabelDelete           { get; set; } = string.Empty;

    public EditModel(CompanyBankAccountDbHelper dbHelper, CountryDbHelper countryDbHelper, TranslationService translation)
    {
        _dbHelper        = dbHelper;
        _countryDbHelper = countryDbHelper;
        _translation     = translation;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        AlertMessageType = "";
        var account = await _dbHelper.GetByIdAsync(id);
        if (account == null || account.Status == StatusConstants.Deleted)
        {
            AlertMessageType    = MessageType.Error;
            AlertMessageTitle   = MessageTitle.Error;
            AlertMessageContent = await _translation.GetAsync(MessageConstants.NotFound);
            return RedirectToPage(Routes.AdminCompanyBankAccounts);
        }

        var langCode     = string.IsNullOrEmpty(CurrentLangCode) ? "en" : CurrentLangCode;
        AccountId        = account.Id;
        ddlCountryCode   = account.CountryCode;
        txtBankName      = account.BankName;
        txtAccountName   = account.AccountName;
        txtAccountNumber = account.AccountNumber;
        txtBranch        = account.Branch   ?? string.Empty;
        txtCurrency      = account.Currency;
        txtRemarks       = account.Remarks  ?? string.Empty;
        ddlStatus        = account.Status;
        CreatedBy        = account.CreatedBy;
        CreatedAt        = account.CreatedAt;
        UpdatedBy        = account.UpdatedBy;
        UpdatedAt        = account.UpdatedAt;

        CountryOptions = await SelectListHelper.GetCountryOptions(_countryDbHelper, langCode);
        StatusOptions  = await SelectListHelper.GetStatusOptions(_translation);

        MsgDeleteConfirmTitle = $"{await _translation.GetAsync("Confirm.DeleteTitle")} {account.BankName}";
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
        var langCode   = string.IsNullOrEmpty(CurrentLangCode) ? "en" : CurrentLangCode;
        AccountId      = id;
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
            Id            = id,
            CountryCode   = ddlCountryCode,
            BankName      = txtBankName.Trim(),
            AccountName   = txtAccountName.Trim(),
            AccountNumber = txtAccountNumber.Trim(),
            Branch        = string.IsNullOrWhiteSpace(txtBranch)  ? null : txtBranch.Trim(),
            Currency      = txtCurrency.Trim().ToUpper(),
            Remarks       = string.IsNullOrWhiteSpace(txtRemarks) ? null : txtRemarks.Trim(),
            Status        = ddlStatus
        };

        await _dbHelper.UpdateAsync(account, CurrentUsername);
        return RedirectToPage(Routes.AdminCompanyBankAccounts);
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            await _dbHelper.DeleteAsync(id, CurrentUsername);
            var msg = await _translation.GetAsync(MessageConstants.DeleteSuccess);
            return new JsonResult(new { success = true, message = msg });
        }
        catch
        {
            var msg = await _translation.GetAsync(MessageConstants.DeleteError);
            return new JsonResult(new { success = false, message = msg });
        }
    }

    private void SetError(string message)
    {
        AlertMessageType    = MessageType.Error;
        AlertMessageTitle   = MessageTitle.Error;
        AlertMessageContent = message;
    }
}
