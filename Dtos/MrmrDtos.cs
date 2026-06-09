using MyApp.Models.MRMR;

namespace MyApp.Dtos;

public class CategorySummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CategoryType { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class ApplicationDashboardDto
{
    public MyApp.Models.MRMR.Application Application { get; set; } = null!;
    public MyApp.Models.MRMR.Payment? NominationPayment { get; set; }
    public MyApp.Models.MRMR.Payment? AwardPayment { get; set; }
}

public enum RegistrationResult
{
    Success,
    DuplicateEmail,
    DuplicateNric,
    DuplicateApplicationType,
    CategoryNotFound,
    Error
}
