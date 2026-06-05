namespace MyApp.Dtos;

public enum ProductAddResult { Created, Restored, DuplicateActive }

public class ProductTranslationInputDto
{
  public string LanguageCode    { get; set; } = string.Empty;
  public string LanguageName    { get; set; } = string.Empty;
  public string ProductName     { get; set; } = string.Empty;
  public string ShortDescription{ get; set; } = string.Empty;
}

public class CountrySelectionDto
{
  public string CountryCode { get; set; } = string.Empty;
  public string CountryName { get; set; } = string.Empty;
  public bool   IsSelected  { get; set; }
  public string StockStatus { get; set; } = StockStatusConstants.Available;
}

public class ImageSortItem
{
  public int Id        { get; set; }
  public int SortOrder { get; set; }
}

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

public class PriceScheduleRowDto
{
  public int       Id           { get; set; }
  public string    CountryCode  { get; set; } = string.Empty;
  public string    CountryName  { get; set; } = string.Empty;
  public string    TierCode     { get; set; } = string.Empty;
  public string    TierName     { get; set; } = string.Empty;
  public string    ScheduleType { get; set; } = string.Empty;
  public DateTime  ValidFrom    { get; set; }
  public DateTime? ValidTo      { get; set; }
  public string    Status       { get; set; } = string.Empty;
}

public class PriceHistoryRowDto
{
  public long     Id          { get; set; }
  public string   CountryCode { get; set; } = string.Empty;
  public string   TierCode    { get; set; } = string.Empty;
  public string   ChangeType  { get; set; } = string.Empty;
  public decimal  ChangedFrom { get; set; }
  public decimal  ChangedTo   { get; set; }
  public string   ChangedBy   { get; set; } = string.Empty;
  public DateTime CreatedAt   { get; set; }
}

public class ProductPricingRowDto
{
  public int     Id          { get; set; }
  public string  CountryCode { get; set; } = string.Empty;
  public string  CountryName { get; set; } = string.Empty;
  public string  TierCode    { get; set; } = string.Empty;
  public string  TierName    { get; set; } = string.Empty;
  public string? VariantCode { get; set; }
  public decimal Price       { get; set; }
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
