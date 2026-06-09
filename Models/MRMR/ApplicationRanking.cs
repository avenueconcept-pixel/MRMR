namespace MyApp.Models.MRMR;

public class ApplicationRanking
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public int AwardCategoryId { get; set; }
    public decimal FinalScore { get; set; }
    public short RankPosition { get; set; }
    public bool IsRecommended { get; set; } = false;
    public bool IsApprovedWinner { get; set; } = false;
    public string? CommitteeRemarks { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime RankedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Application Application { get; set; } = null!;
    public AwardCategory AwardCategory { get; set; } = null!;
}
