using Microsoft.AspNetCore.Mvc;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Departments;

public class IndexModel : AdminPageModel
{
  private readonly DepartmentDbHelper _deptDbHelper;
  private readonly TranslationService _translation;

  public List<Department> Departments { get; set; } = new();

  public IndexModel(DepartmentDbHelper deptDbHelper, TranslationService translation)
  {
    _deptDbHelper = deptDbHelper;
    _translation  = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";
    Departments = await _deptDbHelper.GetAllAsync();
  }

  public async Task<IActionResult> OnPostToggleStatusAsync([FromForm] int id)
  {
    var dept = await _deptDbHelper.GetByIdAsync(id);
    if (dept == null)
      return new JsonResult(new { success = false });

    var newStatus = dept.Status == StatusConstants.Active
        ? StatusConstants.Inactive
        : StatusConstants.Active;

    await _deptDbHelper.UpdateStatusAsync(id, newStatus, CurrentUsername);
    return new JsonResult(new { success = true, newStatus });
  }
}
