using Microsoft.EntityFrameworkCore;
using MyApp.Models;

namespace MyApp.Data;

public class AppDbContext : DbContext
{
  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

  public DbSet<LanguageResource> LanguageResources => Set<LanguageResource>();
  public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
  public DbSet<Customer> Customers => Set<Customer>();
  public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
  public DbSet<Language> Languages => Set<Language>();

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
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
      entity.Property(e => e.CreatedAt).HasColumnName("created_at");
      entity.Property(e => e.LastLogin).HasColumnName("last_login");
      entity.Property(e => e.LastLoginLangCode).HasColumnName("last_login_lang_code").HasMaxLength(10);
    });

    // EmailTemplate
    modelBuilder.Entity<EmailTemplate>(entity =>
    {
      entity.ToTable("email_templates");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.LanguageCode).HasColumnName("language_code").HasMaxLength(10).IsRequired();
      entity.Property(e => e.TemplateKey).HasColumnName("template_key").HasMaxLength(100).IsRequired();
      entity.Property(e => e.Subject).HasColumnName("subject").HasMaxLength(255).IsRequired();
      entity.Property(e => e.BodyHtml).HasColumnName("body_html").IsRequired();
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
      entity.Property(e => e.CreatedAt).HasColumnName("created_at");
    });

    // Language
    modelBuilder.Entity<Language>(entity =>
    {
      entity.ToTable("languages");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.LanguageCode).HasColumnName("language_code").HasMaxLength(10).IsRequired();
      entity.Property(e => e.LanguageName).HasColumnName("language_name").HasMaxLength(100).IsRequired();
      entity.Property(e => e.NativeName).HasColumnName("native_name").HasMaxLength(100).IsRequired();
      entity.Property(e => e.SortOrder).HasColumnName("sort_order");
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
      entity.Property(e => e.CreatedAt).HasColumnName("created_at");
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
