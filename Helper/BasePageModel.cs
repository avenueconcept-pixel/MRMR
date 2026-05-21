using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyApp.Helper;

public class BasePageModel : PageModel
{
  [TempData]
  public string? AlertMessageType { get; set; }

  [TempData]
  public string? AlertMessageTitle { get; set; }

  [TempData]
  public string? AlertMessageContent { get; set; }
}
