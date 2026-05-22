using Microsoft.AspNetCore.Authorization;
using MyApp.Constants;

namespace MyApp.Helper;

[Authorize(AuthenticationSchemes = AuthSchemeConstants.Customer)]
public class CustomerPageModel : BasePageModel { }
