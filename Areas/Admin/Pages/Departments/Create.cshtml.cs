using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Departments;

public class CreateModel : AdminPageModel
{
  private readonly DepartmentDbHelper _deptDbHelper;
  private readonly TranslationService _translation;

  [BindProperty] public string txtDeptName { get; set; } = string.Empty;
  [BindProperty] public string ddlStatus   { get; set; } = StatusConstants.Active;

  public List<SelectListItem> StatusOptions { get; set; } = new();

  public CreateModel(DepartmentDbHelper deptDbHelper, TranslationService translation)
  {
    _deptDbHelper = deptDbHelper;
    _translation  = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";
    StatusOptions    = await SelectListHelper.GetStatusOptions(_translation);
  }

  public async Task<IActionResult> OnPostCreateAsync()
  {
    StatusOptions = await SelectListHelper.GetStatusOptions(_translation);

    if (string.IsNullOrWhiteSpace(txtDeptName))
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.RequiredField);
      return Page();
    }

    var dept = new Department
    {
      DeptName = txtDeptName.Trim(),
      Status   = ddlStatus
    };

    var result = await _deptDbHelper.AddAsync(dept, CurrentUsername);

    if (result == DeptAddResult.DuplicateActive)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.DuplicateError);
      return Page();
    }

    AlertMessageType    = MessageType.Success;
    AlertMessageTitle   = MessageTitle.Success;
    AlertMessageContent = result == DeptAddResult.Restored
        ? await _translation.GetAsync(MessageConstants.RestoreSuccess)
        : await _translation.GetAsync(MessageConstants.SaveSuccess);

    return RedirectToPage(Routes.AdminDepartment);
  }
}
