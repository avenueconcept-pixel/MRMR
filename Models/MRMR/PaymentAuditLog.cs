namespace MyApp.Models.MRMR;

public class PaymentAuditLog
{
    public int Id { get; set; }
    public int PaymentId { get; set; }
    public string Action { get; set; } = string.Empty;
    public int? PerformedBy { get; set; }
    public DateTime PerformedAt { get; set; }
    public string? Remarks { get; set; }
    public string? Snapshot { get; set; }               // JSONB stored as string

    // Navigation
    public Payment Payment { get; set; } = null!;
}
