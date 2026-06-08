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
  public DbSet<Announcement>            Announcements            => Set<Announcement>();
  public DbSet<AnnouncementTranslation> AnnouncementTranslations => Set<AnnouncementTranslation>();
  public DbSet<AnnouncementAttachment>  AnnouncementAttachments  => Set<AnnouncementAttachment>();
  public DbSet<AppSystem>                  AppSystems                  => Set<AppSystem>();
  public DbSet<MaintenanceSchedule>        MaintenanceSchedules        => Set<MaintenanceSchedule>();
  public DbSet<MaintenanceScheduleSystem>  MaintenanceScheduleSystems  => Set<MaintenanceScheduleSystem>();
  public DbSet<MaintenanceScheduleMessage> MaintenanceScheduleMessages => Set<MaintenanceScheduleMessage>();
  public DbSet<AppSetting>                 AppSettings                 => Set<AppSetting>();
  public DbSet<Product>                    Products                    => Set<Product>();
  public DbSet<ProductTranslation>         ProductTranslations         => Set<ProductTranslation>();
  public DbSet<ProductCategoryMap>         ProductCategoryMaps         => Set<ProductCategoryMap>();
  public DbSet<ProductCountry>             ProductCountries            => Set<ProductCountry>();
  public DbSet<ProductPriceTier>           ProductPriceTiers           => Set<ProductPriceTier>();
  public DbSet<ProductPriceSchedule>       ProductPriceSchedules       => Set<ProductPriceSchedule>();
  public DbSet<ProductPriceHistory>        ProductPriceHistories       => Set<ProductPriceHistory>();
  public DbSet<ProductSection>             ProductSections             => Set<ProductSection>();
  public DbSet<ProductSectionTranslation>  ProductSectionTranslations  => Set<ProductSectionTranslation>();
  public DbSet<ProductImage>               ProductImages               => Set<ProductImage>();
  public DbSet<ProductPackageItem>         ProductPackageItems         => Set<ProductPackageItem>();
  public DbSet<Rank>              Ranks              => Set<Rank>();
  public DbSet<Member>            Members            => Set<Member>();
  public DbSet<MemberRankHistory> MemberRankHistories => Set<MemberRankHistory>();
  public DbSet<MemberChangeLog>   MemberChangeLogs   => Set<MemberChangeLog>();

  public DbSet<ExchangeRate>                     ExchangeRates                     => Set<ExchangeRate>();
  public DbSet<WalletBalance>                    WalletBalances                    => Set<WalletBalance>();
  public DbSet<CashWalletTransaction>            CashWalletTransactions            => Set<CashWalletTransaction>();
  public DbSet<CashWalletTransactionArchive>     CashWalletTransactionArchives     => Set<CashWalletTransactionArchive>();
  public DbSet<PurchaseWalletTransaction>        PurchaseWalletTransactions        => Set<PurchaseWalletTransaction>();
  public DbSet<PurchaseWalletTransactionArchive> PurchaseWalletTransactionArchives => Set<PurchaseWalletTransactionArchive>();
  public DbSet<IncentivePeriod>                  IncentivePeriods                  => Set<IncentivePeriod>();
  public DbSet<WalletPayout>                     WalletPayouts                     => Set<WalletPayout>();
  public DbSet<SystemSetting>                    SystemSettings                    => Set<SystemSetting>();

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
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
      entity.Property(e => e.CreatedAt).HasColumnName("created_at")
            .HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at")
            .HasDefaultValueSql("now() AT TIME ZONE 'utc'");
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
      entity.Property(e => e.CurrencySymbol).HasColumnName("currency_symbol").HasMaxLength(5);
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
      entity.Ignore(e => e.Permissions);
    });

    // Permission
    modelBuilder.Entity<Permission>(entity =>
    {
      entity.ToTable("permissions");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.PermissionCode).HasColumnName("permission_code").HasMaxLength(50).IsRequired();
      entity.Property(e => e.Module).HasColumnName("module").HasMaxLength(50);
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

    // Announcement
    modelBuilder.Entity<Announcement>(entity =>
    {
      entity.ToTable("announcements");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.AnnouncementCode).HasColumnName("announcement_code").HasMaxLength(50).IsRequired();
      entity.Property(e => e.Audience).HasColumnName("audience").HasMaxLength(20).IsRequired();
      entity.Property(e => e.StartAt).HasColumnName("start_at");
      entity.Property(e => e.EndAt).HasColumnName("end_at");
      entity.Property(e => e.SortOrder).HasColumnName("sort_order");
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
      entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.HasIndex(e => e.AnnouncementCode).IsUnique();

      entity.HasMany(e => e.Translations)
            .WithOne(t => t.Announcement)
            .HasForeignKey(t => t.AnnouncementCode)
            .HasPrincipalKey(e => e.AnnouncementCode)
            .OnDelete(DeleteBehavior.Cascade);

      entity.HasMany(e => e.Attachments)
            .WithOne(a => a.Announcement)
            .HasForeignKey(a => a.AnnouncementCode)
            .HasPrincipalKey(e => e.AnnouncementCode)
            .OnDelete(DeleteBehavior.Cascade);
    });

    // AnnouncementTranslation
    modelBuilder.Entity<AnnouncementTranslation>(entity =>
    {
      entity.ToTable("announcement_translations");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.AnnouncementCode).HasColumnName("announcement_code").HasMaxLength(50).IsRequired();
      entity.Property(e => e.LanguageCode).HasColumnName("language_code").HasMaxLength(10).IsRequired();
      entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(300);
      entity.Property(e => e.Body).HasColumnName("body");
      entity.HasIndex(e => new { e.AnnouncementCode, e.LanguageCode }).IsUnique();
    });

    // AnnouncementAttachment
    modelBuilder.Entity<AnnouncementAttachment>(entity =>
    {
      entity.ToTable("announcement_attachments");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.AnnouncementCode).HasColumnName("announcement_code").HasMaxLength(50).IsRequired();
      entity.Property(e => e.FileName).HasColumnName("file_name").HasMaxLength(300);
      entity.Property(e => e.OriginalName).HasColumnName("original_name").HasMaxLength(300);
      entity.Property(e => e.FileType).HasColumnName("file_type").HasMaxLength(10);
      entity.Property(e => e.SortOrder).HasColumnName("sort_order");
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
    });

    // AppSystem
    modelBuilder.Entity<AppSystem>(entity =>
    {
      entity.ToTable("systems");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.SystemCode).HasColumnName("system_code").HasMaxLength(50).IsRequired();
      entity.Property(e => e.SystemName).HasColumnName("system_name").HasMaxLength(100).IsRequired();
      entity.Property(e => e.SortOrder).HasColumnName("sort_order");
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.HasIndex(e => e.SystemCode).IsUnique();
    });

    // MaintenanceSchedule
    modelBuilder.Entity<MaintenanceSchedule>(entity =>
    {
      entity.ToTable("maintenance_schedules");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
      entity.Property(e => e.StartAt).HasColumnName("start_at");
      entity.Property(e => e.EndAt).HasColumnName("end_at");
      entity.Property(e => e.IsActive).HasColumnName("is_active");
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");

      entity.HasMany(e => e.Systems)
            .WithOne(s => s.Maintenance)
            .HasForeignKey(s => s.MaintenanceId)
            .OnDelete(DeleteBehavior.Cascade);

      entity.HasMany(e => e.Messages)
            .WithOne(m => m.Maintenance)
            .HasForeignKey(m => m.MaintenanceId)
            .OnDelete(DeleteBehavior.Cascade);
    });

    // MaintenanceScheduleSystem
    modelBuilder.Entity<MaintenanceScheduleSystem>(entity =>
    {
      entity.ToTable("maintenance_schedule_systems");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.MaintenanceId).HasColumnName("maintenance_id");
      entity.Property(e => e.SystemCode).HasColumnName("system_code").HasMaxLength(50);
      entity.HasOne(e => e.System)
            .WithMany()
            .HasForeignKey(e => e.SystemCode)
            .HasPrincipalKey(e => e.SystemCode)
            .OnDelete(DeleteBehavior.Cascade);
    });

    // MaintenanceScheduleMessage
    modelBuilder.Entity<MaintenanceScheduleMessage>(entity =>
    {
      entity.ToTable("maintenance_schedule_messages");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.MaintenanceId).HasColumnName("maintenance_id");
      entity.Property(e => e.LanguageCode).HasColumnName("language_code").HasMaxLength(10);
      entity.Property(e => e.Message).HasColumnName("message");
      entity.HasIndex(e => new { e.MaintenanceId, e.LanguageCode }).IsUnique();
    });

    // AppSetting
    modelBuilder.Entity<AppSetting>(entity =>
    {
      entity.ToTable("app_settings");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.SystemCode).HasColumnName("system_code").HasMaxLength(50).IsRequired();
      entity.Property(e => e.SettingKey).HasColumnName("setting_key").HasMaxLength(100).IsRequired();
      entity.Property(e => e.SettingValue).HasColumnName("setting_value").IsRequired();
      entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100).IsRequired();
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at")
            .HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.HasIndex(e => new { e.SystemCode, e.SettingKey }).IsUnique();
    });

    // Product
    modelBuilder.Entity<Product>(entity =>
    {
      entity.ToTable("products");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.ProductCode).HasColumnName("product_code").HasMaxLength(50).IsRequired();
      entity.Property(e => e.ProductType).HasColumnName("product_type").HasMaxLength(20).IsRequired();
      entity.Property(e => e.ProductNature).HasColumnName("product_nature").HasMaxLength(20).IsRequired();
      entity.Property(e => e.UomCode).HasColumnName("uom_code").HasMaxLength(20).IsRequired();
      entity.Property(e => e.Pv).HasColumnName("pv").HasPrecision(10, 2);
      entity.Property(e => e.SortOrder).HasColumnName("sort_order");
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100).IsRequired();
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.HasIndex(e => e.ProductCode).IsUnique();

      entity.HasOne(e => e.UnitOfMeasure)
            .WithMany()
            .HasForeignKey(e => e.UomCode)
            .HasPrincipalKey(e => e.UomCode)
            .OnDelete(DeleteBehavior.Restrict);

      entity.HasMany(e => e.Translations)
            .WithOne(t => t.Product)
            .HasForeignKey(t => t.ProductCode)
            .HasPrincipalKey(e => e.ProductCode)
            .OnDelete(DeleteBehavior.Cascade);

      entity.HasMany(e => e.CategoryMaps)
            .WithOne(c => c.Product)
            .HasForeignKey(c => c.ProductCode)
            .HasPrincipalKey(e => e.ProductCode)
            .OnDelete(DeleteBehavior.Cascade);

      entity.HasMany(e => e.Countries)
            .WithOne(c => c.Product)
            .HasForeignKey(c => c.ProductCode)
            .HasPrincipalKey(e => e.ProductCode)
            .OnDelete(DeleteBehavior.Cascade);

      entity.HasMany(e => e.PriceTiers)
            .WithOne(p => p.Product)
            .HasForeignKey(p => p.ProductCode)
            .HasPrincipalKey(e => e.ProductCode)
            .OnDelete(DeleteBehavior.Cascade);

      entity.HasMany(e => e.Sections)
            .WithOne(s => s.Product)
            .HasForeignKey(s => s.ProductCode)
            .HasPrincipalKey(e => e.ProductCode)
            .OnDelete(DeleteBehavior.Cascade);

      entity.HasMany(e => e.Images)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductCode)
            .HasPrincipalKey(e => e.ProductCode)
            .OnDelete(DeleteBehavior.Cascade);

      entity.HasMany(e => e.PackageItems)
            .WithOne(p => p.PackageProduct)
            .HasForeignKey(p => p.PackageProductCode)
            .HasPrincipalKey(e => e.ProductCode)
            .OnDelete(DeleteBehavior.Cascade);
    });

    // ProductTranslation
    modelBuilder.Entity<ProductTranslation>(entity =>
    {
      entity.ToTable("product_translations");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.ProductCode).HasColumnName("product_code").HasMaxLength(50).IsRequired();
      entity.Property(e => e.LanguageCode).HasColumnName("language_code").HasMaxLength(10).IsRequired();
      entity.Property(e => e.ProductName).HasColumnName("product_name").HasMaxLength(200).IsRequired();
      entity.Property(e => e.ShortDescription).HasColumnName("short_description").IsRequired();
      entity.HasIndex(e => new { e.ProductCode, e.LanguageCode }).IsUnique();
    });

    // ProductCategoryMap
    modelBuilder.Entity<ProductCategoryMap>(entity =>
    {
      entity.ToTable("product_category_map");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.ProductCode).HasColumnName("product_code").HasMaxLength(50).IsRequired();
      entity.Property(e => e.CategoryCode).HasColumnName("category_code").HasMaxLength(20).IsRequired();
      entity.HasIndex(e => new { e.ProductCode, e.CategoryCode }).IsUnique();

      entity.HasOne(e => e.ProductCategory)
            .WithMany()
            .HasForeignKey(e => e.CategoryCode)
            .HasPrincipalKey(e => e.CategoryCode)
            .OnDelete(DeleteBehavior.Restrict);
    });

    // ProductCountry
    modelBuilder.Entity<ProductCountry>(entity =>
    {
      entity.ToTable("product_countries");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.ProductCode).HasColumnName("product_code").HasMaxLength(50).IsRequired();
      entity.Property(e => e.CountryCode).HasColumnName("country_code").HasMaxLength(2).IsRequired();
      entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
      entity.Property(e => e.StockStatus).HasColumnName("stock_status").HasMaxLength(20).IsRequired();
      entity.HasIndex(e => new { e.ProductCode, e.CountryCode }).IsUnique();

      entity.HasOne(e => e.Country)
            .WithMany()
            .HasForeignKey(e => e.CountryCode)
            .OnDelete(DeleteBehavior.Restrict);
    });

    // ProductPriceTier
    modelBuilder.Entity<ProductPriceTier>(entity =>
    {
      entity.ToTable("product_price_tiers");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.ProductCode).HasColumnName("product_code").HasMaxLength(50).IsRequired();
      entity.Property(e => e.CountryCode).HasColumnName("country_code").HasMaxLength(2).IsRequired();
      entity.Property(e => e.TierCode).HasColumnName("tier_code").HasMaxLength(20).IsRequired();
      entity.Property(e => e.VariantCode).HasColumnName("variant_code").HasMaxLength(50);
      entity.Property(e => e.Price).HasColumnName("price").HasPrecision(10, 2);

      entity.HasOne(e => e.Country)
            .WithMany()
            .HasForeignKey(e => e.CountryCode)
            .OnDelete(DeleteBehavior.Restrict);

      entity.HasOne(e => e.PriceTier)
            .WithMany()
            .HasForeignKey(e => e.TierCode)
            .HasPrincipalKey(e => e.TierCode)
            .OnDelete(DeleteBehavior.Restrict);
    });

    // ProductPriceSchedule
    modelBuilder.Entity<ProductPriceSchedule>(entity =>
    {
      entity.ToTable("product_price_schedules");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.ProductCode).HasColumnName("product_code").HasMaxLength(50).IsRequired();
      entity.Property(e => e.CountryCode).HasColumnName("country_code").HasMaxLength(2).IsRequired();
      entity.Property(e => e.TierCode).HasColumnName("tier_code").HasMaxLength(20).IsRequired();
      entity.Property(e => e.ScheduleType).HasColumnName("schedule_type").HasMaxLength(20).IsRequired();
      entity.Property(e => e.ValidFrom).HasColumnName("valid_from");
      entity.Property(e => e.ValidTo).HasColumnName("valid_to");
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100).IsRequired();
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");

      entity.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductCode)
            .HasPrincipalKey(e => e.ProductCode)
            .OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(e => e.Country)
            .WithMany()
            .HasForeignKey(e => e.CountryCode)
            .OnDelete(DeleteBehavior.Restrict);

      entity.HasOne(e => e.PriceTier)
            .WithMany()
            .HasForeignKey(e => e.TierCode)
            .HasPrincipalKey(e => e.TierCode)
            .OnDelete(DeleteBehavior.Restrict);
    });

    // ProductPriceHistory
    modelBuilder.Entity<ProductPriceHistory>(entity =>
    {
      entity.ToTable("product_price_history");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.ProductCode).HasColumnName("product_code").HasMaxLength(50).IsRequired();
      entity.Property(e => e.CountryCode).HasColumnName("country_code").HasMaxLength(2).IsRequired();
      entity.Property(e => e.TierCode).HasColumnName("tier_code").HasMaxLength(20).IsRequired();
      entity.Property(e => e.ChangeType).HasColumnName("change_type").HasMaxLength(20).IsRequired();
      entity.Property(e => e.ChangedFrom).HasColumnName("changed_from").HasPrecision(10, 2);
      entity.Property(e => e.ChangedTo).HasColumnName("changed_to").HasPrecision(10, 2);
      entity.Property(e => e.ChangedBy).HasColumnName("changed_by").HasMaxLength(100).IsRequired();
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
    });

    // ProductSection
    modelBuilder.Entity<ProductSection>(entity =>
    {
      entity.ToTable("product_sections");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.ProductCode).HasColumnName("product_code").HasMaxLength(50).IsRequired();
      entity.Property(e => e.SectionCode).HasColumnName("section_code").HasMaxLength(50).IsRequired();
      entity.Property(e => e.SortOrder).HasColumnName("sort_order");
      entity.HasIndex(e => new { e.ProductCode, e.SectionCode }).IsUnique();

      entity.HasOne(e => e.ProductSectionType)
            .WithMany()
            .HasForeignKey(e => e.SectionCode)
            .HasPrincipalKey(e => e.SectionCode)
            .OnDelete(DeleteBehavior.Restrict);

      entity.HasMany(e => e.Translations)
            .WithOne(t => t.ProductSection)
            .HasForeignKey(t => t.ProductSectionId)
            .OnDelete(DeleteBehavior.Cascade);
    });

    // ProductSectionTranslation
    modelBuilder.Entity<ProductSectionTranslation>(entity =>
    {
      entity.ToTable("product_section_translations");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.ProductSectionId).HasColumnName("product_section_id");
      entity.Property(e => e.LanguageCode).HasColumnName("language_code").HasMaxLength(10).IsRequired();
      entity.Property(e => e.Content).HasColumnName("content").IsRequired();
      entity.HasIndex(e => new { e.ProductSectionId, e.LanguageCode }).IsUnique();
    });

    // ProductImage
    modelBuilder.Entity<ProductImage>(entity =>
    {
      entity.ToTable("product_images");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.ProductCode).HasColumnName("product_code").HasMaxLength(50).IsRequired();
      entity.Property(e => e.CountryCode).HasColumnName("country_code").HasMaxLength(10).IsRequired();
      entity.Property(e => e.LanguageCode).HasColumnName("language_code").HasMaxLength(10).IsRequired();
      entity.Property(e => e.ImageFilename).HasColumnName("image_filename").HasMaxLength(200).IsRequired();
      entity.Property(e => e.SortOrder).HasColumnName("sort_order");
      entity.Property(e => e.IsPrimary).HasColumnName("is_primary");
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");

      entity.HasOne(e => e.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(e => e.ProductCode)
            .HasPrincipalKey(p => p.ProductCode)
            .OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(e => e.Country)
            .WithMany()
            .HasForeignKey(e => e.CountryCode)
            .HasPrincipalKey(c => c.CountryCode)
            .OnDelete(DeleteBehavior.Restrict);
    });

    // ProductPackageItem
    modelBuilder.Entity<ProductPackageItem>(entity =>
    {
      entity.ToTable("product_package_items");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.PackageProductCode).HasColumnName("package_product_code").HasMaxLength(50).IsRequired();
      entity.Property(e => e.ItemProductCode).HasColumnName("item_product_code").HasMaxLength(50).IsRequired();
      entity.Property(e => e.Quantity).HasColumnName("quantity");
      entity.Property(e => e.SortOrder).HasColumnName("sort_order");
      entity.HasIndex(e => new { e.PackageProductCode, e.ItemProductCode }).IsUnique();

      entity.HasOne(e => e.ItemProduct)
            .WithMany()
            .HasForeignKey(e => e.ItemProductCode)
            .HasPrincipalKey(e => e.ProductCode)
            .OnDelete(DeleteBehavior.Restrict);
    });

    // Rank
    modelBuilder.Entity<Rank>(entity =>
    {
      entity.ToTable("ranks");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.RankCode).HasColumnName("rank_code").HasMaxLength(20).IsRequired();
      entity.Property(e => e.RankName).HasColumnName("rank_name").HasMaxLength(100).IsRequired();
      entity.Property(e => e.SortOrder).HasColumnName("sort_order");
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
      entity.Property(e => e.CreatedAt).HasColumnName("created_at");
      entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
      entity.HasIndex(e => e.RankCode).IsUnique();
    });

    // Member
    modelBuilder.Entity<Member>(entity =>
    {
      entity.ToTable("members");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.Username).HasColumnName("username").HasMaxLength(50).IsRequired();
      entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(200).IsRequired();
      entity.Property(e => e.IdType).HasColumnName("id_type").HasMaxLength(20).IsRequired();
      entity.Property(e => e.IdNo).HasColumnName("id_no").HasMaxLength(50).IsRequired();
      entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(200).IsRequired();
      entity.Property(e => e.PhoneCountryCode).HasColumnName("phone_country_code").HasMaxLength(10).IsRequired();
      entity.Property(e => e.PhoneNumber).HasColumnName("phone_number").HasMaxLength(20).IsRequired();
      entity.Property(e => e.ProfileImage).HasColumnName("profile_image").HasMaxLength(200);
      entity.Property(e => e.AddressLine1).HasColumnName("address_line1").HasMaxLength(200).IsRequired();
      entity.Property(e => e.AddressLine2).HasColumnName("address_line2").HasMaxLength(200);
      entity.Property(e => e.City).HasColumnName("city").HasMaxLength(100).IsRequired();
      entity.Property(e => e.State).HasColumnName("state").HasMaxLength(100).IsRequired();
      entity.Property(e => e.Postcode).HasColumnName("postcode").HasMaxLength(20).IsRequired();
      entity.Property(e => e.CountryCode).HasColumnName("country_code").HasMaxLength(10).IsRequired();
      entity.Property(e => e.BankName).HasColumnName("bank_name").HasMaxLength(100);
      entity.Property(e => e.BankAccountName).HasColumnName("bank_account_name").HasMaxLength(200);
      entity.Property(e => e.BankAccountNo).HasColumnName("bank_account_no").HasMaxLength(50);
      entity.Property(e => e.SponsorId).HasColumnName("sponsor_id");
      entity.Property(e => e.BinaryParentId).HasColumnName("binary_parent_id");
      entity.Property(e => e.BinaryPosition).HasColumnName("binary_position").HasMaxLength(5);
      entity.Property(e => e.IsActivated).HasColumnName("is_activated");
      entity.Property(e => e.ActivatedAt).HasColumnName("activated_at");
      entity.Property(e => e.JoinedAt).HasColumnName("joined_at");
      entity.Property(e => e.CurrentRankCode).HasColumnName("current_rank_code").HasMaxLength(20);
      entity.Property(e => e.HighestRankCode).HasColumnName("highest_rank_code").HasMaxLength(20);
      entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(500).IsRequired();
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
      entity.Property(e => e.CreatedAt).HasColumnName("created_at")
            .HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100).IsRequired();
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at")
            .HasDefaultValueSql("now() AT TIME ZONE 'utc'");

      entity.HasIndex(e => e.Username).IsUnique();

      entity.HasOne(e => e.Country)
            .WithMany()
            .HasForeignKey(e => e.CountryCode)
            .HasPrincipalKey(e => e.CountryCode)
            .OnDelete(DeleteBehavior.Restrict);

      entity.HasOne(e => e.Sponsor)
            .WithMany()
            .HasForeignKey(e => e.SponsorId)
            .OnDelete(DeleteBehavior.Restrict);

      entity.HasOne(e => e.BinaryParent)
            .WithMany()
            .HasForeignKey(e => e.BinaryParentId)
            .OnDelete(DeleteBehavior.Restrict);
    });

    // MemberRankHistory
    modelBuilder.Entity<MemberRankHistory>(entity =>
    {
      entity.ToTable("member_rank_histories");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.MemberId).HasColumnName("member_id").IsRequired();
      entity.Property(e => e.RankCode).HasColumnName("rank_code").HasMaxLength(20).IsRequired();
      entity.Property(e => e.Pv).HasColumnName("pv").HasColumnType("numeric(10,2)");
      entity.Property(e => e.PeriodYear).HasColumnName("period_year");
      entity.Property(e => e.PeriodMonth).HasColumnName("period_month");
      entity.Property(e => e.CreatedAt).HasColumnName("created_at")
            .HasDefaultValueSql("now() AT TIME ZONE 'utc'");

      entity.HasOne(e => e.Member)
            .WithMany()
            .HasForeignKey(e => e.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    });

    // MemberChangeLog
    modelBuilder.Entity<MemberChangeLog>(entity =>
    {
      entity.ToTable("member_change_logs");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.MemberId).HasColumnName("member_id").IsRequired();
      entity.Property(e => e.ChangeType).HasColumnName("change_type").HasMaxLength(50).IsRequired();
      entity.Property(e => e.OldValue).HasColumnName("old_value").IsRequired();
      entity.Property(e => e.NewValue).HasColumnName("new_value").IsRequired();
      entity.Property(e => e.ChangedBy).HasColumnName("changed_by").HasMaxLength(100).IsRequired();
      entity.Property(e => e.ChangedAt).HasColumnName("changed_at")
            .HasDefaultValueSql("now() AT TIME ZONE 'utc'");

      entity.HasOne(e => e.Member)
            .WithMany()
            .HasForeignKey(e => e.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    });

    // ExchangeRate
    modelBuilder.Entity<ExchangeRate>(entity =>
    {
      entity.ToTable("exchange_rates");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.CurrencyCode).HasColumnName("currency_code").HasMaxLength(10).IsRequired();
      entity.Property(e => e.RateToBase).HasColumnName("rate_to_base").HasColumnType("numeric(10,6)");
      entity.Property(e => e.EffectiveDatetime).HasColumnName("effective_datetime");
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
      entity.Property(e => e.CreatedAt).HasColumnName("created_at")
            .HasDefaultValueSql("now() AT TIME ZONE 'utc'");
    });

    // WalletBalance
    modelBuilder.Entity<WalletBalance>(entity =>
    {
      entity.ToTable("wallet_balances");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.MemberId).HasColumnName("member_id").IsRequired();
      entity.Property(e => e.WalletType).HasColumnName("wallet_type").HasMaxLength(20).IsRequired();
      entity.Property(e => e.Balance).HasColumnName("balance").HasColumnType("numeric(10,2)");
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
      entity.HasIndex(e => new { e.MemberId, e.WalletType }).IsUnique();
      entity.HasOne(e => e.Member)
            .WithMany()
            .HasForeignKey(e => e.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    });

    // CashWalletTransaction
    modelBuilder.Entity<CashWalletTransaction>(entity =>
    {
      entity.ToTable("cash_wallet_transactions");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.MemberId).HasColumnName("member_id").IsRequired();
      entity.Property(e => e.TxnType).HasColumnName("txn_type").HasMaxLength(50).IsRequired();
      entity.Property(e => e.AmountUsd).HasColumnName("amount_usd").HasColumnType("numeric(10,2)");
      entity.Property(e => e.Direction).HasColumnName("direction").HasMaxLength(5).IsRequired();
      entity.Property(e => e.BalanceAfter).HasColumnName("balance_after").HasColumnType("numeric(10,2)");
      entity.Property(e => e.DisplayAmount).HasColumnName("display_amount").HasColumnType("numeric(10,2)");
      entity.Property(e => e.DisplayCurrency).HasColumnName("display_currency").HasMaxLength(10).IsRequired();
      entity.Property(e => e.ExchangeRate).HasColumnName("exchange_rate").HasColumnType("numeric(10,6)");
      entity.Property(e => e.ReferenceId).HasColumnName("reference_id").HasMaxLength(100);
      entity.Property(e => e.Remark).HasColumnName("remark");
      entity.Property(e => e.IncentivePeriodId).HasColumnName("incentive_period_id");
      entity.Property(e => e.PeriodDate).HasColumnName("period_date");
      entity.Property(e => e.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(36);
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
      entity.Property(e => e.CreatedAt).HasColumnName("created_at")
            .HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.HasIndex(e => e.IdempotencyKey).IsUnique()
            .HasFilter("idempotency_key IS NOT NULL");
      entity.HasOne(e => e.Member)
            .WithMany()
            .HasForeignKey(e => e.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    });

    // CashWalletTransactionArchive
    modelBuilder.Entity<CashWalletTransactionArchive>(entity =>
    {
      entity.ToTable("cash_wallet_transactions_archive");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
      entity.Property(e => e.MemberId).HasColumnName("member_id").IsRequired();
      entity.Property(e => e.TxnType).HasColumnName("txn_type").HasMaxLength(50).IsRequired();
      entity.Property(e => e.AmountUsd).HasColumnName("amount_usd").HasColumnType("numeric(10,2)");
      entity.Property(e => e.Direction).HasColumnName("direction").HasMaxLength(5).IsRequired();
      entity.Property(e => e.BalanceAfter).HasColumnName("balance_after").HasColumnType("numeric(10,2)");
      entity.Property(e => e.DisplayAmount).HasColumnName("display_amount").HasColumnType("numeric(10,2)");
      entity.Property(e => e.DisplayCurrency).HasColumnName("display_currency").HasMaxLength(10).IsRequired();
      entity.Property(e => e.ExchangeRate).HasColumnName("exchange_rate").HasColumnType("numeric(10,6)");
      entity.Property(e => e.ReferenceId).HasColumnName("reference_id").HasMaxLength(100);
      entity.Property(e => e.Remark).HasColumnName("remark");
      entity.Property(e => e.IncentivePeriodId).HasColumnName("incentive_period_id");
      entity.Property(e => e.PeriodDate).HasColumnName("period_date");
      entity.Property(e => e.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(36);
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
      entity.Property(e => e.CreatedAt).HasColumnName("created_at");
    });

    // PurchaseWalletTransaction
    modelBuilder.Entity<PurchaseWalletTransaction>(entity =>
    {
      entity.ToTable("purchase_wallet_transactions");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.MemberId).HasColumnName("member_id").IsRequired();
      entity.Property(e => e.TxnType).HasColumnName("txn_type").HasMaxLength(50).IsRequired();
      entity.Property(e => e.AmountUsd).HasColumnName("amount_usd").HasColumnType("numeric(10,2)");
      entity.Property(e => e.Direction).HasColumnName("direction").HasMaxLength(5).IsRequired();
      entity.Property(e => e.BalanceAfter).HasColumnName("balance_after").HasColumnType("numeric(10,2)");
      entity.Property(e => e.DisplayAmount).HasColumnName("display_amount").HasColumnType("numeric(10,2)");
      entity.Property(e => e.DisplayCurrency).HasColumnName("display_currency").HasMaxLength(10).IsRequired();
      entity.Property(e => e.ExchangeRate).HasColumnName("exchange_rate").HasColumnType("numeric(10,6)");
      entity.Property(e => e.ReferenceId).HasColumnName("reference_id").HasMaxLength(100);
      entity.Property(e => e.Remark).HasColumnName("remark");
      entity.Property(e => e.IncentivePeriodId).HasColumnName("incentive_period_id");
      entity.Property(e => e.PeriodDate).HasColumnName("period_date");
      entity.Property(e => e.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(36);
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
      entity.Property(e => e.CreatedAt).HasColumnName("created_at")
            .HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.HasIndex(e => e.IdempotencyKey).IsUnique()
            .HasFilter("idempotency_key IS NOT NULL");
      entity.HasOne(e => e.Member)
            .WithMany()
            .HasForeignKey(e => e.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    });

    // PurchaseWalletTransactionArchive
    modelBuilder.Entity<PurchaseWalletTransactionArchive>(entity =>
    {
      entity.ToTable("purchase_wallet_transactions_archive");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
      entity.Property(e => e.MemberId).HasColumnName("member_id").IsRequired();
      entity.Property(e => e.TxnType).HasColumnName("txn_type").HasMaxLength(50).IsRequired();
      entity.Property(e => e.AmountUsd).HasColumnName("amount_usd").HasColumnType("numeric(10,2)");
      entity.Property(e => e.Direction).HasColumnName("direction").HasMaxLength(5).IsRequired();
      entity.Property(e => e.BalanceAfter).HasColumnName("balance_after").HasColumnType("numeric(10,2)");
      entity.Property(e => e.DisplayAmount).HasColumnName("display_amount").HasColumnType("numeric(10,2)");
      entity.Property(e => e.DisplayCurrency).HasColumnName("display_currency").HasMaxLength(10).IsRequired();
      entity.Property(e => e.ExchangeRate).HasColumnName("exchange_rate").HasColumnType("numeric(10,6)");
      entity.Property(e => e.ReferenceId).HasColumnName("reference_id").HasMaxLength(100);
      entity.Property(e => e.Remark).HasColumnName("remark");
      entity.Property(e => e.IncentivePeriodId).HasColumnName("incentive_period_id");
      entity.Property(e => e.PeriodDate).HasColumnName("period_date");
      entity.Property(e => e.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(36);
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
      entity.Property(e => e.CreatedAt).HasColumnName("created_at");
    });

    // IncentivePeriod
    modelBuilder.Entity<IncentivePeriod>(entity =>
    {
      entity.ToTable("incentive_periods");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.PeriodDate).HasColumnName("period_date");
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
      entity.Property(e => e.ClosedAt).HasColumnName("closed_at");
      entity.Property(e => e.ProcessedAt).HasColumnName("processed_at");
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
      entity.Property(e => e.CreatedAt).HasColumnName("created_at")
            .HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100).IsRequired();
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at")
            .HasDefaultValueSql("now() AT TIME ZONE 'utc'");

      entity.HasIndex(e => e.PeriodDate).IsUnique();

      entity.HasMany(e => e.Payouts)
            .WithOne(p => p.IncentivePeriod)
            .HasForeignKey(p => p.IncentivePeriodId)
            .OnDelete(DeleteBehavior.Restrict);
    });

    // WalletPayout
    modelBuilder.Entity<WalletPayout>(entity =>
    {
      entity.ToTable("wallet_payouts");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.IncentivePeriodId).HasColumnName("incentive_period_id").IsRequired();
      entity.Property(e => e.PeriodDate).HasColumnName("period_date");
      entity.Property(e => e.MemberId).HasColumnName("member_id").IsRequired();
      entity.Property(e => e.IncentiveType).HasColumnName("incentive_type").HasMaxLength(50).IsRequired();
      entity.Property(e => e.PvAmount).HasColumnName("pv_amount").HasColumnType("numeric(10,2)");
      entity.Property(e => e.AmountUsd).HasColumnName("amount_usd").HasColumnType("numeric(10,2)");
      entity.Property(e => e.ReferenceId).HasColumnName("reference_id").HasMaxLength(100);
      entity.Property(e => e.Remark).HasColumnName("remark");
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
      entity.Property(e => e.RetryCount).HasColumnName("retry_count");
      entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
      entity.Property(e => e.ProcessedAt).HasColumnName("processed_at");
      entity.Property(e => e.CreatedAt).HasColumnName("created_at")
            .HasDefaultValueSql("now() AT TIME ZONE 'utc'");

      entity.HasOne(e => e.Member)
            .WithMany()
            .HasForeignKey(e => e.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    });

    // SystemSetting
    modelBuilder.Entity<SystemSetting>(entity =>
    {
      entity.ToTable("system_settings");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.SettingKey).HasColumnName("setting_key").HasMaxLength(100).IsRequired();
      entity.Property(e => e.SettingValue).HasColumnName("setting_value").IsRequired();
      entity.Property(e => e.KeyType).HasColumnName("key_type").HasMaxLength(20).IsRequired();
      entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500).IsRequired();
      entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100).IsRequired();
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at")
            .HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.HasIndex(e => e.SettingKey).IsUnique();
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
