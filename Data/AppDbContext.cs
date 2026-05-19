using Microsoft.EntityFrameworkCore;
using MyApp.Models;

namespace MyApp.Data;

public class AppDbContext : DbContext
{
  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

  public DbSet<LanguageResource> LanguageResources => Set<LanguageResource>();
  public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
  public DbSet<Customer> Customers => Set<Customer>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    // LanguageResource
    modelBuilder.Entity<LanguageResource>(entity =>
    {
      entity.ToTable("language_resources");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.LanguageCode).HasColumnName("language_code").HasMaxLength(10).IsRequired();
      entity.Property(e => e.Key).HasColumnName("key").HasMaxLength(100).IsRequired();
      entity.Property(e => e.Value).HasColumnName("value").IsRequired();
    });

    // AdminUser
    modelBuilder.Entity<AdminUser>(entity =>
    {
      entity.ToTable("admin_users");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.Username).HasColumnName("username").HasMaxLength(50).IsRequired();
      entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();
      entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(100).IsRequired();
      entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100).IsRequired();
      entity.Property(e => e.IsActive).HasColumnName("is_active");
      entity.Property(e => e.CreatedAt).HasColumnName("created_at");
      entity.Property(e => e.LastLogin).HasColumnName("last_login");
    });

    // Customer
    modelBuilder.Entity<Customer>(entity =>
    {
      entity.ToTable("customers");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(100).IsRequired();
      entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100).IsRequired();
      entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();
      entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
      entity.Property(e => e.Address).HasColumnName("address");
      entity.Property(e => e.IsActive).HasColumnName("is_active");
      entity.Property(e => e.RegisteredAt).HasColumnName("registered_at");
      entity.Property(e => e.LastLogin).HasColumnName("last_login");
      entity.Property(e => e.LanguageCode).HasColumnName("language_code").HasMaxLength(10);
    });

  }
}
