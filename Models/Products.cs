using System.ComponentModel.DataAnnotations;

namespace MyApp.Models
{
  public class Products
  {
    [Key]
    public Guid ProductId { get; set; } = Guid.NewGuid();

    [StringLength(200)]
    public string ProductName { get; set; } = "";

    [StringLength(500)]
    public string Description { get; set; } = "";
    public Guid CategoryId { get; set; } = Guid.Empty;

    public string ImageFile { get; set; } = "";

    public decimal Price { get; set; } = 0;

    public string? CreatedBy { get; set; } = "";
    public DateTime? CreatedDate { get; set; } = DateTime.Now;
    public string? LastUpdatedBy { get; set; } = "";
    public DateTime? LastUpdatedDate { get; set; } = DateTime.Now;
    public int IsActive { get; set; } = 1;

  }

  public class ProductCategorys
  {
    [Key]
    public Guid CategoryId { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string CategoryName { get; set; } = "";

    public string? CreatedBy { get; set; } = "";
    public DateTime? CreatedDate { get; set; } = DateTime.Now;
    public string? LastUpdatedBy { get; set; } = "";
    public DateTime? LastUpdatedDate { get; set; } = DateTime.Now;
    public int IsActive { get; set; } = 1;

    public int SeqNo { get; set; } = 1;
  }
}
