namespace MyApp.Models.MRMR;

public class Registrant
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string NricPassport { get; set; } = string.Empty;
    public string ContactNo { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? SsmRegNo { get; set; }
    public string? CompanyAddress { get; set; }
    public string? Website { get; set; }
    public string? Industry { get; set; }
    public string? BusinessNature { get; set; }
    public string? PasswordHash { get; set; }
    public string? Username { get; set; }
    public string Status { get; set; } = "inactive";
    public string? PreferredLang { get; set; }
    public bool IsFirstLogin { get; set; } = true;
    public bool IsActive { get; set; } = false;
    public bool DeclInfoAccurate { get; set; } = false;
    public bool DeclFeeNonrefundable { get; set; } = false;
    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public ICollection<Application> Applications { get; set; } = new List<Application>();
}
