using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Announcements;

public class ListModel : AdminPageModel
{
  private readonly AnnouncementDbHelper _dbHelper;

  public List<Announcement> Items { get; set; } = new();

  public ListModel(AnnouncementDbHelper dbHelper)
  {
    _dbHelper = dbHelper;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";
    var langCode = string.IsNullOrEmpty(CurrentLangCode) ? AppConstants.DefaultLanguage : CurrentLangCode;
    Items = await _dbHelper.GetActiveForAudienceAsync(AnnouncementConstants.AudienceAdmin, langCode);
  }
}
