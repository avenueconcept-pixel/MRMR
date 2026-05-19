using System.ComponentModel.DataAnnotations;

namespace AspnetCoreStarter.Dtos
{
  public class Dto_CustomerList
  {
    [Key]
    public Guid CustomerId { get; set; } = Guid.NewGuid();


    public string? CustomerName { get; set; } = "";


    public string? ContactPersonName { get; set; } = "";

    [EmailAddress]
    public string? Email { get; set; } = "";

    [Phone]
    public string? MobileNumber { get; set; } = "";

    public string? Location { get; set; } = "";

    public int TotalJobsheetCount { get; set; } = 0;

    public decimal TotalJobsheetAmount { get; set; } = 0;
  }

  public class Dto_CustomerStats
  {
    public Guid CustomerId { get; set; } = Guid.NewGuid();
    public int TotalJobsheetCount { get; set; }
    public decimal TotalJobsheetAmount { get; set; }
  }



}
