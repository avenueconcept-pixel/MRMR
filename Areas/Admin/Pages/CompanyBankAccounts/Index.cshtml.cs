using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.CompanyBankAccounts;

public class IndexModel : AdminPageModel
{
    private readonly CompanyBankAccountDbHelper _dbHelper;
    private readonly CountryDbHelper            _countryDbHelper;
    private readonly TranslationService         _translation;

    public List<CompanyBankAccount> Items           { get; set; } = new();
    public List<SelectListItem>     CountryOptions  { get; set; } = new();
    public string                   SelectedCountry { get; set; } = string.Empty;

    public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
    public string MsgDeleteConfirmText  { get; set; } = string.Empty;
    public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
    public string MsgCancelBtn          { get; set; } = string.Empty;
    public string MsgDeleteSuccess      { get; set; } = string.Empty;
    public string MsgDeleteError        { get; set; } = string.Empty;
    public string LabelDelete           { get; set; } = string.Empty;
    public string MsgToggleSuccess      { get; set; } = string.Empty;
    public string MsgToggleError        { get; set; } = string.Empty;

    public IndexModel(CompanyBankAccountDbHelper dbHelper, CountryDbHelper countryDbHelper, TranslationService translation)
    {
        _dbHelper        = dbHelper;
        _countryDbHelper = countryDbHelper;
        _translation     = translation;
    }

    public async Task OnGetAsync(string countryCode = "")
    {
        AlertMessageType = "";
        var langCode = string.IsNullOrEmpty(CurrentLangCode) ? "en" : CurrentLangCode;

        var allCountry = await _translation.GetAsync("CompanyBankAccount.AllCountries");
        CountryOptions = await SelectListHelper.GetCountryOptions(_countryDbHelper, langCode);
        CountryOptions.Insert(0, new SelectListItem { Value = string.Empty, Text = allCountry });

        var all = await _dbHelper.GetAllAsync(langCode);
        Items = string.IsNullOrEmpty(countryCode)
            ? all
            : all.Where(a => a.CountryCode == countryCode).ToList();

        SelectedCountry = countryCode;

        MsgDeleteConfirmTitle = await _translation.GetAsync("Confirm.DeleteTitle");
        MsgDeleteConfirmText  = await _translation.GetAsync("Confirm.DeleteText");
        MsgDeleteConfirmBtn   = await _translation.GetAsync("Btn.YesDelete");
        MsgCancelBtn          = await _translation.GetAsync("Btn.Cancel");
        MsgDeleteSuccess      = await _translation.GetAsync(MessageConstants.DeleteSuccess);
        MsgDeleteError        = await _translation.GetAsync(MessageConstants.DeleteError);
        LabelDelete           = await _translation.GetAsync("Btn.Delete");
        MsgToggleSuccess      = await _translation.GetAsync(MessageConstants.SaveSuccess);
        MsgToggleError        = await _translation.GetAsync(MessageConstants.SaveError);
    }

    public async Task<IActionResult> OnPostToggleStatusAsync([FromForm] int id)
    {
        var account = await _dbHelper.GetByIdAsync(id);
        if (account == null)
            return new JsonResult(new { success = false });

        var newStatus = account.Status == StatusConstants.Active
            ? StatusConstants.Inactive
            : StatusConstants.Active;

        await _dbHelper.UpdateStatusAsync(id, newStatus, CurrentUsername);
        return new JsonResult(new { success = true, newStatus });
    }

    public async Task<IActionResult> OnPostDeleteAsync([FromForm] int id)
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
}
