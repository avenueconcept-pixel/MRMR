namespace MyApp.Models.MRMR;

public class JudgeCategoryAssignment
{
    public int Id { get; set; }
    public int JudgeId { get; set; }
    public int AwardCategoryId { get; set; }
    public int AssignedBy { get; set; }
    public DateTime AssignedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public AwardCategory AwardCategory { get; set; } = null!;
}
