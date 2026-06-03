using Microsoft.EntityFrameworkCore;
using MyApp.Models;

namespace MyApp.Data;

public class AuditDbContext : DbContext
{
  public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }

  public DbSet<AuditLog>    AuditLogs    => Set<AuditLog>();
  public DbSet<UserSession> UserSessions => Set<UserSession>();

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

    modelBuilder.Entity<UserSession>(entity =>
    {
      entity.ToTable("user_sessions");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.SystemType).HasColumnName("system_type").HasMaxLength(50).IsRequired();
      entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
      entity.Property(e => e.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
      entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(200);
      entity.Property(e => e.CountryCode).HasColumnName("country_code").HasMaxLength(2);
      entity.Property(e => e.RegionId).HasColumnName("region_id");
      entity.Property(e => e.SessionToken).HasColumnName("session_token").HasMaxLength(200).IsRequired();
      entity.Property(e => e.IpAddress).HasColumnName("ip_address").HasMaxLength(50);
      entity.Property(e => e.Browser).HasColumnName("browser").HasMaxLength(200);
      entity.Property(e => e.Os).HasColumnName("os").HasMaxLength(100);
      entity.Property(e => e.DeviceType).HasColumnName("device_type").HasMaxLength(20);
      entity.Property(e => e.CurrentPage).HasColumnName("current_page").HasMaxLength(200);
      entity.Property(e => e.LastActiveAt).HasColumnName("last_active_at")
            .HasDefaultValueSql("now() AT TIME ZONE 'UTC'");
      entity.Property(e => e.LoginAt).HasColumnName("login_at")
            .HasDefaultValueSql("now() AT TIME ZONE 'UTC'");
      entity.Property(e => e.LogoutAt).HasColumnName("logout_at");
      entity.Property(e => e.IsActive).HasColumnName("is_active");
    });
  }
}
