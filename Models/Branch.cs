using System.ComponentModel.DataAnnotations;

namespace MyApp.Models
{
  public class Branch
  {
    [Key]
    public Guid BranchId { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(500)]
    public string BranchName { get; set; } = "";

    public string? CreatedBy { get; set; } = "";
    public DateTime? CreatedDate { get; set; } = DateTime.Now;
    public string? LastUpdatedBy { get; set; } = "";
    public DateTime? LastUpdatedDate { get; set; } = DateTime.Now;
    public int IsActive { get; set; } = 1;
  }


  public class BranchUser
  {
    [Key]
    public Guid RecordId { get; set; } = Guid.NewGuid();
    public Guid BranchId { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; } = Guid.NewGuid();
  }
}
