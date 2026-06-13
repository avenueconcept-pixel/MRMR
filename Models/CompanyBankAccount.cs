using MyApp.Constants;

namespace MyApp.Models;

public class CompanyBankAccount
{
    public int      Id            { get; set; }
    public string   CountryCode   { get; set; } = string.Empty;
    public string   BankName      { get; set; } = string.Empty;
    public string   AccountName   { get; set; } = string.Empty;
    public string   AccountNumber { get; set; } = string.Empty;
    public string?  Branch        { get; set; }
    public string   Currency      { get; set; } = string.Empty;
    public string?  Remarks       { get; set; }
    public string   Status        { get; set; } = StatusConstants.Active;
    public DateTime CreatedAt     { get; set; }
    public string   CreatedBy     { get; set; } = string.Empty;
    public DateTime UpdatedAt     { get; set; }
    public string   UpdatedBy     { get; set; } = string.Empty;

    public Country? Country { get; set; }
}
