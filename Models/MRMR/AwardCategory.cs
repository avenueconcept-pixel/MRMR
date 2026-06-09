namespace MyApp.Models.MRMR;

public class AwardCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CategoryType { get; set; } = string.Empty;    // Individual | Corporate
    public decimal Price { get; set; }
    public short MaxRecipients { get; set; } = 1;
    public bool IsActive { get; set; } = false;
    public short DisplayOrder { get; set; } = 0;
    public string? Description { get; set; }
    public bool CriteriaLocked { get; set; } = false;
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public ICollection<AwardCriterion> Criteria { get; set; } = new List<AwardCriterion>();
    public ICollection<Application> Applications { get; set; } = new List<Application>();
    public ICollection<JudgeCategoryAssignment> JudgeAssignments { get; set; } = new List<JudgeCategoryAssignment>();
}
