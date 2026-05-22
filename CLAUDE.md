# CLAUDE.md — Project Guidelines for MyApp (CRMCore)

## Technology Stack

- **Framework:** ASP.NET Core Razor Pages, .NET 10
- **Database:** PostgreSQL via `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.1
- **ORM:** Entity Framework Core 10.0.8 (code-first, Fluent API)
- **Auth:** Two named cookie schemes — `AuthSchemeConstants.Admin` (`"AdminCookie"`) for admin area, `AuthSchemeConstants.Customer` (`"CustomerCookie"`) for customer area — 30-day sliding expiration each
- **Email:** MailKit 4.16.0
- **i18n:** DB-backed `LanguageResource` table + `TranslationService` (memory-cached)
- **Frontend:** Bootstrap 5, jQuery, DataTables, ApexCharts, Select2 (vendor-bundled in `wwwroot/vendor/`)
- **Build:** Webpack + Gulp for JS/CSS bundling (`*.dist.js` / `*.dist.css` outputs)
- **Root namespace:** `MyApp`

---

## Architecture Pattern

This project uses **ASP.NET Core Razor Pages** — there are no MVC controllers.

```
PageModel (.cshtml.cs)
  └── DbHelper (Helper/DB/)      ← repository-style EF Core queries
        └── AppDbContext          ← EF Core DbContext
              └── PostgreSQL
  └── Service (Services/)        ← cross-cutting logic (email, translation)
```

- HTTP handling lives in `PageModel` classes (OnGet / OnPost handlers)
- Data access lives in `*DbHelper` classes, never directly in PageModels
- Business/cross-cutting logic lives in `Services/`
- Constants, enums, and app-wide values live in `Constants/`

---

## Folder Structure

```
Areas/
  Admin/Pages/          ← admin Razor Pages (.cshtml + .cshtml.cs)
    Layouts/            ← shared layout files (_CommonMasterLayout, etc.)
    _Partials/          ← reusable partial views
    Account/            ← account-specific pages (SetLanguage, etc.)
  Customer/Pages/       ← customer-facing Razor Pages
Constants/              ← static constant classes only
Data/
  AppDbContext.cs       ← single DbContext, all Fluent API config here
Dtos/                   ← data transfer objects, one file per domain
Helper/
  DB/                   ← DbHelper subclasses (one per entity group)
    DbHelper.cs         ← base class with ExecuteAsync logging wrappers
  AdminPageModel.cs     ← base PageModel for admin area ([Authorize] AdminCookie)
  CustomerPageModel.cs  ← base PageModel for customer area ([Authorize] CustomerCookie)
  PasswordCryptoHelper.cs
  ConvertHelper.cs
  SharedHelper.cs
  UsersHelper.cs
Migrations/             ← EF Core migration files (not used — DB schema is managed manually)
Models/                 ← EF entity classes only, no logic
Services/               ← application services (email, translation, etc.)
wwwroot/
  custom/               ← project-specific compiled JS (*.dist.js)
  js/ css/              ← page-level JS (paired .js source + .dist.js bundle)
  vendor/               ← third-party libs (do not edit)
Program.cs              ← DI registrations + middleware pipeline
appsettings.json        ← connection strings, SMTP, upload paths
```

---

## Naming Conventions

### C# / Files
| Thing | Convention | Example |
|---|---|---|
| Classes, properties, methods | PascalCase | `AdminUser`, `GetByIdAsync` |
| Private fields | `_camelCase` | `_db`, `_cache` |
| Interfaces | `I` prefix | `IHttpContextAccessor` |
| Helper classes | `*Helper.cs` | `PasswordCryptoHelper` |
| DB helper classes | `*DbHelper.cs` in `Helper/DB/` | `AdminDbHelper` |
| Services | `*Service.cs` | `TranslationService` |
| Constants classes | `*Constants.cs`, static | `AppConstants`, `RoleConstants` |
| DTOs | `*Dtos.cs` (one file per domain) | `CustomerDtos.cs` |
| Message keys | `Msg*` string constants | `MessageConstants.MsgSaveSuccess` |
| PageModel classes | `*Model` suffix | `IndexModel`, `LoginModel` |
| Dropdown `<select>` elements | `ddl` prefix | `ddlLanguage`, `ddlStatus` |
| Textbox `<input type="text/password">` | `txt` prefix | `txtUsername`, `txtEmail` |

### Database
| Thing | Convention | Example |
|---|---|---|
| Table names | snake_case | `admin_users`, `language_resources` |
| Column names | snake_case, explicit via Fluent API | `full_name`, `is_active`, `created_at` |
| Identity PKs | `.UseIdentityColumn()` | `id` |

---

## Coding Patterns

### CRUD handler naming
Use named handlers for CRUD operations — never overload a single `OnPost`:

```csharp
public async Task<IActionResult> OnPostCreateAsync() { ... }
public async Task<IActionResult> OnPostUpdateAsync() { ... }
public async Task<IActionResult> OnPostDeleteAsync() { ... }
```

Wire up in the form with `asp-page-handler`:
```html
<form method="post">
  <button asp-page-handler="Create">Save</button>
  <button asp-page-handler="Update">Update</button>
  <button asp-page-handler="Delete">Delete</button>
</form>
```

### DbHelper pattern
All database access through scoped `DbHelper` subclasses injected into PageModels. Every method must wrap its operation in `ExecuteAsync` — this provides automatic error logging with method name, file, and line number:

```csharp
// Helper/DB/ThingDbHelper.cs
public class ThingDbHelper : DbHelper
{
  public ThingDbHelper(AppDbContext db, ILoggerFactory loggerFactory) : base(db, loggerFactory) { }

  // Query (returns value)
  public async Task<Thing?> GetByIdAsync(int id)
      => await ExecuteAsync(async () => await _db.Things.FindAsync(id));

  // Command (no return value)
  public async Task DeleteAsync(int id)
      => await ExecuteAsync(async () =>
      {
        var thing = await _db.Things.FindAsync(id);
        if (thing != null)
        {
          thing.Status = UserStatusConstants.Deleted;
          await _db.SaveChangesAsync();
        }
      });
}
```

Register in `Program.cs`:
```csharp
builder.Services.AddScoped<ThingDbHelper>();
```

### Models — plain POCOs, no logic
```csharp
// Models/Thing.cs
namespace MyApp.Models;

public class Thing
{
  public int Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public bool IsActive { get; set; } = true;
  public DateTime CreatedAt { get; set; }
}
```

### AppDbContext — Fluent API only, one entity block per table
```csharp
modelBuilder.Entity<Thing>(entity =>
{
  entity.ToTable("things");
  entity.HasKey(e => e.Id);
  entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
  entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
  entity.Property(e => e.IsActive).HasColumnName("is_active");
  entity.Property(e => e.CreatedAt).HasColumnName("created_at");
});
```

### Services — inject DbContext + IMemoryCache + IHttpContextAccessor as needed
```csharp
public class MyService
{
  private readonly AppDbContext _db;
  private readonly IMemoryCache _cache;

  public MyService(AppDbContext db, IMemoryCache cache)
  {
    _db = db;
    _cache = cache;
  }
}
```

### Constants — static classes, `const string` only
```csharp
public static class ThingConstants
{
  public const string Active = "active";
  public const int MaxItems = 50;
}
```

### Page authorization — inherit from area base model
Protected pages must inherit from `AdminPageModel` (admin area) or `CustomerPageModel` (customer area) instead of `PageModel` directly. Do not use `[Authorize]` on individual pages — the base model carries it.

```csharp
// Admin page
public class IndexModel : AdminPageModel { ... }

// Customer page
public class DashboardModel : CustomerPageModel { ... }
```

Public pages (Login, ForgotPassword, ResetPassword) inherit from `BasePageModel` or `PageModel` directly — no `[Authorize]`.

### BackgroundService — use raw Npgsql, not EF Core
`BackgroundService` is a singleton. `AppDbContext` is scoped and cannot be injected directly into a singleton. Use raw `NpgsqlConnection` for any DB work in a hosted service:

```csharp
using var conn = new NpgsqlConnection(_connectionString);
await conn.OpenAsync(stoppingToken);
using var cmd = new NpgsqlCommand("DELETE FROM ...", conn);
await cmd.ExecuteNonQueryAsync(stoppingToken);
```

### Localization — always use `TranslationService` for user-facing strings
```csharp
var label = await _translationService.GetAsync(MessageConstants.SaveSuccess);
```

---

## Dependency Injection Rules

All services are registered as **Scoped** unless they hold no per-request state:

```csharp
// Scoped (default for everything)
builder.Services.AddScoped<ThingDbHelper>();
builder.Services.AddScoped<ThingService>();

// Singleton — only for stateless infrastructure
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
```

Config POCOs are bound with `Configure<T>`:
```csharp
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
```

---

## Middleware Pipeline Order

Do not reorder these — order is load-bearing:

```
UseSession
UseStaticFiles
UseRequestLocalization
UseRouting
UseAuthentication
UseAuthorization
MapRazorPages
```

---

## Localization

Supported cultures: `en`, `zh`, `ms`. Language stored in `lang` cookie.  
All translations live in the `language_resources` DB table — never hard-code user-facing strings.  
Use `MessageConstants.*` keys when calling `TranslationService.GetAsync(key)`.

---

## Always

- Use `async/await` for all database and I/O operations
- Map DB columns explicitly with `.HasColumnName("snake_case")` in `AppDbContext`
- Keep Models as plain POCOs — no methods, no business logic
- Put all EF queries in `DbHelper` subclasses, not in PageModels or Services — use `AdminDbHelper` for admin entities, `CustomerDbHelper` for customer entities
- Register new DbHelpers and Services as `Scoped` in `Program.cs`
- Use `string.Empty` as default for string properties in models
- Use `Constants/` classes for any magic strings or numbers used in multiple places
- Use `DbSet<T>` with expression-bodied property syntax: `public DbSet<Thing> Things => Set<Thing>();`
- Create a new `*DbHelper` class per entity group (one for Admin, one for Customer, etc.)
- After model changes, provide a raw SQL script for pgAdmin — do not suggest `dotnet ef migrations`
- Use soft delete — set `Status = UserStatusConstants.Deleted` instead of physically deleting records, unless explicitly told otherwise
- All `<select>` element names and `[BindProperty]` properties for dropdowns use `ddl` prefix — e.g. `ddlLanguage`, `ddlStatus`
- All UI-facing strings go through `TranslationService.GetAsync(key)` — no hardcoded strings in `.cshtml` or PageModels
- Use SweetAlert2 for all alert/notification messages — vendor file at `~/vendor/libs/sweetalert2/sweetalert2.dist.js`, never use `alert()` or inline Bootstrap alerts
- New pages follow the folder structure: admin pages under `Areas/Admin/Pages/`, customer pages under `Areas/Customer/Pages/`
- All protected admin pages inherit from `AdminPageModel`; all protected customer pages inherit from `CustomerPageModel` — never use a bare `[Authorize]` attribute on individual pages
- All DbHelper methods must wrap their body in `ExecuteAsync(...)` — never call `_db.*` directly in a DbHelper method
- Use named handlers for CRUD: `OnPostCreateAsync`, `OnPostUpdateAsync`, `OnPostDeleteAsync` with matching `asp-page-handler` on form buttons

## Never

- Never add MVC controllers — this project is Razor Pages only
- Never query `AppDbContext` directly inside a PageModel — use a `DbHelper`
- Never hard-code user-facing strings — use `TranslationService` + `MessageConstants`
- Never store passwords as plain text — use `PasswordCryptoHelper` (AES) for now
- Never use `AddTransient` for `DbHelper` or `Service` classes — use `AddScoped`
- Never put business logic inside Model classes
- Never edit files under `wwwroot/vendor/` — those are third-party libs
- Never edit `*.dist.js` or `*.dist.css` files directly — edit the source and rebuild via Webpack/Gulp
- Never skip `UseAuthentication` / `UseAuthorization` in the pipeline
- Never physically delete records — always soft delete via `Status = UserStatusConstants.Deleted`
- Never reorder the middleware pipeline without understanding the dependencies
- Never inject `AppDbContext` into a `BackgroundService` — use raw `NpgsqlConnection` instead (EF Core's DbContext is scoped, BackgroundService is singleton)
- Never use `[Authorize]` directly on a page model — use `AdminPageModel` or `CustomerPageModel` as the base class
- Never commit real SMTP passwords or connection strings — move secrets to `appsettings.Development.json` or User Secrets
