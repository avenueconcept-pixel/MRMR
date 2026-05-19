namespace AspnetCoreStarter.Dtos
{
  public class Dto_ProductCategory
  {
    public Guid CategoryId { get; set; } = Guid.NewGuid();
    public string? CategoryName { get; set; } = "";
    public int SeqNo { get; set; }
    public int TotalProductCount { get; set; }
  }

  public class Dto_ProductList
  {
    public Guid productId { get; set; } = Guid.NewGuid();
    public string? productName { get; set; } = "";
    public Decimal price { get; set; }
    public string? imageFile { get; set; } = "";
    public string? imageFileUrl { get; set; } = "";

    public Guid categoryId { get; set; } = Guid.NewGuid();
    public string? category { get; set; } = "";

  }
}
