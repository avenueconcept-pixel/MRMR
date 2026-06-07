namespace MyApp.Constants;

public static class WalletTypeConstants
{
  public const string Cash     = "cash";
  public const string Purchase = "purchase";
}

public static class WalletDirectionConstants
{
  public const string In  = "in";
  public const string Out = "out";
}

public static class CashTxnTypeConstants
{
  public const string Commission  = "commission";
  public const string Adjustment  = "adjustment";
  public const string Withdrawal  = "withdrawal";
  public const string TransferIn  = "transfer_in";
  public const string TransferOut = "transfer_out";
}

public static class PurchaseTxnTypeConstants
{
  public const string TopUp       = "topup";
  public const string Payment     = "payment";
  public const string Adjustment  = "adjustment";
  public const string TransferIn  = "transfer_in";
  public const string TransferOut = "transfer_out";
}

public static class IncentivePeriodStatusConstants
{
  public const string Open      = "open";
  public const string Closed    = "closed";
  public const string Processed = "processed";
  public const string Partial   = "partial";
}

public static class WalletPayoutStatusConstants
{
  public const string Pending    = "pending";
  public const string Processing = "processing";
  public const string Completed  = "completed";
  public const string Failed     = "failed";
}

public static class IncentiveTypeConstants
{
  public const string RetailProfit       = "retail_profit";
  public const string PersonalSalesBonus = "personal_sales_bonus";
  public const string GroupOverride      = "group_override";
  public const string RankBonus          = "rank_bonus";
  public const string Adjustment         = "adjustment";
}
