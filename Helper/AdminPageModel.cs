using Microsoft.AspNetCore.Authorization;
using MyApp.Constants;

namespace MyApp.Helper;

[Authorize(AuthenticationSchemes = AuthSchemeConstants.Admin)]
public class AdminPageModel : BasePageModel { }
