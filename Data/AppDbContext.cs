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
  public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
  public DbSet<AppLog> AppLogs => Set<AppLog>();
  public DbSet<Country> Countries => Set<Country>();
  public DbSet<CountryTranslation> CountryTranslations => Set<CountryTranslation>();
  public DbSet<Department> Departments => Set<Department>();
  public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
  public DbSet<PaymentMethodTranslation> PaymentMethodTranslations => Set<PaymentMethodTranslation>();

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
      entity.Property(e => e.CountryCode).HasColumnName("country_code").HasMaxLength(2);
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
      entity.Property(e => e.CreatedAt).HasColumnName("created_at");
      entity.Property(e => e.LastLogin).HasColumnName("last_login");
      entity.Property(e => e.LastLoginLangCode).HasColumnName("last_login_lang_code").HasMaxLength(10);
      entity.HasOne(e => e.Country).WithMany().HasForeignKey(e => e.CountryCode);
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
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);
    });

    // PasswordResetToken
    modelBuilder.Entity<PasswordResetToken>(entity =>
    {
      entity.ToTable("password_reset_tokens");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.UserType).HasColumnName("user_type").HasMaxLength(20).IsRequired();
      entity.Property(e => e.UserId).HasColumnName("user_id");
      entity.Property(e => e.Token).HasColumnName("token").HasMaxLength(64).IsRequired();
      entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
      entity.Property(e => e.IsUsed).HasColumnName("is_used");
      entity.Property(e => e.CreatedAt).HasColumnName("created_at");
    });

    // AppLog
    modelBuilder.Entity<AppLog>(entity =>
    {
      entity.ToTable("app_logs");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.LogLevel).HasColumnName("log_level").HasMaxLength(20).IsRequired();
      entity.Property(e => e.Category).HasColumnName("category").HasMaxLength(255).IsRequired();
      entity.Property(e => e.Message).HasColumnName("message").IsRequired();
      entity.Property(e => e.Exception).HasColumnName("exception");
      entity.Property(e => e.CreatedAt).HasColumnName("created_at");
    });

    // Country
    modelBuilder.Entity<Country>(entity =>
    {
      entity.ToTable("countries");
      entity.HasKey(e => e.CountryCode);
      entity.Property(e => e.CountryCode).HasColumnName("country_code").HasMaxLength(2).IsRequired();
      entity.Property(e => e.CurrencyCode).HasColumnName("currency_code").HasMaxLength(3);
      entity.Property(e => e.Timezone).HasColumnName("timezone").HasMaxLength(100);
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
      entity.Property(e => e.CreatedAt).HasColumnName("created_at");
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(50);
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
      entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(50);
      entity.HasMany(e => e.Translations).WithOne(t => t.Country).HasForeignKey(t => t.CountryCode);
    });

    // CountryTranslation
    modelBuilder.Entity<CountryTranslation>(entity =>
    {
      entity.ToTable("country_translations");
      entity.HasKey(e => new { e.CountryCode, e.LanguageCode });
      entity.Property(e => e.CountryCode).HasColumnName("country_code").HasMaxLength(2).IsRequired();
      entity.Property(e => e.LanguageCode).HasColumnName("language_code").HasMaxLength(10).IsRequired();
      entity.Property(e => e.CountryName).HasColumnName("country_name").HasMaxLength(150).IsRequired();
      entity.HasOne(e => e.Country).WithMany(c => c.Translations).HasForeignKey(e => e.CountryCode);
    });

    // Department
    modelBuilder.Entity<Department>(entity =>
    {
      entity.ToTable("departments");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.DeptName).HasColumnName("dept_name").HasMaxLength(150).IsRequired();
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);
    });

    // PaymentMethod
    modelBuilder.Entity<PaymentMethod>(entity =>
    {
      entity.ToTable("payment_methods");
      entity.HasKey(e => e.PaymentCode);
      entity.Property(e => e.PaymentCode).HasColumnName("payment_code").HasMaxLength(20).IsRequired();
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);
      entity.HasMany(e => e.Translations).WithOne(t => t.PaymentMethod).HasForeignKey(t => t.PaymentCode).OnDelete(DeleteBehavior.Cascade);
    });

    // PaymentMethodTranslation
    modelBuilder.Entity<PaymentMethodTranslation>(entity =>
    {
      entity.ToTable("payment_method_translations");
      entity.HasKey(e => new { e.PaymentCode, e.LanguageCode });
      entity.Property(e => e.PaymentCode).HasColumnName("payment_code").HasMaxLength(20).IsRequired();
      entity.Property(e => e.LanguageCode).HasColumnName("language_code").HasMaxLength(10).IsRequired();
      entity.Property(e => e.PaymentName).HasColumnName("payment_name").HasMaxLength(150).IsRequired();
      entity.HasOne(e => e.PaymentMethod).WithMany(p => p.Translations).HasForeignKey(e => e.PaymentCode);
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
