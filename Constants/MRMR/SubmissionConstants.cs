namespace MyApp.Constants.MRMR;

public static class SubmissionConstants
{
    // Section keys — used as jsonb keys in submission_sections_jsonb
    public const string SectionA = "A";   // Nominee Details
    public const string SectionB = "B";   // Company Details
    public const string SectionC = "C";   // Category Confirmation (read-only)
    public const string SectionD = "D";   // Positioning Statement
    public const string SectionE = "E";   // Achievements & Contributions
    public const string SectionF = "F";   // Notable Impact
    public const string SectionG = "G";   // Innovation & Future Plans
    public const string SectionH = "H";   // ESG & Social Contribution
    public const string SectionI = "I";   // Financial Performance (Corporate only)
    public const string SectionJ = "J";   // Introducer Details
    public const string SectionK = "K";   // Document Upload
    public const string SectionL = "L";   // Declaration
    // Section M is generated on-the-fly — no row in submission_sections_jsonb

    public const int MaxFileSizeMb = 10;
    public const string AllowedFileExtensions = ".pdf,.jpg,.jpeg,.png";
}
