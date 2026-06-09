namespace MyApp.Models.MRMR;

public class Application
{
    public int Id { get; set; }
    public string ApplicationId { get; set; } = string.Empty;   // e.g. MRMR-I-2026-00001
    public int RegistrantId { get; set; }
    public string ApplicationType { get; set; } = string.Empty;  // Individual | Corporate
    public int AwardCategoryId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Username { get; set; }
    public bool IsFinalSubmitted { get; set; } = false;
    public DateTime? SubmittedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Registrant Registrant { get; set; } = null!;
    public AwardCategory AwardCategory { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ApplicationSubmission? Submission { get; set; }
    public SubmissionSectionA? SectionA { get; set; }
    public SubmissionSectionB? SectionB { get; set; }
    public ICollection<SubmissionSectionJsonb> SectionJsonbs { get; set; } = new List<SubmissionSectionJsonb>();
    public ICollection<ApplicationDocument> Documents { get; set; } = new List<ApplicationDocument>();
    public ICollection<JudgeEvaluation> Evaluations { get; set; } = new List<JudgeEvaluation>();
    public ApplicationRanking? Ranking { get; set; }
}
