# CLAUDE.md — Project Guidelines for MRMR

## Technology Stack

- **Framework:** ASP.NET Core Razor Pages, .NET 10
- **Database:** PostgreSQL via Npgsql EF Core (code-first, Fluent API)
- **Auth:** `AuthSchemeConstants.Admin` / `AuthSchemeConstants.Customer` / `AuthSchemeConstants.Applicant` — 30-day sliding expiration
- **i18n:** DB-backed `LanguageResource` table + `TranslationService` (memory-cached)
- **Frontend:** Materialize CSS + jQuery (public/applicant area), Bootstrap 5 + jQuery (admin/customer area), DataTables, ApexCharts, Select2 (`wwwroot/vendor/`)
- **Build:** Webpack + Gulp (`*.dist.js` / `*.dist.css`)
- **Root namespace:** `MyApp`
- **GitHub:** https://github.com/avenueconcept-pixel/MRMR

---

## Architecture

```
PageModel (.cshtml.cs)
  └── DbHelper (Helper/DB/)      ← repository-style EF Core queries
        └── AppDbContext          ← main EF Core DbContext (PostgreSQL)
        └── AuditDbContext        ← audit DbContext (mrmr_audit DB)
  └── Service (Services/)        ← cross-cutting logic (email, translation)
```

**Two DbContexts:** `AppDbContext` = main app DB. `AuditDbContext` targets `mrmr_audit` — `audit_logs`, `user_sessions`, `page_access_history`, `page_access_history_archive`.

---

## Folder Structure

```
Areas/
  Admin/Pages/           ← admin Razor Pages (Bootstrap 5)
  Customer/Pages/        ← customer Razor Pages (Bootstrap 5)
  Applicant/Pages/       ← applicant portal (Materialize CSS)
Constants/
Data/
  AppDbContext.cs        ← all Fluent API config
  AuditDbContext.cs
Dtos/                    ← one file per domain
Helper/
  DB/                    ← DbHelper subclasses
    DbHelper.cs          ← base with ExecuteAsync wrappers
  AdminPageModel.cs / CustomerPageModel.cs / ApplicantPageModel.cs / PublicPageModel.cs
Models/                  ← EF entity POCOs, no logic
Services/
wwwroot/vendor/          ← do not edit
```

---

## Naming Conventions

| Thing | Convention | Example |
|---|---|---|
| DB helper classes | `*DbHelper.cs` in `Helper/DB/` | `AdminDbHelper` |
| Services | `*Service.cs` | `TranslationService` |
| Constants | `*Constants.cs`, static | `AppConstants` |
| DTOs | `*Dtos.cs` (one file per domain) | `CustomerDtos.cs` |
| Dropdown `<select>` | `ddl` prefix | `ddlLanguage` |
| Textbox `<input>` | `txt` prefix | `txtUsername` |

**Page folder names — always plural.** Exception: use a using alias when folder name collides with model class name.

**Database:** table/column names `snake_case`, explicit `.HasColumnName()`, identity PKs `.UseIdentityColumn()`.

---

## Coding Patterns

### CRUD handler naming
```csharp
OnPostCreateAsync / OnPostUpdateAsync / OnPostDeleteAsync
```
Wire with `asp-page-handler="Create"` / `"Update"` / `"Delete"`.

### DbHelper

**Variant A — extends `DbHelper` (AppDbContext):**
```csharp
public class ThingDbHelper : DbHelper
{
    public ThingDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory) : base(db, audit, loggerFactory) { }

    public async Task<Thing?> GetByIdAsync(int id)
        => await ExecuteAsync(async () => await _db.Things.FindAsync(id));
}
```
Method names: `GetAllAsync`, `GetByIdAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`.

**`UpdateAsync`** — fetch tracked entity inside, assign field by field. Never `_db.Update()` on detached input. Update every affected method when adding/renaming/removing fields (silent data loss otherwise).

**Variant B — standalone (AuditDbContext only):** do not extend base. See `UserSessionDbHelper` / `PageAccessDbHelper`.

### AppDbContext — Fluent API only
```csharp
entity.ToTable("things");
entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() AT TIME ZONE 'utc'");
```
**Non-PK FK** — add `HasPrincipalKey` when FK targets a unique non-PK column.
**Self-referencing FK** — `OnDelete(DeleteBehavior.Restrict)`. Load flat, build tree in memory — never filtered `Include/ThenInclude` (silently returns empty `Children`).

### Page authorization
```
Admin pages    → AdminPageModel
Customer pages → CustomerPageModel
Applicant portal (auth) → ApplicantPageModel
Public pages (no auth)  → PublicPageModel  ← inherits BasePageModel, no [Authorize]
```
Never use `[Authorize]` directly on a PageModel. `BasePageModel` exposes `AlertMessageContent`, `AlertMessageType`, `AlertMessageTitle`.

### Applicant area layouts
`_ViewStart.cshtml` defaults to `_PublicLayout`. Portal pages must override:
```cshtml
@{ Layout = "Layouts/_PortalLayout"; }
```

### PageModel result helpers
`Ok()` / `BadRequest()` / `StatusCode()` don't exist on `PageModel`. Use `new OkResult()`, `new BadRequestObjectResult(...)`, `new JsonResult(...)`.

### BackgroundService — scoped services
```csharp
using var scope  = _serviceProvider.CreateScope();
var myHelper     = scope.ServiceProvider.GetRequiredService<MyDbHelper>();
```
All background jobs live in `Services/LogCleanupService.cs`.

### MsSqlHelper — read-only secondary SQL Server
Subclass `MsSqlHelper` in `Helper/DB/`. Use raw `SqlConnection` (not EF Core). Connection string key: `MsSqlConnection`. Placeholder `PaymentStatusMsSqlHelper` exists — extend for Order module.

### SystemSettingService
```csharp
await _settingService.GetAsBoolAsync("Feature.Flag", false);
await _settingService.GetAsDecimalAsync("Wallet.Max", 10000);
_settingService.ClearCache(); // after any admin update
```

### File upload — profile images
Save via `ProfileImageHelper.SaveProfileImageAsync`. Allowed: `.jpg`, `.jpeg`, `.png`, max 2 MB. On update: delete old file first. On soft delete: **do NOT** delete file.

### Password fields — always include show/hide toggle
```html
<div class="input-group">
  <div class="form-floating form-floating-outline flex-grow-1">
    <input type="password" id="txtPassword" class="form-control" name="txtPassword" placeholder="············" />
    <label>@await T.GetAsync("Password")</label>
  </div>
  <button class="btn btn-outline-secondary" type="button" id="btnTogglePwd" tabindex="-1">
    <i id="iconTogglePwd" class="ri ri-eye-off-line"></i>
  </button>
</div>
```
Toggle JS: swap `input.type` between `password`/`text`; swap `ri-eye-off-line`/`ri-eye-line`. Remix icons need both `ri` base class + specific class.

### Quill rich text editor
Load: `~/vendor/libs/quill/quill.dist.js` + `~/vendor/libs/quill/editor.dist.css`. Hidden `<input>` stores HTML; `<div>` is editor. Sync on `submit` via `editor.root.innerHTML`. Multiple languages: one Quill per language code.

### Date/time inputs
`type="datetime-local"`. GET: `.ToUserLocalTime(UserTimezone, AppConstants.DateTimeInputFormat)`. POST: `DateTime.TryParseExact(...)` then `.ToUtcFromUserTimezone(UserTimezone)`.

### Index pages
- Always reset `AlertMessageType = ""` in `OnGetAsync` (prevents toast re-fire on back-navigation).
- DataTables CDN 1.13.6 loaded globally. Init: `initDataTable('#tblThings', columnIndex)`.
- **Never inline `onclick` on DataTable rows** (DOM rebuilt on sort/page/search) — use `data-*` + delegated events.
- Filter form: `method="get"`, `Filter*` prefix, `[BindProperty(SupportsGet = true)]`. "Clear" = page URL with no query string.
- High-volume log pages: server-side pagination, `PageSize = 50`, `CurrentPage` bound, DbHelper returns `(List<T>, int TotalCount)`.
- Actions column: Bootstrap dropdown, `ri-more-2-fill` trigger, `dropdown-menu-end`. Edit first, Toggle Status second.

### AJAX JSON body handlers
`[FromBody]` + `Content-Type: application/json`. Antiforgery via `RequestVerificationToken` header. Return `new JsonResult(new { success = true/false, message })`.

### SortableJS drag-to-reorder
CDN: `Sortable/1.15.2/Sortable.min.js`. Each row needs `data-id`. On drag end, fire AJAX JSON POST with `[FromBody] List<ThingSortItem>`.

### Edit pages — audit section + soft delete
Every Admin Edit page shows audit metadata and includes `OnPostSoftDeleteAsync`. Populate in `OnGetAsync`: `CreatedBy`, `CreatedAt`, `UpdatedBy`, `UpdatedAt`, and translated `MsgDelete*` / `MsgCancel` strings. Soft delete returns `JsonResult({ success, message })`. SweetAlert confirm → `$.post('?handler=SoftDelete&entityCode=...')` → toast → redirect.

### Row action partials
Partials lack access to the page's `pageMsg`. Pass translated strings as function arguments from the partial's C# context.

### Sidebar navigation menu
Rendered by `_VerticalMenu.cshtml`. URL format: `/Admin/Countries` (plural, no `/Index`). Flat DB load + in-memory tree — never filtered `Include/ThenInclude`. `IsActive`: check `current == url || current?.StartsWith(url + "/")`.

### Translation-enabled entities
Cascade delete on `HasMany(...).WithOne(...).HasForeignKey(...).OnDelete(DeleteBehavior.Cascade)`. `GetAllAsync` takes `string languageCode` — use `CurrentLangCode`, never hardcode `"en"`. `BuildInputsAsync` private helper for GET and POST-error paths.

### Cropper.js — profile image crop
CDN: `cropperjs/1.6.2`. File input → FileReader → 1:1 crop → AJAX `OnPostUploadImageAsync`. Update `<img src>` with `?t=Date.now()`.

### Select2 AJAX search
Handler returns `new JsonResult(results.Select(r => new { id, text }))`. URL computed in `@{ }` block. Use `on('select2:select')` to populate a hidden `<input>`.

### Multi-section Manage page
Single `#formAjax` antiforgery + shared `saveSection(handler, data)` JS. Add both `/Edit` and `/Manage` to `Constants/Routes.cs`.

---

## Middleware Pipeline (do not reorder)

```
UseSession → UseStaticFiles → UseRequestLocalization → UseRouting →
UseAuthentication → UseAuthorization →
UseMiddleware<MaintenanceMiddleware> → UseMiddleware<SessionTrackingMiddleware> →
MapRazorPages
```

---

## Localization

Cultures: `en`, `zh-Hans`. Language in `lang` cookie. Login `<select>` from `languages` table — writes `.AspNetCore.Culture` + `lang` cookies on change. All visible text via `T.GetAsync`. JS strings declared in `@{ }` block before `@section PageScripts`.

---

## Razor (.cshtml) Comments

Always `@* ... *@`. Never `<!-- -->` for code notes.

---

## Always

- `DateTime.UtcNow` for all timestamp fields
- `HasDefaultValueSql("now() AT TIME ZONE 'utc'")` on every `created_at`/`updated_at`
- `.HasColumnName("snake_case")` for all DB columns
- `DbSet<T>` expression-bodied: `public DbSet<Thing> Things => Set<Thing>();`
- All DbHelper methods wrap body in `ExecuteAsync(...)`
- SQL scripts to `D:\MRMR\Script\` — do not suggest `dotnet ef migrations`
- `long`/`BIGSERIAL` for PKs on high-volume tables
- Inject scoped services into middleware `InvokeAsync` as method parameters, never constructor
- After any admin CRUD module: output `language_resources` SQL upsert for all `T.GetAsync(...)` keys, both `en` and `zh-Hans`

## Never

- Never Razor reserved keywords as loop variables (`section`, `functions`, `namespace`, `page`, `model`, `inherits`, `helper`) — use `stype` etc.
- Never `@(condition ? "selected" : "")` inside `<option>` — RZ1031; use `@if` blocks
- Never add MVC controllers
- Never query `AppDbContext` in a PageModel — use DbHelper
- Never hardcode user-facing strings
- Never store passwords as plain text — use `PasswordCryptoHelper` (AES)
- Never `AddTransient` for DbHelper/Service — use `AddScoped`
- Never edit `wwwroot/vendor/` or `*.dist.js`/`*.dist.css`
- Never physically delete records — soft delete via `Status = StatusConstants.Deleted`
- Never inject scoped DbHelper into `BackgroundService` constructor — use `IServiceProvider.CreateScope()`
- Never use `[Authorize]` directly on PageModel — use area base model
- Never commit SMTP passwords or connection strings
- `permissions.module` is varchar storing `menus.menu_code`. EF: `HasForeignKey(p => p.Module).HasPrincipalKey(m => m.MenuCode)`
- Never inline `onclick="fn('@value')"` on DataTable rows with user data — use `data-*` + delegated events
