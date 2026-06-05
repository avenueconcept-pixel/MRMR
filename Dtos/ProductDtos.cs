namespace MyApp.Dtos;

public enum ProductAddResult { Created, Restored, DuplicateActive }

public static class ProductTypeConstants
{
  public const string Standard = "standard";
  public const string Package  = "package";
}

public static class ProductNatureConstants
{
  public const string Physical = "physical";
  public const string Digital  = "digital";
}

public static class StockStatusConstants
{
  public const string Available    = "available";
  public const string LimitedStock = "limited_stock";
  public const string SoldOut      = "sold_out";
}

public static class ScheduleTypeConstants
{
  public const string Promo           = "promo";
  public const string PriceAdjustment = "price_adjustment";
}

public static class PriceChangeTypeConstants
{
  public const string PriceAdjustment = "price_adjustment";
  public const string ManualUpdate    = "manual_update";
}

public static class ScheduleStatusConstants
{
  public const string Pending   = "pending";
  public const string Active    = "active";
  public const string Processed = "processed";
  public const string Cancelled = "cancelled";
}
