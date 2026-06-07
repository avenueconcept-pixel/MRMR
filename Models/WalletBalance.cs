namespace MyApp.Models;

public class WalletBalance
{
  public int      Id         { get; set; }
  public int      MemberId   { get; set; }
  public string   WalletType { get; set; } = string.Empty;
  public decimal  Balance    { get; set; }
  public DateTime UpdatedAt  { get; set; }

  public Member? Member { get; set; }
}
