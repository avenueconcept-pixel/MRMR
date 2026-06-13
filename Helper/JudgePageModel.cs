using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;

namespace MyApp.Helper;

public abstract class JudgePageModel : AdminPageModel
{
    private readonly AppDbContext _db;

    protected JudgePageModel(AppDbContext db)
    {
        _db = db;
    }

    public override async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context,
        PageHandlerExecutionDelegate next)
    {
        var judgeRole = await _db.Roles.FirstOrDefaultAsync(r => r.RoleCode == "JUDGE");
        if (judgeRole == null || CurrentRoleId != judgeRole.Id)
        {
            context.Result = new RedirectToPageResult(Routes.AdminDashboard);
            return;
        }

        await next();
    }
}
