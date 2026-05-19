using MyApp.Helper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using MyApp.Data;

namespace MyApp.Areas.Admin.Pages
{
  public class LogoutModel : PageModel
  {
    private readonly AppDbContext _context;
    private readonly IDbLocalizer _localizer;
    private readonly ILogger<LogoutModel> _logger;

    [TempData]
    public string AlertMessageType { get; set; }

    [TempData]
    public string AlertMessageTitle { get; set; }

    [TempData]
    public string AlertMessageContent { get; set; }

    [TempData]
    public string DefaultUsername { get; set; }

    public LogoutModel( ILogger<LogoutModel> logger, AppDbContext context, IDbLocalizer localizer)
    {
      _localizer = localizer;

      _context = context;

        _logger = logger;
      }

    public async Task<IActionResult> OnGet()
    {
      //_logger.LogInformation("User logged out.");
      DefaultUsername = @User.FindFirst("UserName")?.Value;
      //AlertMessageType = AppConstants.MessageType.Success;
      //AlertMessageTitle = AppConstants.MessageTitle.Success;
      //AlertMessageContent = _localizer.Get("LoggedOut");

      await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

      return RedirectToPage(AppConstants.Routes.Login); // Or wherever you want to redirect
    }
  }
}
