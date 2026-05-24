using Microsoft.EntityFrameworkCore;
using MyApp.Models;

namespace MyApp.Data;

public class AuditDbContext : DbContext
{
  public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }

  public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<AuditLog>(entity =>
    {
      entity.ToTable("audit_logs");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id");
      entity.Property(e => e.TableName).HasColumnName("table_name");
      entity.Property(e => e.RecordId).HasColumnName("record_id");
      entity.Property(e => e.Action).HasColumnName("action");
      entity.Property(e => e.FieldName).HasColumnName("field_name");
      entity.Property(e => e.OldValue).HasColumnName("old_value");
      entity.Property(e => e.NewValue).HasColumnName("new_value");
      entity.Property(e => e.ChangedBy).HasColumnName("changed_by");
      entity.Property(e => e.ChangedAt).HasColumnName("changed_at");
      entity.Property(e => e.IpAddress).HasColumnName("ip_address");
      entity.Property(e => e.Remarks).HasColumnName("remarks");
    });
  }
}
