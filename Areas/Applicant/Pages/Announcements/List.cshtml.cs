using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;

namespace MyApp.Areas.Applicant.Pages.Announcements;

public class ListModel : ApplicantPageModel
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
    Items = await _dbHelper.GetActiveForAudienceAsync(AnnouncementConstants.AudienceCustomer, langCode);
  }
}
