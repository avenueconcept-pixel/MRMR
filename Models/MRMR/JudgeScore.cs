namespace MyApp.Models.MRMR;

public class JudgeScore
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public int JudgeId { get; set; }
    public int AwardCriterionId { get; set; }
    public decimal Score { get; set; }
    public decimal? WeightedScore { get; set; }     // score × (weight / 100) — stored for audit
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Application Application { get; set; } = null!;
    public AwardCriterion AwardCriterion { get; set; } = null!;
}
