namespace MyApp.Models.MRMR;

public class SubmissionSectionJsonb
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public string SectionCode { get; set; } = string.Empty;     // C, D, E, F, G, H, I, J, K, L
    public string? SectionData { get; set; }                    // JSONB stored as string
    public bool IsComplete { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Application Application { get; set; } = null!;
}
