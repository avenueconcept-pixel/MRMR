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
  public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
  public DbSet<ProductCategoryTranslation> ProductCategoryTranslations => Set<ProductCategoryTranslation>();
  public DbSet<State>            States            => Set<State>();
  public DbSet<StateTranslation> StateTranslations => Set<StateTranslation>();
  public DbSet<Region>           Regions           => Set<Region>();
  public DbSet<RegionCountry>    RegionCountries   => Set<RegionCountry>();
  public DbSet<Location>         Locations         => Set<Location>();
  public DbSet<Role>             Roles             => Set<Role>();
  public DbSet<RoleMenu>         RoleMenus         => Set<RoleMenu>();
  public DbSet<RolePermission>   RolePermissions   => Set<RolePermission>();
  public DbSet<Menu>             Menus             => Set<Menu>();
  public DbSet<Permission>       Permissions       => Set<Permission>();
  public DbSet<Bank>             Banks             => Set<Bank>();
  public DbSet<BankTranslation>  BankTranslations  => Set<BankTranslation>();
  public DbSet<UnitOfMeasure>    UnitsOfMeasure    => Set<UnitOfMeasure>();
  public DbSet<UomTranslation>   UomTranslations   => Set<UomTranslation>();
  public DbSet<PriceTier>        PriceTiers        => Set<PriceTier>();
  public DbSet<ProductSectionType>            ProductSectionTypes            => Set<ProductSectionType>();
  public DbSet<ProductSectionTypeTranslation> ProductSectionTypeTranslations => Set<ProductSectionTypeTranslation>();

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
      entity.HasIndex(e => new { e.Key, e.LanguageCode }).IsUnique();
    });

    // AdminUser
    modelBuilder.Entity<AdminUser>(entity =>
    {
      entity.ToTable("admin_users");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
      entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(500).IsRequired();
      entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(200).IsRequired();
      entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(200).IsRequired();
      entity.Property(e => e.RoleId).HasColumnName("role_id");
      entity.Property(e => e.DeptId).HasColumnName("dept_id");
      entity.Property(e => e.CountryCode).HasColumnName("country_code").HasMaxLength(2).HasDefaultValue("MY");
      entity.Property(e => e.RegionId).HasColumnName("region_id");
      entity.Property(e => e.MobileCountryCode).HasColumnName("mobile_country_code").HasMaxLength(10);
      entity.Property(e => e.MobileNo).HasColumnName("mobile_no").HasMaxLength(20);
      entity.Property(e => e.IsForceChangePassword).HasColumnName("is_force_change_password").HasDefaultValue(false);
      entity.Property(e => e.ProfileImage).HasColumnName("profile_image").HasMaxLength(500);
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
      entity.Property(e => e.LastLoginAt).HasColumnName("last_login_at");
      entity.Property(e => e.LastLoginLang).HasColumnName("last_login_lang").HasMaxLength(10);
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);
      entity.HasOne(e => e.Role).WithMany().HasForeignKey(e => e.RoleId).OnDelete(DeleteBehavior.Restrict);
      entity.HasOne(e => e.Department).WithMany().HasForeignKey(e => e.DeptId).OnDelete(DeleteBehavior.NoAction);
      entity.HasOne(e => e.Country).WithMany().HasForeignKey(e => e.CountryCode).OnDelete(DeleteBehavior.NoAction);
      entity.HasOne(e => e.Region).WithMany().HasForeignKey(e => e.RegionId).OnDelete(DeleteBehavior.NoAction);
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

    // ProductCategory
    modelBuilder.Entity<ProductCategory>(entity =>
    {
      entity.ToTable("product_categories");
      entity.HasKey(e => e.CategoryCode);
      entity.Property(e => e.CategoryCode).HasColumnName("category_code").HasMaxLength(20).IsRequired();
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);
      entity.HasMany(e => e.Translations).WithOne(t => t.ProductCategory).HasForeignKey(t => t.CategoryCode).OnDelete(DeleteBehavior.Cascade);
    });

    // ProductCategoryTranslation
    modelBuilder.Entity<ProductCategoryTranslation>(entity =>
    {
      entity.ToTable("product_category_translations");
      entity.HasKey(e => new { e.CategoryCode, e.LanguageCode });
      entity.Property(e => e.CategoryCode).HasColumnName("category_code").HasMaxLength(20).IsRequired();
      entity.Property(e => e.LanguageCode).HasColumnName("language_code").HasMaxLength(10).IsRequired();
      entity.Property(e => e.CategoryName).HasColumnName("category_name").HasMaxLength(150).IsRequired();
      entity.HasOne(e => e.ProductCategory).WithMany(p => p.Translations).HasForeignKey(e => e.CategoryCode);
    });

    // State
    modelBuilder.Entity<State>(entity =>
    {
      entity.ToTable("states");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.CountryCode).HasColumnName("country_code").HasMaxLength(10).IsRequired();
      entity.Property(e => e.StateCode).HasColumnName("state_code").HasMaxLength(20).IsRequired();
      entity.Property(e => e.SortOrder).HasColumnName("sort_order");
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100).IsRequired();

      entity.HasOne(e => e.Country)
            .WithMany()
            .HasForeignKey(e => e.CountryCode)
            .OnDelete(DeleteBehavior.Cascade);

      entity.HasMany(e => e.Translations)
            .WithOne(t => t.State)
            .HasForeignKey(t => t.StateId)
            .OnDelete(DeleteBehavior.Cascade);
    });

    // StateTranslation
    modelBuilder.Entity<StateTranslation>(entity =>
    {
      entity.ToTable("state_translations");
      entity.HasKey(e => new { e.StateId, e.LanguageCode });
      entity.Property(e => e.StateId).HasColumnName("state_id");
      entity.Property(e => e.LanguageCode).HasColumnName("language_code").HasMaxLength(10).IsRequired();
      entity.Property(e => e.StateName).HasColumnName("state_name").HasMaxLength(200).IsRequired();
    });

    // Region
    modelBuilder.Entity<Region>(entity =>
    {
      entity.ToTable("regions");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.RegionCode).HasColumnName("region_code").HasMaxLength(50).IsRequired();
      entity.Property(e => e.RegionName).HasColumnName("region_name").HasMaxLength(100).IsRequired();
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);
      entity.HasIndex(e => e.RegionCode).IsUnique();
      entity.HasMany(e => e.RegionCountries)
            .WithOne(rc => rc.Region)
            .HasForeignKey(rc => rc.RegionId)
            .OnDelete(DeleteBehavior.Cascade);
    });

    // RegionCountry
    modelBuilder.Entity<RegionCountry>(entity =>
    {
      entity.ToTable("region_countries");
      entity.HasKey(e => new { e.RegionId, e.CountryCode });
      entity.Property(e => e.RegionId).HasColumnName("region_id");
      entity.Property(e => e.CountryCode).HasColumnName("country_code").HasMaxLength(2).IsRequired();
      entity.HasOne(e => e.Country).WithMany().HasForeignKey(e => e.CountryCode);
    });

    // Location
    modelBuilder.Entity<Location>(entity =>
    {
      entity.ToTable("locations");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.LocationCode).HasColumnName("location_code").HasMaxLength(50).IsRequired();
      entity.Property(e => e.LocationName).HasColumnName("location_name").HasMaxLength(150).IsRequired();
      entity.Property(e => e.LocationType).HasColumnName("location_type").HasMaxLength(20).IsRequired();
      entity.Property(e => e.CountryCode).HasColumnName("country_code").HasMaxLength(2).IsRequired();
      entity.Property(e => e.StateId).HasColumnName("state_id");
      entity.Property(e => e.City).HasColumnName("city").HasMaxLength(100);
      entity.Property(e => e.Postcode).HasColumnName("postcode").HasMaxLength(20);
      entity.Property(e => e.Address).HasColumnName("address").HasMaxLength(500);
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);
      entity.HasIndex(e => e.LocationCode).IsUnique();
      entity.HasOne(e => e.Country).WithMany().HasForeignKey(e => e.CountryCode);
      entity.HasOne(e => e.State).WithMany().HasForeignKey(e => e.StateId).OnDelete(DeleteBehavior.SetNull);
    });

    // Role
    modelBuilder.Entity<Role>(entity =>
    {
      entity.ToTable("roles");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.RoleCode).HasColumnName("role_code").HasMaxLength(50).IsRequired();
      entity.Property(e => e.RoleName).HasColumnName("role_name").HasMaxLength(150).IsRequired();
      entity.Property(e => e.Description).HasColumnName("description");
      entity.Property(e => e.IsSuperAdmin).HasColumnName("is_super_admin").HasDefaultValue(false);
      entity.Property(e => e.DataScope).HasColumnName("data_scope").HasMaxLength(20).IsRequired();
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);
      entity.HasIndex(e => e.RoleCode).IsUnique();
      entity.HasMany(e => e.RoleMenus)
            .WithOne(rm => rm.Role)
            .HasForeignKey(rm => rm.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
      entity.HasMany(e => e.RolePermissions)
            .WithOne(rp => rp.Role)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    });

    // RoleMenu
    modelBuilder.Entity<RoleMenu>(entity =>
    {
      entity.ToTable("role_menus");
      entity.HasKey(e => new { e.RoleId, e.MenuId });
      entity.Property(e => e.RoleId).HasColumnName("role_id");
      entity.Property(e => e.MenuId).HasColumnName("menu_id");
      entity.HasOne(e => e.Menu).WithMany().HasForeignKey(e => e.MenuId);
    });

    // RolePermission
    modelBuilder.Entity<RolePermission>(entity =>
    {
      entity.ToTable("role_permissions");
      entity.HasKey(e => new { e.RoleId, e.PermissionId });
      entity.Property(e => e.RoleId).HasColumnName("role_id");
      entity.Property(e => e.PermissionId).HasColumnName("permission_id");
      entity.Property(e => e.IsGranted).HasColumnName("is_granted");
      entity.HasOne(e => e.Permission).WithMany().HasForeignKey(e => e.PermissionId);
    });

    // Menu
    modelBuilder.Entity<Menu>(entity =>
    {
      entity.ToTable("menus");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.MenuCode).HasColumnName("menu_code").HasMaxLength(50).IsRequired();
      entity.Property(e => e.ParentId).HasColumnName("parent_id");
      entity.Property(e => e.MenuName).HasColumnName("menu_name").HasMaxLength(100).IsRequired();
      entity.Property(e => e.MenuIcon).HasColumnName("menu_icon").HasMaxLength(100);
      entity.Property(e => e.MenuUrl).HasColumnName("menu_url").HasMaxLength(500);
      entity.Property(e => e.SortOrder).HasColumnName("sort_order");
      entity.Property(e => e.Level).HasColumnName("level");
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);
      entity.HasOne(e => e.Parent).WithMany(m => m.Children).HasForeignKey(e => e.ParentId).OnDelete(DeleteBehavior.Restrict);
      entity.HasMany(e => e.Permissions).WithOne(p => p.Menu).HasForeignKey(p => p.MenuId).OnDelete(DeleteBehavior.Cascade);
    });

    // Permission
    modelBuilder.Entity<Permission>(entity =>
    {
      entity.ToTable("permissions");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.PermissionCode).HasColumnName("permission_code").HasMaxLength(50).IsRequired();
      entity.Property(e => e.MenuId).HasColumnName("menu_id");
      entity.Property(e => e.PermissionName).HasColumnName("permission_name").HasMaxLength(100).IsRequired();
      entity.Property(e => e.SortOrder).HasColumnName("sort_order");
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
    });

    // Bank
    modelBuilder.Entity<Bank>(entity =>
    {
      entity.ToTable("banks");
      entity.HasKey(e => e.BankCode);
      entity.Property(e => e.BankCode).HasColumnName("bank_code").HasMaxLength(20).IsRequired();
      entity.Property(e => e.CountryCode).HasColumnName("country_code").HasMaxLength(10).IsRequired();
      entity.Property(e => e.SwiftCode).HasColumnName("swift_code").HasMaxLength(20);
      entity.Property(e => e.LocalCode).HasColumnName("local_code").HasMaxLength(20);
      entity.Property(e => e.Website).HasColumnName("website").HasMaxLength(200);
      entity.Property(e => e.Logo).HasColumnName("logo").HasMaxLength(200);
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
      entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100).IsRequired();

      entity.HasOne(e => e.Country)
            .WithMany()
            .HasForeignKey(e => e.CountryCode)
            .OnDelete(DeleteBehavior.Restrict);

      entity.HasMany(e => e.Translations)
            .WithOne(t => t.Bank)
            .HasForeignKey(t => t.BankCode)
            .OnDelete(DeleteBehavior.Cascade);
    });

    // BankTranslation
    modelBuilder.Entity<BankTranslation>(entity =>
    {
      entity.ToTable("bank_translations");
      entity.HasKey(e => new { e.BankCode, e.LanguageCode });
      entity.Property(e => e.BankCode).HasColumnName("bank_code").HasMaxLength(20).IsRequired();
      entity.Property(e => e.LanguageCode).HasColumnName("language_code").HasMaxLength(10).IsRequired();
      entity.Property(e => e.BankName).HasColumnName("bank_name").HasMaxLength(200).IsRequired();
      entity.Property(e => e.ShortName).HasColumnName("short_name").HasMaxLength(100).IsRequired();
    });

    // UnitOfMeasure
    modelBuilder.Entity<UnitOfMeasure>(entity =>
    {
      entity.ToTable("units_of_measure");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.UomCode).HasColumnName("uom_code").HasMaxLength(20).IsRequired();
      entity.Property(e => e.UomName).HasColumnName("uom_name").HasMaxLength(100).IsRequired();
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100).IsRequired();
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.HasMany(e => e.Translations)
            .WithOne(t => t.UnitOfMeasure)
            .HasForeignKey(t => t.UomCode)
            .HasPrincipalKey(e => e.UomCode)
            .OnDelete(DeleteBehavior.Cascade);
    });

    // UomTranslation
    modelBuilder.Entity<UomTranslation>(entity =>
    {
      entity.ToTable("uom_translations");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.UomCode).HasColumnName("uom_code").HasMaxLength(20).IsRequired();
      entity.Property(e => e.LanguageCode).HasColumnName("language_code").HasMaxLength(10).IsRequired();
      entity.Property(e => e.UomName).HasColumnName("uom_name").HasMaxLength(100).IsRequired();
    });

    // PriceTier
    modelBuilder.Entity<PriceTier>(entity =>
    {
      entity.ToTable("price_tiers");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.TierCode).HasColumnName("tier_code").HasMaxLength(20).IsRequired();
      entity.Property(e => e.TierName).HasColumnName("tier_name").HasMaxLength(100).IsRequired();
      entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100).IsRequired();
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
    });

    // ProductSectionType
    modelBuilder.Entity<ProductSectionType>(entity =>
    {
      entity.ToTable("product_section_types");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.SectionCode).HasColumnName("section_code").HasMaxLength(50).IsRequired();
      entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100).IsRequired();
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.HasMany(e => e.Translations)
            .WithOne(t => t.ProductSectionType)
            .HasForeignKey(t => t.SectionCode)
            .HasPrincipalKey(e => e.SectionCode)
            .OnDelete(DeleteBehavior.Cascade);
    });

    // ProductSectionTypeTranslation
    modelBuilder.Entity<ProductSectionTypeTranslation>(entity =>
    {
      entity.ToTable("product_section_type_translations");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.SectionCode).HasColumnName("section_code").HasMaxLength(50).IsRequired();
      entity.Property(e => e.LanguageCode).HasColumnName("language_code").HasMaxLength(10).IsRequired();
      entity.Property(e => e.SectionName).HasColumnName("section_name").HasMaxLength(200).IsRequired();
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
