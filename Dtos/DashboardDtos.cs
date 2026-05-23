namespace MyApp.Dtos;

public class TopProductDto
{
  public string ProductName { get; set; } = string.Empty;
  public string Category { get; set; } = string.Empty;
  public int UnitsSold { get; set; }
  public decimal Revenue { get; set; }
}

public class TopReferralDto
{
  public string CustomerName { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public int ReferralCount { get; set; }
  public decimal Revenue { get; set; }
}

public record CustomerByCountryDto(string CountryName, int TotalCustomers, int NewThisMonth, double PercentageOfTotal);

public record SalesByCountryDto(string CountryName, decimal TotalSales, int OrderCount);
