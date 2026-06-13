using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;

namespace MyApp.Areas.Customer.Pages.CompanyBankAccounts;

public class IndexModel : CustomerPageModel
{
    private readonly CompanyBankAccountDbHelper _dbHelper;

    public List<CompanyBankAccount> Items { get; set; } = new();

    public IndexModel(CompanyBankAccountDbHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public async Task OnGetAsync()
    {
        AlertMessageType = "";
        var langCode = string.IsNullOrEmpty(CurrentLangCode) ? "en" : CurrentLangCode;
        Items = await _dbHelper.GetAllActiveAsync(langCode);
    }
}
