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
  public const string AdminCountry = "/Countries/Index";

  // Customer
  public const string CustomerLogin = "/Login";
  public const string CustomerDashboard = "/Dashboard";
  public const string CustomerOrders = "/Orders";
  public const string CustomerRegister = "/Register";

  // Shared
  public const string SetLanguage = "/Account/SetLanguage";
  public const string AccessDenied = "/Account/AccessDenied";
}
