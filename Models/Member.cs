namespace MyApp.Models;

public class Member
{
  public int      Id               { get; set; }
  public string   Username         { get; set; } = string.Empty;
  public string   FullName         { get; set; } = string.Empty;
  public string   IdType           { get; set; } = string.Empty;
  public string   IdNo             { get; set; } = string.Empty;
  public string   Email            { get; set; } = string.Empty;
  public string   PhoneCountryCode { get; set; } = string.Empty;
  public string   PhoneNumber      { get; set; } = string.Empty;
  public string?  ProfileImage     { get; set; }
  public string   AddressLine1     { get; set; } = string.Empty;
  public string?  AddressLine2     { get; set; }
  public string   City             { get; set; } = string.Empty;
  public string   State            { get; set; } = string.Empty;
  public string   Postcode         { get; set; } = string.Empty;
  public string   CountryCode      { get; set; } = string.Empty;
  public string?  BankName         { get; set; }
  public string?  BankAccountName  { get; set; }
  public string?  BankAccountNo    { get; set; }
  public int?     SponsorId        { get; set; }
  public int?     BinaryParentId   { get; set; }
  public string?  BinaryPosition   { get; set; }
  public bool     IsActivated      { get; set; }
  public DateTime? ActivatedAt     { get; set; }
  public DateTime JoinedAt         { get; set; }
  public string?  CurrentRankCode  { get; set; }
  public string?  HighestRankCode  { get; set; }
  public string   PasswordHash     { get; set; } = string.Empty;
  public string   Status           { get; set; } = string.Empty;
  public string   CreatedBy        { get; set; } = string.Empty;
  public DateTime CreatedAt        { get; set; }
  public string   UpdatedBy        { get; set; } = string.Empty;
  public DateTime UpdatedAt        { get; set; }

  public Country? Country      { get; set; }
  public Member?  Sponsor      { get; set; }
  public Member?  BinaryParent { get; set; }
}
