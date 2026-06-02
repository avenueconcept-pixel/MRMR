using Microsoft.AspNetCore.Mvc;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Roles;

public class IndexModel : AdminPageModel
{
  private readonly RoleDbHelper      _roleDbHelper;
  private readonly TranslationService _translation;

  public List<Role> Roles { get; set; } = new();

  public IndexModel(RoleDbHelper roleDbHelper, TranslationService translation)
  {
    _roleDbHelper = roleDbHelper;
    _translation  = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = string.Empty;
    Roles = await _roleDbHelper.GetAllAsync();
  }

  public async Task<IActionResult> OnPostToggleStatusAsync(int id)
  {
    var role = await _roleDbHelper.GetByIdAsync(id);
    if (role == null)
    {
      var msg = await _translation.GetAsync(MessageConstants.NotFound);
      return new JsonResult(new { success = false, message = msg });
    }

    var newStatus = role.Status == StatusConstants.Active
        ? StatusConstants.Inactive
        : StatusConstants.Active;

    await _roleDbHelper.UpdateStatusAsync(id, newStatus, CurrentUsername);
    return new JsonResult(new { success = true });
  }
}
