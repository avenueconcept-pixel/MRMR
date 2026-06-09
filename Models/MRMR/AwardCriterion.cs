namespace MyApp.Models.MRMR;

public class AwardCriterion
{
    public int Id { get; set; }
    public int AwardCategoryId { get; set; }
    public string CriterionName { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public short DisplayOrder { get; set; } = 0;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public AwardCategory AwardCategory { get; set; } = null!;
    public ICollection<JudgeScore> JudgeScores { get; set; } = new List<JudgeScore>();
}
