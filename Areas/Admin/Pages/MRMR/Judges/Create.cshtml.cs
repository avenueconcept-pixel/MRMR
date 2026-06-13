using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models.MRMR;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.MRMR.Judges;

public class CreateModel : AdminPageModel
{
    private readonly AdminMrmrDbHelper _mrmrDb;
    private readonly EmailService      _emailService;

    public CreateModel(AdminMrmrDbHelper mrmrDb, EmailService emailService)
    {
        _mrmrDb       = mrmrDb;
        _emailService = emailService;
    }

    [BindProperty] public string    FullName    { get; set; } = string.Empty;
    [BindProperty] public string    Email       { get; set; } = string.Empty;
    [BindProperty] public string    Username    { get; set; } = string.Empty;
    [BindProperty] public List<int> CategoryIds { get; set; } = [];

    public List<AwardCategory> Categories { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        AlertMessageType = string.Empty;
        Categories = await _mrmrDb.GetActiveCategoriesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (string.IsNullOrWhiteSpace(FullName) ||
            string.IsNullOrWhiteSpace(Email)    ||
            string.IsNullOrWhiteSpace(Username))
        {
            AlertMessageContent = "Full name, email, and username are required.";
            AlertMessageType    = "error";
            Categories          = await _mrmrDb.GetActiveCategoriesAsync();
            return Page();
        }

        var tempPassword = $"Judge@{DateTime.Now:MMddyyyy}!";

        try
        {
            var judge = await _mrmrDb.CreateJudgeAsync(
                FullName, Email, Username, tempPassword, CurrentUserId);

            if (CategoryIds.Any())
                await _mrmrDb.AssignJudgeToCategoriesAsync(judge.Id, CategoryIds, CurrentUserId);

            await _emailService.SendApplicantCredentialsAsync(
                Email, FullName, Username, tempPassword, "en");

            TempData["SuccessMessage"] =
                $"Judge '{FullName}' created and credentials emailed to {Email}.";
            return RedirectToPage("Index");
        }
        catch (InvalidOperationException ex)
        {
            AlertMessageContent = ex.Message;
            AlertMessageType    = "error";
            Categories          = await _mrmrDb.GetActiveCategoriesAsync();
            return Page();
        }
    }
}
