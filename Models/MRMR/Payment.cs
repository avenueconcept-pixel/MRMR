namespace MyApp.Models.MRMR;

public class Payment
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public string PaymentType { get; set; } = string.Empty;     // NominationFee | AwardFee
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;          // Axaipay | ManualBankTransfer
    public string Status { get; set; } = string.Empty;          // Pending | Verified | Rejected
    public string? SlipFilePath { get; set; }
    public DateTime? SlipUploadedAt { get; set; }
    public string? AxaipayRefNo { get; set; }
    public string? AxaipayPayload { get; set; }                 // JSONB stored as string
    public int? VerifiedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? AdminRemarks { get; set; }
    public string? InvoiceNo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Application Application { get; set; } = null!;
    public ICollection<PaymentAuditLog> AuditLogs { get; set; } = new List<PaymentAuditLog>();
}
