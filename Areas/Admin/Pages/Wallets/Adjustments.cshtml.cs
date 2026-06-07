using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Wallets;

public class AdjustmentsModel : AdminPageModel
{
  private readonly WalletDbHelper    _walletDbHelper;
  private readonly TranslationService _translation;

  [BindProperty(SupportsGet = true)] public string? FilterMemberUsername { get; set; }
  [BindProperty(SupportsGet = true)] public string? FilterWalletType     { get; set; }
  [BindProperty(SupportsGet = true)] public string? FilterCreatedBy      { get; set; }
  [BindProperty(SupportsGet = true)] public string? FilterStartDate      { get; set; }
  [BindProperty(SupportsGet = true)] public string? FilterEndDate        { get; set; }
  [BindProperty(SupportsGet = true)] public int     CurrentPage          { get; set; } = 1;

  public List<AdminWalletTxnRowDto> Items        { get; set; } = new();
  public int                        TotalCount   { get; set; }
  public int                        PageSize     => 50;
  public int                        TotalPages   => (int)Math.Ceiling((double)TotalCount / PageSize);
  public List<SelectListItem>       ddlWalletType { get; set; } = new();

  public AdjustmentsModel(WalletDbHelper walletDbHelper, TranslationService translation)
  {
    _walletDbHelper = walletDbHelper;
    _translation    = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";

    DateTime? filterStartUtc = null;
    if (!string.IsNullOrEmpty(FilterStartDate) &&
        DateTime.TryParseExact(FilterStartDate, AppConstants.DateInputFormat,
            null, System.Globalization.DateTimeStyles.None, out var startLocal))
      filterStartUtc = startLocal.ToUtcFromUserTimezone(UserTimezone);

    DateTime? filterEndUtc = null;
    if (!string.IsNullOrEmpty(FilterEndDate) &&
        DateTime.TryParseExact(FilterEndDate, AppConstants.DateInputFormat,
            null, System.Globalization.DateTimeStyles.None, out var endLocal))
      filterEndUtc = endLocal.AddDays(1).AddSeconds(-1).ToUtcFromUserTimezone(UserTimezone);

    (Items, TotalCount) = await _walletDbHelper.GetAllAdjustmentsAsync(
        FilterMemberUsername, FilterWalletType, FilterCreatedBy,
        filterStartUtc, filterEndUtc, CurrentPage, PageSize);

    ddlWalletType = new List<SelectListItem>
    {
      new() { Value = string.Empty,                 Text = await _translation.GetAsync("Filter.All") },
      new() { Value = WalletTypeConstants.Cash,     Text = await _translation.GetAsync("Wallets.Type.Cash") },
      new() { Value = WalletTypeConstants.Purchase, Text = await _translation.GetAsync("Wallets.Type.Purchase") }
    };
  }
}
