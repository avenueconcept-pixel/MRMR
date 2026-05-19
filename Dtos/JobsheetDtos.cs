using AspnetCoreStarter.Models;
using Microsoft.EntityFrameworkCore;

namespace AspnetCoreStarter.Dtos
{


  public class Dto_JobSheetList
  {
    public Guid JobId { get; set; }

    public string? JobNo { get; set; }
    public DateTime EntryDate { get; set; }
    public DateTime DueDate { get; set; }

    [Precision(10, 2)] // Optional: controls SQL precision
    public decimal Deposit { get; set; }

    [Precision(10, 2)] // Optional: controls SQL precision
    public decimal TotalAmount { get; set; }

    public string? InvoiceNo { get; set; }
    public string? JobType { get; set; }
    public string? JobStatus { get; set; }
    public int Priority { get; set; }
    public Guid DesignerId { get; set; }
    public DateTime DesignerDueDate { get; set; }
    public DateTime DesignerCompletedDate { get; set; }
    public string? RequiredDesignerFlag { get; set; }
    public Guid ProductionId { get; set; }
    public DateTime ProductionDueDate { get; set; }
    public DateTime ProductionCompletedDate { get; set; }
    public DateTime InstallationDate { get; set; }
    public DateTime InstallationCompletedDate { get; set; }
    public string? InstallLocation { get; set; }
    public string? InstallAddress { get; set; }
    public Guid AdminAccountId { get; set; }
    public string? FinishingMethod { get; set; }
    public DateTime CustomerPickupDate { get; set; }
    public DateTime JobCompleteDate { get; set; }
    public string? JobDescription { get; set; }
    public string? SOSRemarks { get; set; }
    public Guid CustomerId { get; set; }
    public string? ContactPICName { get; set; }
    public string? ContactPICMobile { get; set; }
    public string? ContactPICEmail { get; set; }
    public string? RequiredAttention { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? LastUpdatedBy { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerNameShort { get; set; }
    public string? JobStatusText { get; set; }
    public string? FinishingMethodText { get; set; }
    public string? JobTypeText { get; set; }
    public string? PriorityText { get; set; }
    public string? ProcessDay { get; set; }

    [Precision(10, 2)] // Optional: controls SQL precision
    public decimal BalanceAmount { get; set; }
    public decimal? BonusMarginAmount { get; set; }

    public string? BranchName { get; set; }
    public string? AdminUsername { get; set; }
    public string? DesignerUsername { get; set; }
  }

  public class Dto_JobInstallerDetailView
  {
    public Guid RecordId { get; set; }
    public Guid JobId { get; set; }
    public Guid InstallerId { get; set; }
    public string InstallerUsername { get; set; }
    public string InstallerFullName { get; set; }
  }

  public class Dto_JobSummaryByUsersReport
  {
    public Guid UserId { get; set; }

    public string Username { get; set; }
    public string FullName { get; set; }
    public int CurrentJobOnHand { get; set; }
    public int TotalJobPast30Day { get; set; }
    public int TotalJobPast90Day { get; set; }
    public int TotalJobPast180Day { get; set; }
    public int TotalJobPast365Day { get; set; }
  }

  public class Dto_InstallerList
  {
    public Guid InstallerId { get; set; }
    public string InstallerUserName { get; set; }
    public string InstallerName { get; set; }
    public bool IsSelected { get; set; }
  }

  public class Dto_DataOptionType
  {
    public Guid TypeId { get; set; } = Guid.NewGuid();
    public string? TypeName { get; set; } = "";
    public string? JobType { get; set; } = "";
    public string? JobTypeName { get; set; } = "";
    public int? SeqNo { get; set; }

    public int TotalCount { get; set; }
  }


  public class Dto_JobTypeList
  {
    public string? JobType { get; set; } = "";

    public string? JobTypeName { get; set; } = "";

    public int SeqNo { get; set; }
    public int TotalCount { get; set; }

  }

  public class Dto_OptionTypeList
  {

    public string? JobType { get; set; } = "";

    public string? JobTypeName { get; set; } = "";

    public int? SeqNo { get; set; }
    public int TotalCount { get; set; }

  }

 

 

  public class Dto_DataOptionList
  {
    public Guid OptionId { get; set; } = Guid.NewGuid();
    public string? OptionName { get; set; } = "";
    //public string? JobType { get; set; } = "";
    public Guid? OptionCategoryId { get; set; } = Guid.NewGuid();
    public string? OptionCategoryNameList { get; set; } = "";
    public int? SeqNo { get; set; }

    public int TotalCount { get; set; }


  }

  //public class Dto_DataOptionCategory
  //{
  //  public Guid OptionCategoryId { get; set; } = Guid.NewGuid();

  //  public string OptionCategoryName { get; set; }

  //  public Guid? TypeId { get; set; } = Guid.NewGuid();
  //  public string? TypeName { get; set; } = "";
  //  public string? JobType { get; set; } = "";
  //  public string? JobTypeName { get; set; } = "";

  //  public int SeqNo { get; set; }
  //  public int TotalCount { get; set; }

  //}

  public class Dto_OptionCategory
  {
    public Guid OptionCategoryId { get; set; }
    public string OptionCategoryName { get; set; }
    public Guid?  TypeId { get; set; }
    public string TypeName { get; set; }
    public string JobType { get; set; }
    public string? JobTypeName { get; set; } = "";
    public string DisplayName { get; set; }
    public int TotalCount { get; set; }
    public bool IsSelected { get; set; }

    public int? SeqNo { get; set; }

  }






}
