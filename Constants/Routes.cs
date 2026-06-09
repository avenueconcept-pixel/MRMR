namespace MyApp.Constants;

public class Routes
{
  // Admin
  public const string AdminLogin = "/Login";
  public const string AdminDashboard = "/Dashboard/Index";
  public const string AdminForgotPassword = "/ForgotPassword";
  public const string AdminResetPassword = "/ResetPassword";
  public const string AdminProducts       = "/Products";
  public const string AdminProductsCreate = "/Products/Create";
  public const string AdminProductsEdit   = "/Products/Edit";
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
  public const string AdminBank            = "/Banks/Index";
  public const string AdminAdminUsers          = "/AdminUsers/Index";
  public const string AdminTranslationManager  = "/TranslationManager/Index";
  public const string AdminForceChangePassword  = "/Account/ForceChangePassword";
  public const string AdminChangePassword       = "/Account/ChangePassword";
  public const string AdminPageAccessHistory    = "/PageAccessHistory/Index";
  public const string AdminUnitOfMeasure        = "/UnitsOfMeasure/Index";
  public const string AdminPriceTier            = "/PriceTiers/Index";
  public const string AdminProductSectionType   = "/ProductSectionTypes/Index";
  public const string AdminEmailTemplate        = "/EmailTemplates/Index";
  public const string AdminAnnouncements        = "/Announcements/Index";
  public const string AdminAnnouncementsList    = "/Announcements/List";
  public const string AdminSystems              = "/Systems/Index";
  public const string AdminMaintenances         = "/Maintenances/Index";
  public const string AdminSystemBranding       = "/SystemSettings/Branding/Index";
  public const string AdminMembers              = "/Members/Index";
  public const string AdminMembersCreate        = "/Members/Create";
  public const string AdminMembersEdit          = "/Members/Edit";
  public const string AdminMembersManage        = "/Members/Manage";
  public const string AdminMembersWallet        = "/Members/Wallet";
  public const string AdminExchangeRates          = "/ExchangeRates/Index";
  public const string AdminExchangeRatesHistory   = "/ExchangeRates/History";
  public const string AdminIncentivePeriods       = "/IncentivePeriods/Index";
  public const string AdminIncentivePeriodsView   = "/IncentivePeriods/View";
  public const string AdminWalletHistory          = "/Wallets/History";
  public const string AdminWalletBalances         = "/Wallets/Balances";
  public const string AdminWalletAdjustments      = "/Wallets/Adjustments";
  public const string AdminSystemSettings         = "/SystemSettings/Index";
  public const string AdminRank                   = "/Ranks/Index";

  // Applicant
  public const string ApplicantLogin                  = "/Login";
  public const string ApplicantDashboard              = "/Dashboard";
  public const string ApplicantRegister               = "/Register";
  public const string ApplicantPaymentSelect          = "/Payment/Select";
  public const string ApplicantPaymentManual          = "/Payment/Manual";
  public const string ApplicantPaymentAxaipayCallback  = "/Payment/AxaipayCallback";
  public const string ApplicantPaymentPending          = "/Payment/Pending";
  public const string ApplicantPaymentConfirmation     = "/Payment/Confirmation";
  public const string ApplicantLogout                  = "/Account/Logout";
  public const string ApplicantForgotPassword          = "/Account/ForgotPassword";
  public const string ApplicantResetPassword           = "/Account/ResetPassword";
  public const string ApplicantChangePassword          = "/Account/ChangePassword";

  // MRMR Admin
  public const string AdminMrmrApplications = "/MRMR/Applications/Index";
  public const string AdminMrmrPayments     = "/MRMR/Payments/Index";
  public const string AdminMrmrDocuments    = "/MRMR/Documents/Index";
  public const string AdminMrmrCategories   = "/MRMR/Categories/Index";
  public const string AdminMrmrJudges       = "/MRMR/Judges/Index";
  public const string AdminMrmrReports      = "/MRMR/Reports/Index";

  // Shared
  public const string SetLanguage = "/Account/SetLanguage";
  public const string AccessDenied = "/Account/AccessDenied";
}
