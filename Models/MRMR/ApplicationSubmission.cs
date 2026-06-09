namespace MyApp.Models.MRMR;

public class ApplicationSubmission
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public bool SectionAComplete { get; set; } = false;
    public bool SectionBComplete { get; set; } = false;
    public bool SectionCComplete { get; set; } = false;
    public bool SectionDComplete { get; set; } = false;
    public bool SectionEComplete { get; set; } = false;
    public bool SectionFComplete { get; set; } = false;
    public bool SectionGComplete { get; set; } = false;
    public bool SectionHComplete { get; set; } = false;
    public bool SectionIComplete { get; set; } = false;    // Corporate only — always true for Individual
    public bool SectionJComplete { get; set; } = false;
    public bool SectionKComplete { get; set; } = false;
    public bool SectionLComplete { get; set; } = false;
    public bool IsFinalSubmitted { get; set; } = false;
    public DateTime? LastSavedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Application Application { get; set; } = null!;
}
