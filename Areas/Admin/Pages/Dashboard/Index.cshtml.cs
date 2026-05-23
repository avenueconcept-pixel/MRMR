using MyApp.Dtos;
using MyApp.Helper;
using System.Text.Json;

namespace MyApp.Areas.Admin.Pages.Dashboard
{
  public class IndexModel : AdminPageModel
  {
    // Stat cards
    public string AdminName { get; set; } = string.Empty;
    public int TotalCustomers { get; set; }
    public int NewCustomersThisMonth { get; set; }
    public double CustomerGrowth { get; set; }
    public int TotalOrders { get; set; }
    public int NewOrdersThisMonth { get; set; }
    public double OrderGrowth { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public double RevenueGrowth { get; set; }
    public decimal TotalRevenue { get; set; }
    public double TotalRevenueGrowth { get; set; }

    // Chart: Sales Analysis + Customer Growth (shared month labels)
    public List<string> MonthLabels { get; set; } = new();
    public List<int> OrdersData { get; set; } = new();
    public List<int> NewCustomersData { get; set; } = new();

    // Chart: Sales Comparison (this year vs last year, 12 months)
    public List<string> ComparisonLabels { get; set; } = new();
    public List<decimal> ThisYearRevenue { get; set; } = new();
    public List<decimal> LastYearRevenue { get; set; } = new();

    // Tables
    public List<TopProductDto> TopProducts { get; set; } = new();
    public List<TopReferralDto> TopReferrals { get; set; } = new();

    // Customer by Country
    public List<CustomerByCountryDto> CustomersByCountry { get; set; } = new();
    public string ChartCustomerLabels { get; set; } = string.Empty;
    public string ChartCustomerCounts { get; set; } = string.Empty;

    // Sales by Country
    public List<SalesByCountryDto> SalesByCountry { get; set; } = new();
    public string ChartCountryLabels { get; set; } = string.Empty;
    public string ChartCountrySales { get; set; } = string.Empty;



    public void OnGet()
    {
      // Replace all values below with real DB queries via AdminDbHelper
      AdminName = CurrentFullName;

      TotalCustomers = 1_284;
      NewCustomersThisMonth = 47;
      CustomerGrowth = 12.5;

      TotalOrders = 3_560;
      NewOrdersThisMonth = 128;
      OrderGrowth = 5.8;

      RevenueThisMonth = 24_750.00m;
      RevenueGrowth = 8.3;

      TotalRevenue = 198_400.00m;
      TotalRevenueGrowth = 15.2;

      MonthLabels = new() { "Dec", "Jan", "Feb", "Mar", "Apr", "May" };
      OrdersData = new() { 310, 420, 380, 510, 490, 128 };
      NewCustomersData = new() { 38, 52, 41, 67, 55, 47 };

      ComparisonLabels = new() { "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "Jan", "Feb", "Mar", "Apr", "May" };
      ThisYearRevenue = new() { 19_200, 20_100, 21_500, 22_800, 23_500, 24_100, 18_200, 21_400, 19_800, 23_100, 22_300, 24_750 };
      LastYearRevenue = new() { 16_500, 17_200, 18_100, 19_800, 20_500, 21_300, 15_800, 18_500, 17_200, 20_100, 19_800, 21_500 };

      TopProducts = new()
      {
        new() { ProductName = "Product Alpha",   Category = "Electronics", UnitsSold = 342, Revenue = 51_300m },
        new() { ProductName = "Product Beta",    Category = "Clothing",    UnitsSold = 287, Revenue = 28_700m },
        new() { ProductName = "Product Gamma",   Category = "Electronics", UnitsSold = 215, Revenue = 43_000m },
        new() { ProductName = "Product Delta",   Category = "Home",        UnitsSold = 198, Revenue = 19_800m },
        new() { ProductName = "Product Epsilon", Category = "Sports",      UnitsSold = 176, Revenue = 17_600m },
      };

      TopReferrals = new()
      {
        new() { CustomerName = "Alice Tan",  Email = "alice@example.com", ReferralCount = 12, Revenue = 8_400m },
        new() { CustomerName = "Bob Lim",    Email = "bob@example.com",   ReferralCount = 9,  Revenue = 6_300m },
        new() { CustomerName = "Carol Wong", Email = "carol@example.com", ReferralCount = 7,  Revenue = 4_900m },
        new() { CustomerName = "David Ng",   Email = "david@example.com", ReferralCount = 6,  Revenue = 4_200m },
        new() { CustomerName = "Eve Chong",  Email = "eve@example.com",   ReferralCount = 5,  Revenue = 3_500m },
      };

      var rawCountries = new (string Country, int Total, int NewThisMonth)[]
      {
        ("Malaysia",     420, 18),
        ("Singapore",    310, 12),
        ("Indonesia",    285,  9),
        ("Thailand",     198,  7),
        ("Philippines",  156,  5),
        ("Vietnam",      115,  4),
      };

      int grandTotal = rawCountries.Sum(c => c.Total);
      CustomersByCountry = rawCountries
          .Select(c => new CustomerByCountryDto(
              c.Country,
              c.Total,
              c.NewThisMonth,
              grandTotal > 0 ? Math.Round((double)c.Total / grandTotal * 100, 1) : 0))
          .OrderByDescending(c => c.TotalCustomers)
          .ToList();

      ChartCustomerLabels = JsonSerializer.Serialize(CustomersByCountry.Select(c => c.CountryName));
      ChartCustomerCounts = JsonSerializer.Serialize(CustomersByCountry.Select(c => c.TotalCustomers));

      SalesByCountry = new List<SalesByCountryDto>
      {
        new("Malaysia",     68_400.00m, 312),
        new("Singapore",    52_750.00m, 198),
        new("Indonesia",    41_200.00m, 175),
        new("Thailand",     29_850.00m, 134),
        new("Philippines",  21_300.00m,  98),
        new("Vietnam",      15_900.00m,  73),
      }
      .OrderByDescending(s => s.TotalSales)
      .ToList();

      ChartCountryLabels = JsonSerializer.Serialize(SalesByCountry.Select(s => s.CountryName));
      ChartCountrySales  = JsonSerializer.Serialize(SalesByCountry.Select(s => s.TotalSales));
    }
  }
}
