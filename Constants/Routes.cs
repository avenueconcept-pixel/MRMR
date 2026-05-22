namespace MyApp.Constants;

public class Routes
{
  // Admin
  public const string AdminLogin = "/Admin/Login";
  public const string AdminForgotPassword = "/Admin/ForgotPassword";
  public const string AdminResetPassword = "/Admin/ResetPassword";
  public const string AdminDashboard = "/Admin/Dashboard";
  public const string AdminProducts = "/Admin/Products";
  public const string AdminCustomers = "/Admin/Customers";
  public const string AdminLogs = "/Logs/Index";

  // Customer
  public const string CustomerLogin = "/Customer/Login";
  public const string CustomerDashboard = "/Customer/Dashboard";
  public const string CustomerOrders = "/Customer/Orders";
  public const string CustomerRegister = "/Customer/Register";

  // Shared
  public const string SetLanguage = "/Account/SetLanguage";
  public const string AccessDenied = "/Account/AccessDenied";
}
