namespace MyApp.Constants;

public class Routes
{
  // Admin
  public const string AdminLogin = "/Login";
  public const string AdminDashboard = "/Dashboard/Index";
  public const string AdminForgotPassword = "/ForgotPassword";
  public const string AdminResetPassword = "/ResetPassword";
  public const string AdminProducts = "/Products";
  public const string AdminCustomers = "/Customers";
  public const string AdminLogs = "/Logs/Index";
  public const string AdminCountry  = "/Countries/Index";
  public const string AdminLanguage    = "/Languages/Index";
  public const string AdminDepartment  = "/Departments/Index";
  public const string AdminPaymentMethod    = "/PaymentMethods/Index";
  public const string AdminProductCategory  = "/ProductCategories/Index";
  public const string AdminAuditLogs       = "/AuditLogs/Index";
  public const string AdminAuditLogsDetail = "/AuditLogs/Detail";
  public const string AdminState           = "/States/Index";
  public const string AdminRegion          = "/Regions/Index";
  public const string AdminLocation        = "/Locations/Index";
  public const string AdminRole            = "/Roles/Index";
  public const string AdminMenu            = "/Menus/Index";

  // Customer
  public const string CustomerLogin = "/Login";
  public const string CustomerDashboard = "/Dashboard";
  public const string CustomerOrders = "/Orders";
  public const string CustomerRegister = "/Register";

  // Shared
  public const string SetLanguage = "/Account/SetLanguage";
  public const string AccessDenied = "/Account/AccessDenied";
}
