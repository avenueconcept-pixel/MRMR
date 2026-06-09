namespace MyApp.Models.MRMR;

public class SubmissionSectionA
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public string? Title { get; set; }
    public string? FullName { get; set; }
    public string? NricPassport { get; set; }
    public string? ContactNo { get; set; }
    public string? Email { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Postcode { get; set; }
    public string? Country { get; set; }
    public string? MembershipNo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Application Application { get; set; } = null!;
}
