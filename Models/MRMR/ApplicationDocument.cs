namespace MyApp.Models.MRMR;

public class ApplicationDocument
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string OriginalFilename { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int? FileSizeBytes { get; set; }
    public string? MimeType { get; set; }
    public bool IsRequired { get; set; } = false;
    public string VerificationStatus { get; set; } = string.Empty;  // Pending | Verified | Rejected
    public string? AdminRemarks { get; set; }
    public int? VerifiedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Application Application { get; set; } = null!;
}
