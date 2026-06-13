using MyApp.Models.MRMR;

namespace MyApp.Dtos;

public class CategorySummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CategoryType { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class ApplicationDashboardDto
{
    public MyApp.Models.MRMR.Application             Application       { get; set; } = null!;
    public MyApp.Models.MRMR.Payment?                NominationPayment { get; set; }
    public MyApp.Models.MRMR.Payment?                AwardPayment      { get; set; }
    public List<MyApp.Models.MRMR.ApplicationDocument> Documents       { get; set; } = [];
}

public enum RegistrationResult
{
    Success,
    DuplicateEmail,
    DuplicateNric,
    DuplicateApplicationType,
    CategoryNotFound,
    Error
}

public class CriterionScoreInputDto
{
    public int     CriterionId { get; set; }
    public decimal Score       { get; set; }
    public string? Comment     { get; set; }
}

public class ApplicationScoreSummaryDto
{
    public MyApp.Models.MRMR.Application           Application   { get; set; } = null!;
    public decimal                                  TotalWeighted { get; set; }
    public int                                      JudgeCount    { get; set; }
    public List<JudgeScoreRowDto>                   JudgeRows     { get; set; } = [];
    public MyApp.Models.MRMR.ApplicationRanking?    Ranking       { get; set; }
}

public class JudgeScoreRowDto
{
    public int     JudgeId        { get; set; }
    public string  JudgeName      { get; set; } = string.Empty;
    public decimal TotalWeighted  { get; set; }
    public string? Recommendation { get; set; }
}

public class FinalizeDecisionDto
{
    public int     ApplicationId    { get; set; }
    public bool    IsApprovedWinner { get; set; }
    public string? CommitteeRemarks { get; set; }
}
