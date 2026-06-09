using MyApp.Models.MRMR;

namespace MyApp.Dtos;

public class CategorySummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CategoryType { get; set; } = string.Empty;
    public decimal Price { get; set; }
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
