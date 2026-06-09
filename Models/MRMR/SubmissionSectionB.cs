namespace MyApp.Models.MRMR;

public class SubmissionSectionB
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public string? CompanyName { get; set; }
    public string? SsmRegNo { get; set; }
    public DateOnly? IncorporationDate { get; set; }
    public string? ContactNo { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Postcode { get; set; }
    public string? Country { get; set; }
    public string? Website { get; set; }
    public string? Industry { get; set; }
    public string? BusinessNature { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Application Application { get; set; } = null!;
}
