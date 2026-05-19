using System.Runtime.InteropServices;

namespace MyApp.Helper
{
  public static class AppConstants
  {
    public static class Routes
    {
      public const string Login = "/Auth/Login";
      public const string Logout = "/Auth/Logout";
      public const string ForgotPassword = "/Auth/ForgotPassword";
      //public const string Dashboard = "/DashboardF/CRM";
      public const string Dashboard = "/Index";
      public const string UnderMaintenance = "/UnderMaintenance";

     
    }

    


    public static class MenuAccessType
    {

      public const string View = "View";
      public const string Edit = "Edit";
      public const string Delete = "Delete";     
     

    }



    public static class SystemMessage
    {
      public const string Invalid_Route_Id = "Invalid Route Id";
      public const string Invalid_Data_Action = "Invalid Data Action";

    }

    public static class UploadWebPath
    {
      
      public const string Product = "uploads/product";
      

    }




    public static class SearchDateRange
    {
      public const string Past90Days = "90";
      public const string Past180Days = "180";
      public const string Past365Days = "365";
      public const string CustomDateRange = "custom";
    }
    public static class LanguageOption
    {
      public const string Code_English = "en";
      public const string Code_Chinese = "zh";
      public const string Code_BahasaMalayu = "ms";

      public const string Word_English = "English";
      public const string Word_Chinese = "中文";
      public const string Word_BahasaMalayu = "Bahasa Malayu";

      public const string DefaultCode = "en";
    }

    public static class SessionKeys
    {
      public const string UserId = "UserId";
      public const string Username = "UserName";
      public const string LoginLanguage = "LoginLanguage";
    }




   

  



    public static class YesNo
    {
      public const string Yes = "Y";
      public const string No = "N";

    }

   
    


    public static class MessageTitle
    {
      public const string Error = "Oops...";
      public const string Success = "";
    }

    public static class MessageType
    {
      public const string Success = "success";
      public const string Error = "error";
      public const string Warning = "warning";
    }



    public static class LoginStatus
    {
      public const int Active = 1;
      public const int Inactive = 0;
      public const int Deleted = -1;
    }

    public static class CustomerStatus
    {
      public const int Active = 1;
      public const int Inactive = 0;
    }

    public static class DataStatus
    {
      public const int Active = 1;
      public const int Inactive = 0;
      public const int Deleted = -1;
    }

    public static class AppDefault
    {
      public const string MinDate_DMY = "01/01/1900";
      public const string MinDate_YMD = "1900-01-01";
      public const string DateFormat_YMD = "yyyy-MM-dd";
      public const string DateFormat_YMD_Time = "yyyy-MM-dd HH:mm:ss";
      public const string DateFormat_DMY = "dd/MM/yyyy";

     

      public const string Default_CountryCode = "MY";
      public const string Default_CurrencyCode = "MYR";

      public const string GUID_Empty = "00000000-0000-0000-0000-000000000000";
    }

    public static class EmailTemplate
    {
      public const string ForgotPassword = "ForgotPassword";

    }
  }

}
