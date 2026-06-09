namespace MyApp.Models.MRMR;

public class JudgeEvaluation
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public int JudgeId { get; set; }
    public string Status { get; set; } = string.Empty;         // NotStarted | Draft | Submitted
    public string? OverallComment { get; set; }
    public string? Recommendation { get; set; }                // Recommended | NotRecommended | Neutral
    public DateTime? SubmittedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Application Application { get; set; } = null!;
    public ICollection<JudgeScore> Scores { get; set; } = new List<JudgeScore>();
}
