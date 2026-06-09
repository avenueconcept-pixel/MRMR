using Microsoft.EntityFrameworkCore;
using MyApp.Models;
using MyApp.Models.MRMR;

namespace MyApp.Data;

public class AppDbContext : DbContext
{
  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

  public DbSet<LanguageResource> LanguageResources => Set<LanguageResource>();
  public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
  public DbSet<Applicant> Applicants => Set<Applicant>();
  public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
  public DbSet<Language> Languages => Set<Language>();
  public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
  public DbSet<AppLog> AppLogs => Set<AppLog>();
  public DbSet<Country> Countries => Set<Country>();
  public DbSet<CountryTranslation> CountryTranslations => Set<CountryTranslation>();
  public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
  public DbSet<PaymentMethodTranslation> PaymentMethodTranslations => Set<PaymentMethodTranslation>();
  public DbSet<State>            States            => Set<State>();
  public DbSet<StateTranslation> StateTranslations => Set<StateTranslation>();
  public DbSet<Role>             Roles             => Set<Role>();
  public DbSet<RoleMenu>         RoleMenus         => Set<RoleMenu>();
  public DbSet<RolePermission>   RolePermissions   => Set<RolePermission>();
  public DbSet<Menu>             Menus             => Set<Menu>();
  public DbSet<Permission>       Permissions       => Set<Permission>();
  public DbSet<Bank>             Banks             => Set<Bank>();
  public DbSet<BankTranslation>  BankTranslations  => Set<BankTranslation>();
  public DbSet<Announcement>            Announcements            => Set<Announcement>();
  public DbSet<AnnouncementTranslation> AnnouncementTranslations => Set<AnnouncementTranslation>();
  public DbSet<AnnouncementAttachment>  AnnouncementAttachments  => Set<AnnouncementAttachment>();
  public DbSet<AppSystem>                  AppSystems                  => Set<AppSystem>();
  public DbSet<MaintenanceSchedule>        MaintenanceSchedules        => Set<MaintenanceSchedule>();
  public DbSet<MaintenanceScheduleSystem>  MaintenanceScheduleSystems  => Set<MaintenanceScheduleSystem>();
  public DbSet<MaintenanceScheduleMessage> MaintenanceScheduleMessages => Set<MaintenanceScheduleMessage>();
  public DbSet<AppSetting>                 AppSettings                 => Set<AppSetting>();

  // MRMR2026
  public DbSet<Registrant>              Registrants              => Set<Registrant>();
  public DbSet<Application>             Applications             => Set<Application>();
  public DbSet<Payment>                 Payments                 => Set<Payment>();
  public DbSet<PaymentAuditLog>         PaymentAuditLogs         => Set<PaymentAuditLog>();
  public DbSet<AwardCategory>           AwardCategories          => Set<AwardCategory>();
  public DbSet<AwardCriterion>          AwardCriteria            => Set<AwardCriterion>();
  public DbSet<JudgeCategoryAssignment> JudgeCategoryAssignments => Set<JudgeCategoryAssignment>();
  public DbSet<ApplicationSubmission>   ApplicationSubmissions   => Set<ApplicationSubmission>();
  public DbSet<SubmissionSectionA>      SubmissionSectionsA      => Set<SubmissionSectionA>();
  public DbSet<SubmissionSectionB>      SubmissionSectionsB      => Set<SubmissionSectionB>();
  public DbSet<SubmissionSectionJsonb>  SubmissionSectionsJsonb  => Set<SubmissionSectionJsonb>();
  public DbSet<ApplicationDocument>     ApplicationDocuments     => Set<ApplicationDocument>();
  public DbSet<JudgeEvaluation>         JudgeEvaluations         => Set<JudgeEvaluation>();
  public DbSet<JudgeScore>              JudgeScores              => Set<JudgeScore>();
  public DbSet<ApplicationRanking>      ApplicationRankings      => Set<ApplicationRanking>();
  public DbSet<SystemSetting>           SystemSettings           => Set<SystemSetting>();

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

    // MRMR2026
    modelBuilder.Entity<Registrant>(entity =>
    {
      entity.ToTable("registrants");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(20).IsRequired();
      entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(200).IsRequired();
      entity.Property(e => e.NricPassport).HasColumnName("nric_passport").HasMaxLength(50).IsRequired();
      entity.Property(e => e.ContactNo).HasColumnName("contact_no").HasMaxLength(30).IsRequired();
      entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
      entity.Property(e => e.CompanyName).HasColumnName("company_name").HasMaxLength(200);
      entity.Property(e => e.SsmRegNo).HasColumnName("ssm_reg_no").HasMaxLength(50);
      entity.Property(e => e.CompanyAddress).HasColumnName("company_address");
      entity.Property(e => e.Website).HasColumnName("website").HasMaxLength(200);
      entity.Property(e => e.Industry).HasColumnName("industry").HasMaxLength(50);
      entity.Property(e => e.BusinessNature).HasColumnName("business_nature");
      entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
      entity.Property(e => e.IsFirstLogin).HasColumnName("is_first_login").HasDefaultValue(true);
      entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(false);
      entity.Property(e => e.DeclInfoAccurate).HasColumnName("decl_info_accurate").HasDefaultValue(false);
      entity.Property(e => e.DeclFeeNonrefundable).HasColumnName("decl_fee_nonrefundable").HasDefaultValue(false);
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.HasIndex(e => e.Email).IsUnique();
      entity.HasIndex(e => e.NricPassport).IsUnique();
    });

    modelBuilder.Entity<Application>(entity =>
    {
      entity.ToTable("applications");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.ApplicationId).HasColumnName("application_id").HasMaxLength(25).IsRequired();
      entity.Property(e => e.RegistrantId).HasColumnName("registrant_id").IsRequired();
      entity.Property(e => e.ApplicationType).HasColumnName("application_type").HasMaxLength(20).IsRequired();
      entity.Property(e => e.AwardCategoryId).HasColumnName("award_category_id").IsRequired();
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
      entity.Property(e => e.PaymentMethod).HasColumnName("payment_method").HasMaxLength(25).IsRequired();
      entity.Property(e => e.Username).HasColumnName("username").HasMaxLength(25);
      entity.Property(e => e.IsFinalSubmitted).HasColumnName("is_final_submitted").HasDefaultValue(false);
      entity.Property(e => e.SubmittedAt).HasColumnName("submitted_at");
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.HasIndex(e => e.ApplicationId).IsUnique();
      entity.HasIndex(e => new { e.RegistrantId, e.ApplicationType }).IsUnique();
      entity.HasOne(e => e.Registrant).WithMany(r => r.Applications).HasForeignKey(e => e.RegistrantId).OnDelete(DeleteBehavior.Restrict);
      entity.HasOne(e => e.AwardCategory).WithMany(c => c.Applications).HasForeignKey(e => e.AwardCategoryId).OnDelete(DeleteBehavior.Restrict);
    });

    modelBuilder.Entity<Payment>(entity =>
    {
      entity.ToTable("payments");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.ApplicationId).HasColumnName("application_id").IsRequired();
      entity.Property(e => e.PaymentType).HasColumnName("payment_type").HasMaxLength(20).IsRequired();
      entity.Property(e => e.Amount).HasColumnName("amount").HasPrecision(12, 2).IsRequired();
      entity.Property(e => e.Method).HasColumnName("method").HasMaxLength(25).IsRequired();
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
      entity.Property(e => e.SlipFilePath).HasColumnName("slip_file_path");
      entity.Property(e => e.SlipUploadedAt).HasColumnName("slip_uploaded_at");
      entity.Property(e => e.AxaipayRefNo).HasColumnName("axaipay_ref_no").HasMaxLength(100);
      entity.Property(e => e.AxaipayPayload).HasColumnName("axaipay_payload").HasColumnType("jsonb");
      entity.Property(e => e.VerifiedBy).HasColumnName("verified_by");
      entity.Property(e => e.VerifiedAt).HasColumnName("verified_at");
      entity.Property(e => e.AdminRemarks).HasColumnName("admin_remarks");
      entity.Property(e => e.InvoiceNo).HasColumnName("invoice_no").HasMaxLength(30);
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.HasIndex(e => new { e.ApplicationId, e.PaymentType }).IsUnique();
      entity.HasIndex(e => e.Status);
      entity.HasOne(e => e.Application).WithMany(a => a.Payments).HasForeignKey(e => e.ApplicationId).OnDelete(DeleteBehavior.Restrict);
    });

    modelBuilder.Entity<PaymentAuditLog>(entity =>
    {
      entity.ToTable("payment_audit_logs");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.PaymentId).HasColumnName("payment_id").IsRequired();
      entity.Property(e => e.Action).HasColumnName("action").HasMaxLength(50).IsRequired();
      entity.Property(e => e.PerformedBy).HasColumnName("performed_by");
      entity.Property(e => e.PerformedAt).HasColumnName("performed_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.Remarks).HasColumnName("remarks");
      entity.Property(e => e.Snapshot).HasColumnName("snapshot").HasColumnType("jsonb");
      entity.HasIndex(e => e.PaymentId);
      entity.HasOne(e => e.Payment).WithMany(p => p.AuditLogs).HasForeignKey(e => e.PaymentId).OnDelete(DeleteBehavior.Restrict);
    });

    modelBuilder.Entity<AwardCategory>(entity =>
    {
      entity.ToTable("award_categories");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
      entity.Property(e => e.CategoryType).HasColumnName("category_type").HasMaxLength(20).IsRequired();
      entity.Property(e => e.Price).HasColumnName("price").HasPrecision(12, 2).IsRequired();
      entity.Property(e => e.MaxRecipients).HasColumnName("max_recipients").HasDefaultValue((short)1);
      entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(false);
      entity.Property(e => e.DisplayOrder).HasColumnName("display_order").HasDefaultValue((short)0);
      entity.Property(e => e.Description).HasColumnName("description");
      entity.Property(e => e.CriteriaLocked).HasColumnName("criteria_locked").HasDefaultValue(false);
      entity.Property(e => e.CreatedBy).HasColumnName("created_by").IsRequired();
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
    });

    modelBuilder.Entity<AwardCriterion>(entity =>
    {
      entity.ToTable("award_criteria");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.AwardCategoryId).HasColumnName("award_category_id").IsRequired();
      entity.Property(e => e.CriterionName).HasColumnName("criterion_name").HasMaxLength(200).IsRequired();
      entity.Property(e => e.Weight).HasColumnName("weight").HasPrecision(5, 2).IsRequired();
      entity.Property(e => e.DisplayOrder).HasColumnName("display_order").HasDefaultValue((short)0);
      entity.Property(e => e.Description).HasColumnName("description");
      entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.HasIndex(e => e.AwardCategoryId);
      entity.HasOne(e => e.AwardCategory).WithMany(c => c.Criteria).HasForeignKey(e => e.AwardCategoryId).OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<JudgeCategoryAssignment>(entity =>
    {
      entity.ToTable("judge_category_assignments");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.JudgeId).HasColumnName("judge_id").IsRequired();
      entity.Property(e => e.AwardCategoryId).HasColumnName("award_category_id").IsRequired();
      entity.Property(e => e.AssignedBy).HasColumnName("assigned_by").IsRequired();
      entity.Property(e => e.AssignedAt).HasColumnName("assigned_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
      entity.HasIndex(e => new { e.JudgeId, e.AwardCategoryId }).IsUnique();
      entity.HasIndex(e => e.AwardCategoryId);
      entity.HasOne(e => e.AwardCategory).WithMany(c => c.JudgeAssignments).HasForeignKey(e => e.AwardCategoryId).OnDelete(DeleteBehavior.Restrict);
    });

    modelBuilder.Entity<ApplicationSubmission>(entity =>
    {
      entity.ToTable("application_submissions");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.ApplicationId).HasColumnName("application_id").IsRequired();
      entity.Property(e => e.SectionAComplete).HasColumnName("section_a_complete").HasDefaultValue(false);
      entity.Property(e => e.SectionBComplete).HasColumnName("section_b_complete").HasDefaultValue(false);
      entity.Property(e => e.SectionCComplete).HasColumnName("section_c_complete").HasDefaultValue(false);
      entity.Property(e => e.SectionDComplete).HasColumnName("section_d_complete").HasDefaultValue(false);
      entity.Property(e => e.SectionEComplete).HasColumnName("section_e_complete").HasDefaultValue(false);
      entity.Property(e => e.SectionFComplete).HasColumnName("section_f_complete").HasDefaultValue(false);
      entity.Property(e => e.SectionGComplete).HasColumnName("section_g_complete").HasDefaultValue(false);
      entity.Property(e => e.SectionHComplete).HasColumnName("section_h_complete").HasDefaultValue(false);
      entity.Property(e => e.SectionIComplete).HasColumnName("section_i_complete").HasDefaultValue(false);
      entity.Property(e => e.SectionJComplete).HasColumnName("section_j_complete").HasDefaultValue(false);
      entity.Property(e => e.SectionKComplete).HasColumnName("section_k_complete").HasDefaultValue(false);
      entity.Property(e => e.SectionLComplete).HasColumnName("section_l_complete").HasDefaultValue(false);
      entity.Property(e => e.IsFinalSubmitted).HasColumnName("is_final_submitted").HasDefaultValue(false);
      entity.Property(e => e.LastSavedAt).HasColumnName("last_saved_at");
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.HasIndex(e => e.ApplicationId).IsUnique();
      entity.HasOne(e => e.Application).WithOne(a => a.Submission).HasForeignKey<ApplicationSubmission>(e => e.ApplicationId).OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<SubmissionSectionA>(entity =>
    {
      entity.ToTable("submission_section_a");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.ApplicationId).HasColumnName("application_id").IsRequired();
      entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(20);
      entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(200);
      entity.Property(e => e.NricPassport).HasColumnName("nric_passport").HasMaxLength(50);
      entity.Property(e => e.ContactNo).HasColumnName("contact_no").HasMaxLength(30);
      entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(150);
      entity.Property(e => e.AddressLine1).HasColumnName("address_line1").HasMaxLength(200);
      entity.Property(e => e.AddressLine2).HasColumnName("address_line2").HasMaxLength(200);
      entity.Property(e => e.City).HasColumnName("city").HasMaxLength(100);
      entity.Property(e => e.State).HasColumnName("state").HasMaxLength(100);
      entity.Property(e => e.Postcode).HasColumnName("postcode").HasMaxLength(10);
      entity.Property(e => e.Country).HasColumnName("country").HasMaxLength(100);
      entity.Property(e => e.MembershipNo).HasColumnName("membership_no").HasMaxLength(50);
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.HasIndex(e => e.ApplicationId).IsUnique();
      entity.HasOne(e => e.Application).WithOne(a => a.SectionA).HasForeignKey<SubmissionSectionA>(e => e.ApplicationId).OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<SubmissionSectionB>(entity =>
    {
      entity.ToTable("submission_section_b");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.ApplicationId).HasColumnName("application_id").IsRequired();
      entity.Property(e => e.CompanyName).HasColumnName("company_name").HasMaxLength(200);
      entity.Property(e => e.SsmRegNo).HasColumnName("ssm_reg_no").HasMaxLength(50);
      entity.Property(e => e.IncorporationDate).HasColumnName("incorporation_date");
      entity.Property(e => e.ContactNo).HasColumnName("contact_no").HasMaxLength(30);
      entity.Property(e => e.AddressLine1).HasColumnName("address_line1").HasMaxLength(200);
      entity.Property(e => e.AddressLine2).HasColumnName("address_line2").HasMaxLength(200);
      entity.Property(e => e.City).HasColumnName("city").HasMaxLength(100);
      entity.Property(e => e.State).HasColumnName("state").HasMaxLength(100);
      entity.Property(e => e.Postcode).HasColumnName("postcode").HasMaxLength(10);
      entity.Property(e => e.Country).HasColumnName("country").HasMaxLength(100);
      entity.Property(e => e.Website).HasColumnName("website").HasMaxLength(200);
      entity.Property(e => e.Industry).HasColumnName("industry").HasMaxLength(50);
      entity.Property(e => e.BusinessNature).HasColumnName("business_nature");
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.HasIndex(e => e.ApplicationId).IsUnique();
      entity.HasOne(e => e.Application).WithOne(a => a.SectionB).HasForeignKey<SubmissionSectionB>(e => e.ApplicationId).OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<SubmissionSectionJsonb>(entity =>
    {
      entity.ToTable("submission_sections_jsonb");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.ApplicationId).HasColumnName("application_id").IsRequired();
      entity.Property(e => e.SectionCode).HasColumnName("section_code").HasMaxLength(2).IsRequired();
      entity.Property(e => e.SectionData).HasColumnName("section_data").HasColumnType("jsonb");
      entity.Property(e => e.IsComplete).HasColumnName("is_complete").HasDefaultValue(false);
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.HasIndex(e => new { e.ApplicationId, e.SectionCode }).IsUnique();
      entity.HasOne(e => e.Application).WithMany(a => a.SectionJsonbs).HasForeignKey(e => e.ApplicationId).OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<ApplicationDocument>(entity =>
    {
      entity.ToTable("application_documents");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.ApplicationId).HasColumnName("application_id").IsRequired();
      entity.Property(e => e.DocumentType).HasColumnName("document_type").HasMaxLength(50).IsRequired();
      entity.Property(e => e.OriginalFilename).HasColumnName("original_filename").HasMaxLength(255).IsRequired();
      entity.Property(e => e.FilePath).HasColumnName("file_path").IsRequired();
      entity.Property(e => e.FileSizeBytes).HasColumnName("file_size_bytes");
      entity.Property(e => e.MimeType).HasColumnName("mime_type").HasMaxLength(100);
      entity.Property(e => e.IsRequired).HasColumnName("is_required").HasDefaultValue(false);
      entity.Property(e => e.VerificationStatus).HasColumnName("verification_status").HasMaxLength(20).IsRequired();
      entity.Property(e => e.AdminRemarks).HasColumnName("admin_remarks");
      entity.Property(e => e.VerifiedBy).HasColumnName("verified_by");
      entity.Property(e => e.VerifiedAt).HasColumnName("verified_at");
      entity.Property(e => e.UploadedAt).HasColumnName("uploaded_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.HasIndex(e => e.ApplicationId);
      entity.HasIndex(e => e.VerificationStatus);
      entity.HasOne(e => e.Application).WithMany(a => a.Documents).HasForeignKey(e => e.ApplicationId).OnDelete(DeleteBehavior.Restrict);
    });

    modelBuilder.Entity<JudgeEvaluation>(entity =>
    {
      entity.ToTable("judge_evaluations");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.ApplicationId).HasColumnName("application_id").IsRequired();
      entity.Property(e => e.JudgeId).HasColumnName("judge_id").IsRequired();
      entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
      entity.Property(e => e.OverallComment).HasColumnName("overall_comment");
      entity.Property(e => e.Recommendation).HasColumnName("recommendation").HasMaxLength(20);
      entity.Property(e => e.SubmittedAt).HasColumnName("submitted_at");
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.HasIndex(e => new { e.ApplicationId, e.JudgeId }).IsUnique();
      entity.HasIndex(e => e.Status);
      entity.HasOne(e => e.Application).WithMany(a => a.Evaluations).HasForeignKey(e => e.ApplicationId).OnDelete(DeleteBehavior.Restrict);
    });

    modelBuilder.Entity<JudgeScore>(entity =>
    {
      entity.ToTable("judge_scores");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.ApplicationId).HasColumnName("application_id").IsRequired();
      entity.Property(e => e.JudgeId).HasColumnName("judge_id").IsRequired();
      entity.Property(e => e.AwardCriterionId).HasColumnName("award_criterion_id").IsRequired();
      entity.Property(e => e.Score).HasColumnName("score").HasPrecision(5, 2).IsRequired();
      entity.Property(e => e.WeightedScore).HasColumnName("weighted_score").HasPrecision(8, 4);
      entity.Property(e => e.Comment).HasColumnName("comment");
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.HasIndex(e => new { e.ApplicationId, e.JudgeId, e.AwardCriterionId }).IsUnique();
      entity.HasOne(e => e.Application).WithMany().HasForeignKey(e => e.ApplicationId).OnDelete(DeleteBehavior.Restrict);
      entity.HasOne(e => e.AwardCriterion).WithMany(c => c.JudgeScores).HasForeignKey(e => e.AwardCriterionId).OnDelete(DeleteBehavior.Restrict);
    });

    modelBuilder.Entity<ApplicationRanking>(entity =>
    {
      entity.ToTable("application_rankings");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
      entity.Property(e => e.ApplicationId).HasColumnName("application_id").IsRequired();
      entity.Property(e => e.AwardCategoryId).HasColumnName("award_category_id").IsRequired();
      entity.Property(e => e.FinalScore).HasColumnName("final_score").HasPrecision(8, 4).IsRequired();
      entity.Property(e => e.RankPosition).HasColumnName("rank_position").IsRequired();
      entity.Property(e => e.IsRecommended).HasColumnName("is_recommended").HasDefaultValue(false);
      entity.Property(e => e.IsApprovedWinner).HasColumnName("is_approved_winner").HasDefaultValue(false);
      entity.Property(e => e.CommitteeRemarks).HasColumnName("committee_remarks");
      entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
      entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");
      entity.Property(e => e.RankedAt).HasColumnName("ranked_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
      entity.HasIndex(e => e.ApplicationId).IsUnique();
      entity.HasIndex(e => e.AwardCategoryId);
      entity.HasOne(e => e.Application).WithOne(a => a.Ranking).HasForeignKey<ApplicationRanking>(e => e.ApplicationId).OnDelete(DeleteBehavior.Restrict);
      entity.HasOne(e => e.AwardCategory).WithMany().HasForeignKey(e => e.AwardCategoryId).OnDelete(DeleteBehavior.Restrict);
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

    // Applicant
    modelBuilder.Entity<Applicant>(entity =>
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
