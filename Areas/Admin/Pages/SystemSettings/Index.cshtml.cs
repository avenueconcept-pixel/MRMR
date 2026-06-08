using Microsoft.AspNetCore.Mvc;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.SystemSettings;

public class IndexModel : AdminPageModel
{
    private readonly SystemSettingDbHelper _settingDbHelper;
    private readonly SystemSettingService  _settingService;
    private readonly TranslationService    _translation;

    public IndexModel(SystemSettingDbHelper settingDbHelper, SystemSettingService settingService,
        TranslationService translation)
    {
        _settingDbHelper = settingDbHelper;
        _settingService  = settingService;
        _translation     = translation;
    }

    public List<SystemSetting> Items { get; set; } = new();

    [BindProperty] public string EditKey   { get; set; } = string.Empty;
    [BindProperty] public string EditValue { get; set; } = string.Empty;

    public string MsgUpdateSuccess { get; set; } = string.Empty;
    public string MsgSaveError     { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        AlertMessageType = "";
        Items = await _settingDbHelper.GetAllAsync();
        MsgUpdateSuccess = await _translation.GetAsync(MessageConstants.UpdateSuccess);
        MsgSaveError     = await _translation.GetAsync(MessageConstants.SaveError);
    }

    public async Task<IActionResult> OnPostUpdateAsync()
    {
        try
        {
            var setting = new SystemSetting
            {
                SettingKey   = EditKey,
                SettingValue = EditValue.Trim()
            };
            await _settingDbHelper.UpdateAsync(setting, CurrentUsername);
            _settingService.ClearCache();
            var msg = await _translation.GetAsync(MessageConstants.UpdateSuccess);
            return new JsonResult(new { success = true, message = msg });
        }
        catch
        {
            var msg = await _translation.GetAsync(MessageConstants.SaveError);
            return new JsonResult(new { success = false, message = msg });
        }
    }
}
