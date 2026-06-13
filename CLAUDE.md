# CLAUDE.md — Project Guidelines for MRMR

## Technology Stack

- **Framework:** ASP.NET Core Razor Pages, .NET 10
- **Database:** PostgreSQL via Npgsql EF Core (code-first, Fluent API)
- **Auth:** `AuthSchemeConstants.Admin` (`"AdminCookie"`) / `AuthSchemeConstants.Customer` (`"CustomerCookie"`) — 30-day sliding expiration
- **i18n:** DB-backed `LanguageResource` table + `TranslationService` (memory-cached)
- **Frontend:** Bootstrap 5, jQuery, DataTables, ApexCharts, Select2 (`wwwroot/vendor/`)
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

**Two DbContexts:** `AppDbContext` = main app DB. `AuditDbContext` targets `mrmr_audit` — holds `audit_logs`, `user_sessions`, `page_access_history`, `page_access_history_archive`.

---

## Folder Structure

```
Areas/
  Admin/Pages/          ← admin Razor Pages
    Layouts/            ← _CommonMasterLayout, etc.
    _Partials/
    Account/
  Customer/Pages/
  Applicant/Pages/
Constants/
Data/
  AppDbContext.cs       ← all Fluent API config
  AuditDbContext.cs
Dtos/                   ← one file per domain
Helper/
  DB/                   ← DbHelper subclasses
    DbHelper.cs         ← base with ExecuteAsync wrappers
  AdminPageModel.cs
  CustomerPageModel.cs
  ApplicantPageModel.cs
Models/                 ← EF entity POCOs, no logic
Services/
wwwroot/vendor/         ← do not edit
Program.cs
appsettings.json
```

---

## Naming Conventions

### C# / Files
| Thing | Convention | Example |
|---|---|---|
| DB helper classes | `*DbHelper.cs` in `Helper/DB/` | `AdminDbHelper` |
| Services | `*Service.cs` | `TranslationService` |
| Constants classes | `*Constants.cs`, static | `AppConstants` |
| DTOs | `*Dtos.cs` (one file per domain) | `CustomerDtos.cs` |
| Message keys | `Msg*` constants | `MessageConstants.MsgSaveSuccess` |
| Dropdown `<select>` | `ddl` prefix | `ddlLanguage` |
| Textbox `<input>` | `txt` prefix | `txtUsername` |

### Page folder names — always plural

Folders under `Areas/*/Pages/` must use plural (`Countries/`, `Departments/`). Avoids namespace collision where the folder name matches the model class.

**Exception — compound names:** resolve with a using alias:
```csharp
using PageAccessHistoryModel = MyApp.Models.PageAccessHistory;
```

### Database
- Table names: `snake_case` (`admin_users`)
- Column names: `snake_case`, explicit `.HasColumnName()` (`full_name`, `is_active`)
- Identity PKs: `.UseIdentityColumn()`

---

## Coding Patterns

### CRUD handler naming
```csharp
public async Task<IActionResult> OnPostCreateAsync() { ... }
public async Task<IActionResult> OnPostUpdateAsync() { ... }
public async Task<IActionResult> OnPostDeleteAsync() { ... }
```
Wire with `asp-page-handler="Create"` / `"Update"` / `"Delete"`.

### DbHelper — two variants

**Variant A — extends `DbHelper` (standard, for `AppDbContext`):**
```csharp
public class ThingDbHelper : DbHelper
{
  public ThingDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory) : base(db, audit, loggerFactory) { }

  public async Task<Thing?> GetByIdAsync(int id)
      => await ExecuteAsync(async () => await _db.Things.FindAsync(id));
}
```

Method naming — entity-agnostic within each class: `GetAllAsync`, `GetByIdAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`.

**Variant B — standalone (for `AuditDbContext`-only):** Do **not** extend base. See `UserSessionDbHelper` / `PageAccessDbHelper` for boilerplate. Inject `AuditDbContext` + `ILoggerFactory` directly.

**`UpdateAsync`** — accept a model object, fetch tracked entity inside, assign field by field. Never call `_db.Update()` on detached input:
```csharp
public async Task UpdateAsync(Thing thing, string updatedBy) { ... }
```

**When adding/renaming/removing a field, update every affected DbHelper method** — missing a field causes silent data loss.

### AppDbContext — Fluent API only
```csharp
modelBuilder.Entity<Thing>(entity =>
{
  entity.ToTable("things");
  entity.HasKey(e => e.Id);
  entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
  entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
  entity.Property(e => e.CreatedAt).HasColumnName("created_at")
        .HasDefaultValueSql("now() AT TIME ZONE 'utc'");
  entity.Property(e => e.UpdatedAt).HasColumnName("updated_at")
        .HasDefaultValueSql("now() AT TIME ZONE 'utc'");
});
```

**Non-PK FK** — add `HasPrincipalKey` when FK targets a unique non-PK column (omitting causes runtime `column does not exist`):
```csharp
entity.HasOne(e => e.System).WithMany()
      .HasForeignKey(e => e.SystemCode).HasPrincipalKey(e => e.SystemCode)
      .OnDelete(DeleteBehavior.Cascade);
```

**Self-referencing FK** — `OnDelete(DeleteBehavior.Restrict)`, never `Cascade`. Load flat, build tree in memory — never filtered `Include/ThenInclude` on self-referencing (silently returns empty `Children`).

Raw SQL: `created_at TIMESTAMPTZ NOT NULL DEFAULT (now() AT TIME ZONE 'utc')`

### Page authorization
```csharp
public class IndexModel : AdminPageModel { ... }         // admin area
public class DashboardModel : CustomerPageModel { ... }  // customer area
public class IndexModel : ApplicantPageModel { ... }     // applicant portal (auth required)
```
`ApplicantPageModel` applies `[Authorize(AuthenticationSchemes = AuthSchemeConstants.Applicant)]`.

Public pages (Login, ForgotPassword) inherit `BasePageModel` or `PageModel`. `BasePageModel` exposes `AlertMessageContent`, `AlertMessageType`, `AlertMessageTitle` — **not** `AlertMessage`.

### Applicant area layouts
`_ViewStart.cshtml` defaults to `_PublicLayout`. Portal pages must override:
```cshtml
@{ Layout = "Layouts/_PortalLayout"; }
```

### PageModel result helpers — no Controller shorthand
`Ok()`, `BadRequest()`, `StatusCode()` do not exist on `PageModel`. Use:
```csharp
return new OkResult();
return new BadRequestObjectResult("message");
return new JsonResult(new { success = true });
```

### BackgroundService — scoped services
```csharp
using var scope = _serviceProvider.CreateScope();
var myHelper    = scope.ServiceProvider.GetRequiredService<MyDbHelper>();
await myHelper.DoWorkAsync();
```
Guard per-interval jobs with a timestamp field:
```csharp
private DateTime _lastRun = DateTime.MinValue;
if (DateTime.UtcNow.Date > _lastRun.Date) { /* do work */ _lastRun = DateTime.UtcNow; }
await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
```
All background jobs live in `Services/LogCleanupService.cs`.

### MsSqlHelper — read-only secondary SQL Server
Subclass `MsSqlHelper` in `Helper/DB/`. Use raw `SqlConnection`, not EF Core:
```csharp
public class MyMsSqlHelper : MsSqlHelper
{
    public MyMsSqlHelper(IConfiguration config, ILoggerFactory loggerFactory)
        : base(config, loggerFactory, nameof(MyMsSqlHelper)) { }

    public async Task<string?> GetSomethingAsync(string key)
        => await ExecuteAsync(async () =>
        {
            using var conn = await OpenAsync();
            using var cmd  = new SqlCommand("SELECT value FROM table WHERE key = @key", conn);
            cmd.Parameters.AddWithValue("@key", key);
            return await cmd.ExecuteScalarAsync() as string;
        });
}
```
Connection string key: `MsSqlConnection`. Placeholder `PaymentStatusMsSqlHelper` exists — extend for Order module.

### SystemSettingService
```csharp
var maxAmount  = await _settingService.GetAsDecimalAsync("Wallet.MaxAdjustmentAmount", 10000);
var retryLimit = await _settingService.GetAsIntAsync("Wallet.PayoutRetryLimit", 3);
var isEnabled  = await _settingService.GetAsBoolAsync("Feature.SomeFlag", false);
_settingService.ClearCache(); // call after any admin update — cache TTL 1 hour
```

### File upload — profile images
- Upload paths in `appsettings.json` under `UploadPaths`
- Save via `ProfileImageHelper.SaveProfileImageAsync(file, username, fullPath)` — filename: `{guid}_{sanitizedUsername}{ext}`
- Allowed: `.jpg`, `.jpeg`, `.png` — validated server + client. Max 2MB.
- `[BindProperty] public IFormFile? fileProfileImage { get; set; }`
- On update: delete old physical file first. On soft delete: **do NOT** delete the physical file.

### Password fields — always include show/hide toggle
```html
<div class="input-group">
  <div class="form-floating form-floating-outline flex-grow-1">
    <input type="password" id="txtPassword" class="form-control" name="txtPassword" placeholder="············" />
    <label for="txtPassword">@await T.GetAsync("Password")</label>
  </div>
  <button class="btn btn-outline-secondary" type="button" id="btnTogglePwd" tabindex="-1">
    <i id="iconTogglePwd" class="ri ri-eye-off-line"></i>
  </button>
</div>
```
Toggle JS: swap `input.type` between `password`/`text`; swap `ri-eye-off-line`/`ri-eye-line`.

> Remix icons require both `ri` base class + specific class (`ri ri-eye-off-line`). Omitting `ri` renders empty.

### Quill rich text editor
Load compiled: `~/vendor/libs/quill/quill.dist.js` + `~/vendor/libs/quill/editor.dist.css`.

Hidden `<input>` stores HTML; `<div>` is the editor. Sync on `submit`:
```javascript
var editor = new Quill('#editor_en', { theme: 'snow' });
editor.clipboard.dangerouslyPasteHTML(document.getElementById('hdnBody_en').value); // pre-fill on Edit
document.querySelector('form').addEventListener('submit', function () {
  document.getElementById('hdnBody_en').value = editor.root.innerHTML;
});
```
Multiple languages: one Quill per language code, sync all in submit handler. Multiple forms: scope sync with `form.contains(hdn)`. Naming: `hdn` ↔ `editor` prefix with same suffix (`hdnNew_en` / `editorNew_en`).

### Date/time inputs
- Use `type="datetime-local"`
- GET: `entity.StartAt.ToUserLocalTime(UserTimezone, AppConstants.DateTimeInputFormat)`
- POST: `DateTime.TryParseExact(...)` then `.ToUtcFromUserTimezone(UserTimezone)`
- `AppConstants.DateTimeInputFormat` = `"yyyy-MM-dd HH:mm"`

### Index pages — always reset AlertMessageType in OnGetAsync
```csharp
public async Task OnGetAsync()
{
  AlertMessageType = "";
  Items = await _dbHelper.GetAllAsync();
}
```
Without this, a success toast can re-fire on browser back-navigation.

### DataTables — standard listing pattern
CSS/JS (CDN 1.13.6) and `admin-datatable.js` loaded globally — do not import per page.
```js
$(document).ready(function () {
  initDataTable('#tblThings', 3); // 3 = 0-based Actions column index
});
```

**Row buttons — never inline `onclick` on DataTable rows** (DOM rebuilt on sort/page/search). Use `data-*` + delegated events:
```javascript
$(document).on('click', '.btn-edit-thing', function () {
  openEditModal($(this).data('code'), $(this).data('name'));
});
```
Exception: `onclick` ok for guaranteed safe identifiers with no user data.

### Index pages — server-side pre-filters + DataTables
- Filter form: `method="get"`, values in query string
- Filter properties: `Filter*` prefix + `[BindProperty(SupportsGet = true)]`
- Dropdown lists: `ddl*` prefix; first option `Value = string.Empty` ("All")
- "Clear" link: page URL with no query string

### High-volume log/audit pages — server-side pagination
No DataTables. `PageSize = 50`, `CurrentPage` as `[BindProperty(SupportsGet = true)]`. DbHelper returns `(List<T> Items, int TotalCount)` with filters at DB level before `.Skip().Take()`. Pagination links carry all filter params. End-date: `AddDays(1).AddSeconds(-1)`.

### Index pages — Actions column dropdown
Bootstrap dropdown, icon-only trigger (`ri-more-2-fill`), `dropdown-menu-end`. Edit first, Toggle Status second:
```cshtml
<td>
  <div class="dropdown">
    <button type="button" class="btn btn-sm btn-outline-secondary dropdown-toggle"
            data-bs-toggle="dropdown" aria-expanded="false">
      <i class="ri ri-more-2-fill"></i>
    </button>
    <ul class="dropdown-menu dropdown-menu-end">
      <li>
        <a class="dropdown-item" asp-area="Admin" asp-page="/Things/Edit" asp-route-id="@thing.Id">
          <i class="ri ri-edit-line me-1"></i>@await T.GetAsync("Edit")
        </a>
      </li>
      <li>
        <button class="dropdown-item" type="button" onclick="toggleStatus(@thing.Id)">
          <i class="ri ri-toggle-line me-1"></i>@await T.GetAsync("ToggleStatus")
        </button>
      </li>
    </ul>
  </div>
</td>
```

### AJAX JSON body handlers
`[FromBody]` + `Content-Type: application/json`. Antiforgery in `RequestVerificationToken` header:
```csharp
public async Task<IActionResult> OnPostSaveSortAsync([FromBody] List<ThingSortItem> items)
{
  try { await _thingDbHelper.SaveSortOrderAsync(items, CurrentUsername); return new JsonResult(new { success = true }); }
  catch { return new JsonResult(new { success = false, message = await _translation.GetAsync(MessageConstants.SaveError) }); }
}
```
```javascript
fetch('?handler=SaveSort', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
  body: JSON.stringify(items)
});
```
Sort DTO in `Dtos/*Dtos.cs`: `public class ThingSortItem { public int Id { get; set; } public int SortOrder { get; set; } }`

### SortableJS drag-to-reorder
CDN: `https://cdnjs.cloudflare.com/ajax/libs/Sortable/1.15.2/Sortable.min.js`

Each item needs `data-id="@item.Id"`. On drag end, collect ordered IDs and fire AJAX JSON POST (use `[FromBody]` handler pattern above).

### Non-`[BindProperty]` read-only properties
When a value must control view logic but not be overwritten by POST, declare as plain property (not `[BindProperty]`). Set in `OnGetAsync` and every `ReloadPageAsync` path.

### Edit pages — audit section + soft delete

Every Admin Edit page must show audit metadata and include `OnPostSoftDeleteAsync`.

**PageModel properties (populate in `OnGetAsync`):** `CreatedBy`, `CreatedAt`, `UpdatedBy`, `UpdatedAt`, `MsgDeleteConfirmTitle`, `MsgDeleteConfirmText`, `MsgDeleteConfirmBtn`, `MsgCancelBtn`, `MsgDeleteSuccess`, `MsgDeleteError`, `LabelDelete`.

**Audit section** — below `</form>` inside `card-body`. Dates: `.ToUserLocalTime(Model.UserTimezone, AppConstants.DateTimeFormat)`.

**Soft delete handler:**
```csharp
public async Task<IActionResult> OnPostSoftDeleteAsync(string entityCode)
{
  try
  {
    await _thingDbHelper.UpdateStatusAsync(entityCode, StatusConstants.Deleted, CurrentUsername);
    return new JsonResult(new { success = true, message = await _translation.GetAsync(MessageConstants.DeleteSuccess) });
  }
  catch { return new JsonResult(new { success = false, message = await _translation.GetAsync(MessageConstants.DeleteError) }); }
}
```

Delete button with `ms-auto` + hidden `<form id="formAjax" method="post">@Html.AntiForgeryToken()</form>` + `pageMsg` JS object:
```javascript
var pageMsg = {
  deleteConfirmTitle: '@Html.Raw(Model.MsgDeleteConfirmTitle)',
  deleteConfirmText:  '@Html.Raw(Model.MsgDeleteConfirmText)',
  deleteConfirmBtn:   '@Html.Raw(Model.MsgDeleteConfirmBtn)',
  cancelBtn:          '@Html.Raw(Model.MsgCancelBtn)',
  deleteSuccess:      '@Html.Raw(Model.MsgDeleteSuccess)',
  deleteError:        '@Html.Raw(Model.MsgDeleteError)'
};
```
SweetAlert confirm → `$.post('?handler=SoftDelete&entityCode=...')` → success toast → redirect to index.

### Row action partials — passing translated strings to JS
Partials don't have access to the page's `pageMsg`. Pass translated strings as function arguments from the partial's C# context via `@Html.Raw(model.MsgXxx)`.

### Sidebar navigation menu
Rendered by `_VerticalMenu.cshtml`. Calls `MenuDbHelper.GetNavMenuAsync(roleId, isSuperAdmin)`.

URL format: `/Admin/Countries` (plural, no `/Index`). Login claims: `RoleId` and `IsSuperAdmin` stored in auth cookie. Uses flat DB load + in-memory tree build — never filtered `Include/ThenInclude` (silently returns empty `Children`).

**`IsActive` detection:**
```csharp
bool IsActive(string? url) =>
    !string.IsNullOrEmpty(url) &&
    (current == url || current?.StartsWith(url + "/") == true);
```
Do not use `ViewContext.RouteData.Values["Page"]`.

### Translation-enabled entities

**DbContext** — cascade delete on `HasMany`:
```csharp
entity.HasMany(e => e.Translations).WithOne(t => t.PaymentMethod)
      .HasForeignKey(t => t.PaymentCode).OnDelete(DeleteBehavior.Cascade);
```

`GetAllAsync` always takes `string languageCode`. Use `CurrentLangCode` — never hardcode `"en"`.

`BuildInputsAsync` — private helper for GET and POST-error paths. Pass existing translations on GET; `null` to re-read from `Request.Form` on error.

Entity name for delete confirm title — use English translation, fall back to code:
```csharp
var entityName = entity.Translations.FirstOrDefault(t => t.LanguageCode == "en")?.PaymentName ?? paymentCode;
```

### Localization
All visible text via `T.GetAsync`. JS strings declared in `@{ }` block before `@section PageScripts`.

### Cropper.js — profile image crop + AJAX upload
CDN: `cropperjs/1.6.2`. Pattern: file input → FileReader → Cropper 1:1 crop → AJAX via separate `OnPostUploadImageAsync`. Handler returns `{ success, filename, message }`. Update `<img src>` with `?t=Date.now()` cache bust. On soft delete: do NOT delete the physical file.

### Select2 AJAX search
```csharp
public async Task<IActionResult> OnGetSearchMembersAsync(string term)
{
  var results = await _memberDbHelper.SearchAsync(term ?? string.Empty);
  return new JsonResult(results.Select(r => new { id = r.Id, text = $"{r.Username} — {r.FullName}" }));
}
```
Compute URL in `@{ }` block. Use `on('select2:select')` to populate a hidden `<input>` — the `<select>` is display only.

### Multi-section Manage page
Single `#formAjax` antiforgery token + shared `saveSection(handler, data)` JS function. All `[BindProperty]` per section; plain properties for display in `PopulateAsync()`. Add both `/Edit` and `/Manage` to `Constants/Routes.cs`.

---

## Middleware Pipeline

Do not reorder:
```
UseSession → UseStaticFiles → UseRequestLocalization → UseRouting →
UseAuthentication → UseAuthorization →
UseMiddleware<MaintenanceMiddleware> → UseMiddleware<SessionTrackingMiddleware> →
MapRazorPages
```

---

## Localization

Cultures: `en`, `zh-Hans`. Language in `lang` cookie. Login page `<select>` from `languages` table (active only) — writes `.AspNetCore.Culture` + `lang` cookies on change.

---

## Razor (.cshtml) Comments

Always `@* ... *@`. Never `<!-- -->` for code notes. Never `{{!-- --}}` (Handlebars).

---

## Always

- `DateTime.UtcNow` for all timestamp fields
- In `CreateAsync`: assign all fields including `CreatedAt = DateTime.UtcNow`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`
- `HasDefaultValueSql("now() AT TIME ZONE 'utc'")` on every `created_at`/`updated_at`
- `.HasColumnName("snake_case")` for all DB columns
- `DbSet<T>` expression-bodied: `public DbSet<Thing> Things => Set<Thing>();`
- All DbHelper methods wrap body in `ExecuteAsync(...)` — never call `_db.*` directly
- SQL scripts to `D:\MRMR\Script\` after model changes — do not suggest `dotnet ef migrations`
- `long`/`BIGSERIAL` for PKs on high-volume tables
- Inject scoped services into middleware `InvokeAsync` as method parameters, never constructor
- After any admin CRUD module: output `language_resources` SQL upsert for all `T.GetAsync(...)` keys, both `en` and `zh-Hans`

## Never

- Never use Razor reserved keywords as loop variables — `section`, `functions`, `namespace`, `page`, `model`, `inherits`, `helper`. Use `stype` instead of `section`, etc.
- Never `@(condition ? "selected" : "")` inside `<option>` — causes RZ1031; use `@if` blocks
- Never add MVC controllers
- Never query `AppDbContext` in a PageModel — use DbHelper
- Never hardcode user-facing strings
- Never store passwords as plain text — use `PasswordCryptoHelper` (AES)
- Never `AddTransient` for DbHelper/Service — use `AddScoped`
- Never edit `wwwroot/vendor/` or `*.dist.js`/`*.dist.css` directly
- Never physically delete records — soft delete via `Status = StatusConstants.Deleted`
- Never inject scoped DbHelper into `BackgroundService` constructor — use `IServiceProvider.CreateScope()`
- Never use `[Authorize]` directly on PageModel — use area base model
- Never commit SMTP passwords or connection strings
- `permissions.module` is varchar storing `menus.menu_code` (NOT an int `menu_id`). EF: `HasForeignKey(p => p.Module).HasPrincipalKey(m => m.MenuCode)`
- Never inline `onclick="fn('@value')"` on DataTable rows when value is user data — use `data-*` + delegated events
