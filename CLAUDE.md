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
        └── AppDbContext          ← main EF Core DbContext (PostgreSQL)
        └── AuditDbContext        ← audit EF Core DbContext (myapp_audit DB)
  └── Service (Services/)        ← cross-cutting logic (email, translation)
```

**Two DbContexts:** `AppDbContext` is the main application database. `AuditDbContext` targets the separate `myapp_audit` database and holds audit/session/access-log tables (`audit_logs`, `user_sessions`, `page_access_history`, `page_access_history_archive`). Both are registered with their own connection strings in `Program.cs`.

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
  AppDbContext.cs       ← main DbContext (app DB), all Fluent API config here
  AuditDbContext.cs     ← audit DbContext (audit DB) — audit_logs, user_sessions, page_access_history
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

### Page folder names — always plural

Razor Pages folders under `Areas/Admin/Pages/` must always use the **plural** form of the entity name, even when the model class is singular:

| Model class | Folder name | Namespace |
|---|---|---|
| `Country` | `Countries/` | `MyApp.Areas.Admin.Pages.Countries` |
| `Language` | `Languages/` | `MyApp.Areas.Admin.Pages.Languages` |
| `Department` | `Departments/` | `MyApp.Areas.Admin.Pages.Departments` |

**Why:** C# resolves an unqualified name to the enclosing namespace first. A folder named `Department/` produces namespace `MyApp.Areas.Admin.Pages.Department`, making `Department` refer to the namespace rather than `MyApp.Models.Department`. Using the plural form avoids the collision entirely — no `using` alias required.

**Exception — compound names that are already plural or can't be pluralised** (e.g. `PageAccessHistory`): when the folder name is unavoidably identical to the model class name, resolve the collision with a using alias at the top of every `.cshtml.cs` in that folder:

```csharp
using PageAccessHistoryModel = MyApp.Models.PageAccessHistory;
```

Then use the alias throughout the file in place of the bare type name (`List<PageAccessHistoryModel>`, etc.).

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

### DbHelper pattern — two variants

**Variant A — extends `DbHelper` base (standard, for `AppDbContext` entities):**

All database access through scoped `DbHelper` subclasses injected into PageModels. Every method must wrap its operation in `ExecuteAsync` — this provides automatic error logging with method name, file, and line number:

```csharp
// Helper/DB/ThingDbHelper.cs
public class ThingDbHelper : DbHelper
{
  public ThingDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory) : base(db, audit, loggerFactory) { }

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
          thing.Status = StatusConstants.Deleted;
          await _db.SaveChangesAsync();
        }
      });
}
```

**DbHelper method naming — use generic, entity-agnostic names within each class.** Since each DbHelper is already scoped to one entity, the entity name is redundant in the method name:

| Method | Not |
|---|---|
| `GetAllAsync` | `GetAllThingsAsync` |
| `GetByIdAsync` | `GetThingByIdAsync` |
| `GetByCodeAsync` | `GetThingByCodeAsync` |
| `AddAsync` | `AddThingAsync` |
| `UpdateAsync` | `UpdateThingAsync` |
| `UpdateStatusAsync` | `UpdateThingStatusAsync` |
| `DeleteAsync` | `DeleteThingAsync` |

**Variant B — standalone (for `AuditDbContext`-only helpers):**

When a DbHelper works exclusively with `AuditDbContext` and never touches `AppDbContext`, do **not** extend the base `DbHelper` (which requires `AppDbContext + AuditHelper`). Instead, write it standalone with its own `ExecuteAsync` wrappers — exactly like `UserSessionDbHelper` and `PageAccessDbHelper`:

```csharp
// Helper/DB/ThingAuditDbHelper.cs
public class ThingAuditDbHelper
{
  private readonly AuditDbContext              _auditDb;
  private readonly ILogger<ThingAuditDbHelper> _logger;

  public ThingAuditDbHelper(AuditDbContext auditDb, ILoggerFactory loggerFactory)
  {
    _auditDb = auditDb;
    _logger  = loggerFactory.CreateLogger<ThingAuditDbHelper>();
  }

  private async Task<T> ExecuteAsync<T>(Func<Task<T>> op,
      [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
  {
    try { return await op(); }
    catch (Exception ex) { _logger.LogError(ex, "DB error in {M} ({F}:{L})", caller, Path.GetFileName(file), line); throw; }
  }

  private async Task ExecuteAsync(Func<Task> op,
      [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
  {
    try { await op(); }
    catch (Exception ex) { _logger.LogError(ex, "DB error in {M} ({F}:{L})", caller, Path.GetFileName(file), line); throw; }
  }
}
```

Register in `Program.cs`:
```csharp
builder.Services.AddScoped<ThingDbHelper>();
```

**`UpdateAsync` signature — always accept a model object, never individual field parameters:**

```csharp
// Correct — pass the entity object
public async Task UpdateAsync(Thing thing, string updatedBy) { ... }
public async Task UpdateAsync(Thing thing, List<ThingTranslation> translations, string updatedBy) { ... }

// Wrong — individual parameters break the pattern and hide missing fields
public async Task UpdateAsync(int id, string name, string status, string updatedBy) { ... }
```

The corresponding `OnPostUpdateAsync` in the PageModel constructs a new model object and passes it:

```csharp
public async Task<IActionResult> OnPostUpdateAsync(string thingCode)
{
  var thing = new Thing
  {
    ThingCode = thingCode,
    Name      = txtName.Trim(),
    Status    = ddlStatus
  };
  // ... build translations ...
  await _thingDbHelper.UpdateAsync(thing, translations, CurrentUsername);
  ...
}
```

Inside `UpdateAsync`, always fetch the tracked entity first, then assign from the passed object — never call `_db.Update()` on the detached input directly.

**When adding, renaming, or removing a field on any Model or PageModel, always check the corresponding DbHelper and update every affected method:**
- `Add*Async` — ensure the new field is assigned before insert
- `Update*Async` — ensure the new field is included in the update block
- Missing a field here means silent data loss — EF Core will not warn you

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

Timestamp columns (`created_at`, `updated_at`) must always default to UTC in PostgreSQL using `HasDefaultValueSql("now() AT TIME ZONE 'utc'")`. This ensures the DB itself writes UTC if a row is ever inserted outside the application:

```csharp
modelBuilder.Entity<Thing>(entity =>
{
  entity.ToTable("things");
  entity.HasKey(e => e.Id);
  entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
  entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
  entity.Property(e => e.IsActive).HasColumnName("is_active");
  entity.Property(e => e.CreatedAt).HasColumnName("created_at")
        .HasDefaultValueSql("now() AT TIME ZONE 'utc'");
  entity.Property(e => e.UpdatedAt).HasColumnName("updated_at")
        .HasDefaultValueSql("now() AT TIME ZONE 'utc'");
});
```

**Non-PK FK relationships** — when a FK references a unique non-PK column (e.g. `system_code → systems.system_code`, `module → menus.menu_code`), add `HasPrincipalKey` so EF Core uses the right column instead of inferring `{NavProperty}Id`:

```csharp
entity.HasOne(e => e.System)
      .WithMany()
      .HasForeignKey(e => e.SystemCode)
      .HasPrincipalKey(e => e.SystemCode)   // ← required when FK targets a non-PK unique column
      .OnDelete(DeleteBehavior.Cascade);
```

Omitting `HasPrincipalKey` causes a runtime `column does not exist` error because EF generates `e.SystemId` by convention instead.

The corresponding raw SQL when creating the table in pgAdmin:
```sql
created_at  TIMESTAMPTZ NOT NULL DEFAULT (now() AT TIME ZONE 'utc'),
updated_at  TIMESTAMPTZ NOT NULL DEFAULT (now() AT TIME ZONE 'utc'),
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

### BackgroundService — scoped services via CreateScope

`BackgroundService` is a singleton. Scoped services (`DbHelper`, `AppDbContext`, `AuditDbContext`) cannot be injected into the constructor — use `IServiceProvider.CreateScope()` to resolve them per operation:

```csharp
using var scope  = _serviceProvider.CreateScope();
var myHelper     = scope.ServiceProvider.GetRequiredService<MyDbHelper>();
await myHelper.DoWorkAsync();
```

Use this pattern for EF Core–based helpers (`UserSessionDbHelper`, `PageAccessDbHelper`, etc.).

Use raw `NpgsqlConnection` only when you need to target a database that has no registered DbContext, or when bulk-deleting without loading entities first:

```csharp
using var conn = new NpgsqlConnection(_connectionString);
await conn.OpenAsync(stoppingToken);
using var cmd = new NpgsqlCommand("DELETE FROM app_logs WHERE created_at < @cutoff", conn);
cmd.Parameters.AddWithValue("@cutoff", cutoff);
await cmd.ExecuteNonQueryAsync(stoppingToken);
```

**Guard per-interval jobs with a timestamp field** — never poll blindly every tick:

```csharp
private DateTime _lastRun = DateTime.MinValue;

// Inside ExecuteAsync loop:
if (DateTime.UtcNow.Date > _lastRun.Date)   // once daily
{
    // ... do work ...
    _lastRun = DateTime.UtcNow;
}

await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken); // single sleep at the bottom
```

All background jobs live in `Services/LogCleanupService.cs` — do not create additional `BackgroundService` classes.

### File upload — profile images and attachments

Upload paths are declared in `appsettings.json` under `UploadPaths` and read via `IConfiguration["UploadPaths:Key"]`:
```json
"UploadPaths": {
  "AdminProfile": "uploads/admin-profiles"
}
```

Physical files are saved under `wwwroot/` using `ProfileImageHelper.SaveProfileImageAsync(file, username, fullPath)` in `Helper/ProfileImageHelper.cs`.

**Filename format:** `{guid}_{sanitizedUsername}{ext}`
**Allowed types:** `.jpg`, `.jpeg`, `.png` — validated server-side in `ProfileImageHelper` and client-side in JS
**Max size:** 2MB — validated server-side and client-side

**Form setup** — forms with file inputs must declare `enctype`:
```html
<form method="post" asp-page-handler="Create" enctype="multipart/form-data">
```

**PageModel binding:**
```csharp
[BindProperty] public IFormFile? fileProfileImage { get; set; }
```

**Inject into PageModel constructor** when file upload is needed:
```csharp
private readonly IWebHostEnvironment _env;
private readonly IConfiguration      _config;

// In handler:
var relPath  = _config["UploadPaths:AdminProfile"] ?? "uploads/admin-profiles";
var fullPath = Path.Combine(_env.WebRootPath, relPath.Replace('/', Path.DirectorySeparatorChar));
var filename = await ProfileImageHelper.SaveProfileImageAsync(file, username, fullPath);
```

**Display** — use the relative URL path, with fallback avatar when null:
```cshtml
@{
  var avatarSrc = string.IsNullOrEmpty(Model.ProfileImage)
      ? "/images/default-avatar.png"
      : $"/uploads/admin-profiles/{Model.ProfileImage}";
}
<img src="@avatarSrc" style="width:36px;height:36px;object-fit:cover;border-radius:50%;" />
```

**On update:** delete old physical file before saving new one.
**On soft delete:** do NOT delete the physical file.

---

### Password fields — always include show/hide toggle
Every `<input type="password">` must have a show/hide toggle button. Use a plain Bootstrap 5 `input-group` button with a Remix icon — never use `input-group-merge` (it makes the icon invisible):

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

Toggle JS (inline on the page or in a shared script):
```javascript
document.getElementById('btnTogglePwd').addEventListener('click', function () {
  var input = document.getElementById('txtPassword');
  var icon = document.getElementById('iconTogglePwd');
  if (input.type === 'password') {
    input.type = 'text';
    icon.classList.replace('ri-eye-off-line', 'ri-eye-line');
  } else {
    input.type = 'password';
    icon.classList.replace('ri-eye-line', 'ri-eye-off-line');
  }
});
```

> **Icon note:** Remix icons require both the base `ri` class AND the specific icon class (e.g. `ri ri-eye-off-line`). The base class applies the CSS mask; the specific class sets the SVG variable. Omitting `ri` renders an empty box.

If a page has multiple password fields (e.g. Password + Confirm Password), give each toggle button and icon a unique `id` (e.g. `btnTogglePwd`, `btnToggleConfirmPwd`).

### Quill rich text editor

Use the vendor-bundled Quill for any rich text / HTML body field. Always use the compiled (`*.dist`) files — `quill.js` and `editor.css` are the raw sources and must not be referenced directly:

- JS: `~/vendor/libs/quill/quill.dist.js`
- CSS: `~/vendor/libs/quill/editor.dist.css`

**Pattern:** one hidden `<input>` stores the HTML (submitted with the form), one `<div>` is the visible editor. Sync Quill → hidden input on `submit`.

```html
@* Hidden input — submitted with the form *@
<input type="hidden" name="txtBody_@lang.LanguageCode"
       id="hdnBody_@lang.LanguageCode" value="@existingHtml" />
@* Editor container *@
<div id="editor_@lang.LanguageCode" style="height:200px;"></div>
```

```html
@section VendorStyles  { <link rel="stylesheet" href="~/vendor/libs/quill/editor.dist.css" /> }
@section VendorScripts { <script src="~/vendor/libs/quill/quill.dist.js"></script> }
```

```javascript
// Init
var editor = new Quill('#editor_en', { theme: 'snow' });

// Pre-fill (Edit page)
var existing = document.getElementById('hdnBody_en').value;
if (existing) editor.clipboard.dangerouslyPasteHTML(existing);

// Sync on submit
document.querySelector('form').addEventListener('submit', function () {
  document.getElementById('hdnBody_en').value = editor.root.innerHTML;
});
```

When multiple languages are shown as Bootstrap tabs, initialise one Quill instance per language and sync all of them in the submit handler. Use a JS object (`var editors = {}`) keyed by language code.

---

### Date/time range inputs

For `datetime-local` inputs (e.g. Start At / End At on maintenance schedules):

**HTML** — use `type="datetime-local"`:
```html
<input type="datetime-local" name="txtStartAt" class="form-control" value="@Model.txtStartAt" />
```

**Populating on GET (Edit page)** — convert stored UTC to user local time using the display input format:
```csharp
txtStartAt = entity.StartAt.ToUserLocalTime(UserTimezone, AppConstants.DateTimeInputFormat);
```

**Parsing on POST** — parse then convert back to UTC:
```csharp
if (!DateTime.TryParseExact(txtStartAt, AppConstants.DateTimeInputFormat,
        null, System.Globalization.DateTimeStyles.None, out var startLocal))
{
  // validation error
}
var startUtc = startLocal.ToUtcFromUserTimezone(UserTimezone);
```

`AppConstants.DateTimeInputFormat` is `"yyyy-MM-dd HH:mm"` — the format browsers submit for `datetime-local` inputs.

---

### Index pages — always reset AlertMessageType in OnGetAsync

Every Index `OnGetAsync()` must start with `AlertMessageType = "";` to clear any TempData alert left over from a preceding redirect (e.g. after a create or delete):

```csharp
public async Task OnGetAsync()
{
  AlertMessageType = "";
  Items = await _dbHelper.GetAllAsync();
}
```

Without this, a success toast from a previous action can re-fire if the user navigates back via the browser.

### DataTables — standard pattern for all Admin listing pages

DataTables CSS and JS are loaded globally in `_CommonMasterLayout.cshtml` (CDN 1.13.6) — do **not** import them per page.

A shared initializer lives at `wwwroot/js/admin-datatable.js` and is also loaded globally.

**Every listing table must have a unique `id`:**
```html
<table id="tblThings" class="table table-hover align-middle">
```

**Call `initDataTable` in `@section PageScripts` inside `$(document).ready()`:**
```js
$(document).ready(function () {
  initDataTable('#tblThings', 3); // pass the 0-based index of the Actions column
});
```

Rules:
- Actions column is always last and its index is always passed to disable sorting
- `initDataTable` defaults: `pageLength: 25`, `order: [[0, 'asc']]`
- For tables that need non-standard options (different order direction, extra `columnDefs`), call `.DataTable({...})` directly — do not override `initDataTable`

### High-volume log/audit pages — server-side pagination, no DataTables

For tables that grow continuously (access logs, audit trails, background-job output), use **server-side pagination** instead of DataTables. DataTables loads all rows into the browser — unsuitable for millions of rows.

**Filter form** — use `method="get"` so all filter params land in the query string and the browser's Back button works:

```cshtml
<form method="get" asp-page="/PageAccessHistory/Index">
  <input type="text" asp-for="FilterUsername" class="form-control" />
  <select asp-for="FilterSystemType" asp-items="Model.ddlSystemType" class="form-select"></select>
  <input type="date" asp-for="FilterStartDate" class="form-control" />
  <button type="submit" class="btn btn-primary">@await T.GetAsync("Btn.Search")</button>
  <a asp-page="..." class="btn btn-outline-secondary">@await T.GetAsync("Btn.Clear")</a>
</form>
```

**PageModel pattern:**

```csharp
[BindProperty(SupportsGet = true)] public string? FilterUsername  { get; set; }
[BindProperty(SupportsGet = true)] public int     CurrentPage     { get; set; } = 1;

public List<MyModel> Items      { get; set; } = new();
public int           TotalCount { get; set; }
public int           PageSize   => 50;
public int           TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
```

**Pagination links** — every link must carry all current filter params to preserve the active filter across page changes:

```cshtml
<a asp-page="/Thing/Index"
   asp-route-currentPage="@p"
   asp-route-filterUsername="@Model.FilterUsername"
   asp-route-filterStartDate="@Model.FilterStartDate">
  @p
</a>
```

**Date filters** — parse with `AppConstants.DateInputFormat` (`yyyy-MM-dd`), convert to UTC via `.ToUtcFromUserTimezone(UserTimezone)`. For end date add `AddDays(1).AddSeconds(-1)` to include the full day.

**DbHelper** — return a `(List<T> Items, int TotalCount)` tuple. Apply all filters at DB level with `.Where(...)` before `.Skip().Take()`.

Do **not** call `initDataTable` on these pages.

### Index pages — Actions column dropdown

Every Admin listing page must use a Bootstrap dropdown for row actions — never inline buttons. The trigger is icon-only (`ri-more-2-fill`), no text label. Menu opens with `dropdown-menu-end` to avoid overflow. Edit is always first, Toggle Status always second.

```cshtml
<td>
  <div class="dropdown">
    <button type="button" class="btn btn-sm btn-outline-secondary dropdown-toggle"
            data-bs-toggle="dropdown" aria-expanded="false">
      <i class="ri ri-more-2-fill"></i>
    </button>
    <ul class="dropdown-menu dropdown-menu-end">
      <li>
        <a class="dropdown-item" asp-area="Admin" asp-page="/Things/Edit"
           asp-route-id="@thing.Id">
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

### AJAX JSON body handlers — non-CRUD endpoints

For operations that receive a JSON payload (e.g. batch sort/reorder), use `[FromBody]` on the parameter and set `Content-Type: application/json` in the fetch call. The antiforgery token goes in the `RequestVerificationToken` request header (not the body):

```csharp
// PageModel handler
public async Task<IActionResult> OnPostSaveSortAsync([FromBody] List<ThingSortItem> items)
{
  try
  {
    await _thingDbHelper.SaveSortOrderAsync(items, CurrentUsername);
    return new JsonResult(new { success = true });
  }
  catch
  {
    var msg = await _translation.GetAsync(MessageConstants.SaveError);
    return new JsonResult(new { success = false, message = msg });
  }
}
```

```javascript
// JS fetch
const token = document.querySelector('#formAjax input[name="__RequestVerificationToken"]').value;
const res = await fetch('?handler=SaveSort', {
  method:  'POST',
  headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
  body:    JSON.stringify(items)
});
const data = await res.json();
```

The DTO for sort items lives in `Dtos/*Dtos.cs`:
```csharp
public class ThingSortItem
{
  public int  Id        { get; set; }
  public int  SortOrder { get; set; }
  public int? ParentId  { get; set; }
  public int  Level     { get; set; }
}
```

### Row action partials — passing translated strings to JS

When a row action dropdown is shared across multiple rendering contexts (e.g. a partial that is called for both parent and child rows), extract it to a `_EntityActions.cshtml` partial. Because partials don't have access to the page's inline `pageMsg` JS object, pass all translated strings directly as function arguments from the partial's C# context:

```cshtml
@* _ThingActions.cshtml — receives "thing" (Thing) and "model" (IndexModel) via ViewData *@
@{
  var thing = (MyApp.Models.Thing)ViewData["thing"]!;
  var model = (MyApp.Areas.Admin.Pages.Things.IndexModel)ViewData["model"]!;
}

<button class="dropdown-item text-danger" type="button"
        onclick="confirmDelete(@thing.Id,
                               '@Html.Raw(model.MsgDeleteConfirmTitle)',
                               '@Html.Raw(model.MsgDeleteConfirmText)',
                               '@Html.Raw(model.MsgDeleteConfirmBtn)',
                               '@Html.Raw(model.MsgCancelBtn)')">
  <i class="ri ri-delete-bin-line me-1"></i>@model.LabelDelete
</button>
```

The `confirmDelete` JS function signature:
```javascript
async function confirmDelete(id, title, text, confirmBtn, cancelBtn) {
  const result = await Swal.fire({
    icon: 'warning', title, text,
    showCancelButton: true, confirmButtonText: confirmBtn, cancelButtonText: cancelBtn,
    confirmButtonColor: '#dc3545'
  });
  if (!result.isConfirmed) return;
  // ... fetch to ?handler=SoftDelete&id=
}
```

The IndexModel must expose all message properties (`MsgDeleteConfirmTitle`, `MsgDeleteConfirmText`, `MsgDeleteConfirmBtn`, `MsgCancelBtn`, `LabelDelete`) loaded in `OnGetAsync` via `TranslationService`, so they are available when the partial is rendered.

### Edit pages — audit fields and soft delete

Every Admin Edit page must display audit metadata below the form, and include a soft delete button.

**PageModel properties (add to every EditModel):**
```csharp
public string   CreatedBy { get; set; } = string.Empty;
public DateTime CreatedAt { get; set; }
public string   UpdatedBy { get; set; } = string.Empty;
public DateTime UpdatedAt { get; set; }
```

Populate in `OnGetAsync` directly from the loaded entity:
```csharp
CreatedBy = entity.CreatedBy;
CreatedAt = entity.CreatedAt;
UpdatedBy = entity.UpdatedBy;
UpdatedAt = entity.UpdatedAt;
```

**Audit section in `.cshtml`** — placed inside `card-body`, after `</form>`, before closing `</div>`:
```cshtml
<hr class="my-4" />
<div class="row g-3">
  <div class="col-md-6">
    <label class="form-label text-muted small">@await T.GetAsync("CreatedBy")</label>
    <p class="mb-0">@Model.CreatedBy</p>
  </div>
  <div class="col-md-6">
    <label class="form-label text-muted small">@await T.GetAsync("CreatedAt")</label>
    <p class="mb-0">@Model.CreatedAt.ToUserLocalTime(Model.UserTimezone, AppConstants.DateTimeFormat)</p>
  </div>
  <div class="col-md-6">
    <label class="form-label text-muted small">@await T.GetAsync("UpdatedBy")</label>
    <p class="mb-0">@Model.UpdatedBy</p>
  </div>
  <div class="col-md-6">
    <label class="form-label text-muted small">@await T.GetAsync("UpdatedAt")</label>
    <p class="mb-0">@Model.UpdatedAt.ToUserLocalTime(Model.UserTimezone, AppConstants.DateTimeFormat)</p>
  </div>
</div>
```

Dates must always use `.ToUserLocalTime(Model.UserTimezone, AppConstants.DateTimeFormat)` — never render raw `DateTime` values.

**Soft delete** — see the `pageMsg` pattern below; every Edit page also needs a Delete button and `OnPostSoftDeleteAsync`.

### Edit pages — pageMsg JS object and soft delete

Every Admin Edit page must use the `pageMsg` JS object to pass all translated strings to JavaScript, and must include a soft delete button wired to `OnPostSoftDeleteAsync`.

**PageModel properties:**
```csharp
public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
public string MsgDeleteConfirmText  { get; set; } = string.Empty;
public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
public string MsgCancelBtn          { get; set; } = string.Empty;
public string MsgDeleteSuccess      { get; set; } = string.Empty;
public string MsgDeleteError        { get; set; } = string.Empty;
public string LabelDelete           { get; set; } = string.Empty;
```

Load in `OnGetAsync`:
```csharp
var entityName        = await _translation.GetAsync("Menu.<Entity>");
MsgDeleteConfirmTitle = $"{await _translation.GetAsync("Confirm.DeleteTitle")} {entityName}";
MsgDeleteConfirmText  = await _translation.GetAsync("Confirm.DeleteText");
MsgDeleteConfirmBtn   = await _translation.GetAsync("Btn.YesDelete");
MsgCancelBtn          = await _translation.GetAsync("Btn.Cancel");
MsgDeleteSuccess      = await _translation.GetAsync(MessageConstants.DeleteSuccess);
MsgDeleteError        = await _translation.GetAsync(MessageConstants.DeleteError);
LabelDelete           = await _translation.GetAsync("Btn.Delete");
```

**Handler:**
```csharp
public async Task<IActionResult> OnPostSoftDeleteAsync(string entityCode)
{
  try
  {
    await _thingDbHelper.UpdateStatusAsync(entityCode, StatusConstants.Deleted, CurrentUsername);
    var msg = await _translation.GetAsync(MessageConstants.DeleteSuccess);
    return new JsonResult(new { success = true, message = msg });
  }
  catch
  {
    var msg = await _translation.GetAsync(MessageConstants.DeleteError);
    return new JsonResult(new { success = false, message = msg });
  }
}
```

**Delete button** — inside the form's button row, pushed right with `ms-auto`:
```cshtml
<a href="#" id="btnDelete" class="btn btn-outline-danger ms-auto" data-code="@Model.EntityCode">
  <i class="ri ri-delete-bin-line me-1"></i>@Model.LabelDelete
</a>
```

**Hidden antiforgery form** — placed outside the main form (before `@section VendorScripts`):
```cshtml
@* Hidden form supplies antiforgery token for AJAX calls *@
<form id="formAjax" method="post">
  @Html.AntiForgeryToken()
</form>
```

**`pageMsg` object and AJAX in `@section PageScripts`:**
```cshtml
@section PageScripts {
<script>
  var pageMsg = {
    deleteConfirmTitle: '@Html.Raw(Model.MsgDeleteConfirmTitle)',
    deleteConfirmText:  '@Html.Raw(Model.MsgDeleteConfirmText)',
    deleteConfirmBtn:   '@Html.Raw(Model.MsgDeleteConfirmBtn)',
    cancelBtn:          '@Html.Raw(Model.MsgCancelBtn)',
    deleteSuccess:      '@Html.Raw(Model.MsgDeleteSuccess)',
    deleteError:        '@Html.Raw(Model.MsgDeleteError)'
  };

  document.getElementById('btnDelete').addEventListener('click', function (e) {
    e.preventDefault();
    var code = this.getAttribute('data-code');
    Swal.fire({
      title: pageMsg.deleteConfirmTitle,
      text: pageMsg.deleteConfirmText,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: pageMsg.deleteConfirmBtn,
      cancelButtonText: pageMsg.cancelBtn
    }).then(function (result) {
      if (result.isConfirmed) {
        $.post('?handler=SoftDelete&entityCode=' + encodeURIComponent(code), {
          __RequestVerificationToken: $('#formAjax input[name="__RequestVerificationToken"]').val()
        }, function (data) {
          if (data.success) {
            Swal.fire({ icon: 'success', title: pageMsg.deleteSuccess, timer: 1500, showConfirmButton: false })
              .then(function () { window.location.href = '@Url.Page(Routes.AdminXxx)'; });
          } else {
            Swal.fire({ icon: 'error', text: data.message || pageMsg.deleteError });
          }
        }).fail(function () {
          Swal.fire({ icon: 'error', text: pageMsg.deleteError });
        });
      }
    });
  });
</script>
}
```

### Sidebar navigation menu

The sidebar is rendered by `Areas/Admin/Pages/Layouts/Sections/Menu/_VerticalMenu.cshtml`, included from `_ContentNavbarLayout.cshtml`. It calls `MenuDbHelper.GetNavMenuAsync(roleId, isSuperAdmin)`.

**Menu level convention:**

| `level` | Role | Has URL? | Example |
|---|---|---|---|
| 0 | Group header (non-clickable label) | No | "Administration" |
| 1 | Module (collapsible, top-level) | No | "Master Data" |
| 2 | Sub module OR function | Only if leaf | "General Setup", "Countries" |

The sidebar renders up to 3 collapsible levels. A Level 2 item with children renders as a collapsible sub-module; without children it renders as a leaf link.

**Menu URL format — always store as the actual URL path:**
```
/Admin/Countries     ← correct (plural, matches Razor Pages folder)
/Admin/Country       ← wrong (singular)
/Admin/Countries/Index  ← wrong (don't include /Index)
```
Never use `Url.Page()` for sidebar links — store and render the URL directly as `href`.

**`IsActive` detection uses `Context.Request.Path.Value`:**
```csharp
string? current = Context.Request.Path.Value;  // e.g. "/Admin/Countries"

bool IsActive(string? url) =>
    !string.IsNullOrEmpty(url) &&
    (current == url || current?.StartsWith(url + "/") == true);
```
Do not use `ViewContext.RouteData.Values["Page"]` — that returns Razor Pages path format (`/Countries/Index`) which does not match stored URL paths.

**Role-based filtering — `GetNavMenuAsync(int roleId, bool isSuperAdmin)`:**
- SuperAdmin (`IsSuperAdmin = true`): returns all active menus
- Other roles: returns only menus assigned in `role_menus` for the role, with empty parent branches pruned
- Uses flat DB load + in-memory tree build (NOT EF Core filtered Include/ThenInclude, which silently returns empty `Children` on self-referencing entities)

**Login claims — `RoleId` and `IsSuperAdmin` are stored in the auth cookie:**
```csharp
new Claim(CookieConstants.SessionKeys.RoleId,       adminUser.RoleId.ToString()),
new Claim(CookieConstants.SessionKeys.IsSuperAdmin, (adminUser.Role?.IsSuperAdmin ?? false) ? "true" : "false")
```
`AdminDbHelper.GetByUsernameAsync` must `.Include(a => a.Role)` so `IsSuperAdmin` is available at login time.

**`AdminPageModel` helpers:**
```csharp
public int  CurrentRoleId      => int.TryParse(User.FindFirstValue(CookieConstants.SessionKeys.RoleId), out var id) ? id : 0;
public bool CurrentIsSuperAdmin => User.FindFirstValue(CookieConstants.SessionKeys.IsSuperAdmin) == "true";
```

### Translation-enabled entities (entity + `*Translation` table)

Some entities pair a main table with a `*Translation` child table (e.g. `payment_methods` + `payment_method_translations`, `countries` + `country_translations`). Follow these rules for every such entity:

**DbContext — cascade delete on the `HasMany` side:**
```csharp
entity.HasMany(e => e.Translations)
      .WithOne(t => t.PaymentMethod)
      .HasForeignKey(t => t.PaymentCode)
      .OnDelete(DeleteBehavior.Cascade);
```
Configure the relationship only once, on the parent entity block. The child entity block maps columns and the FK but does not repeat `.WithMany`.

**DbHelper — `GetAllAsync` and `GetAllActiveAsync` always take `string languageCode`:**
```csharp
public async Task<List<PaymentMethod>> GetAllAsync(string languageCode)
    => await ExecuteAsync(async () =>
    {
      var items = await _db.PaymentMethods
          .Where(p => p.Status != StatusConstants.Deleted)
          .Include(p => p.Translations.Where(t => t.LanguageCode == languageCode))
          .ToListAsync();
      return items
          .OrderBy(p => p.Translations.FirstOrDefault()?.PaymentName ?? p.PaymentCode)
          .ToList();
    });
```

**Index PageModel — use `CurrentLangCode`, never hardcode `"en"`:**
```csharp
public async Task OnGetAsync()
{
  AlertMessageType = "";
  var langCode = string.IsNullOrEmpty(CurrentLangCode) ? "en" : CurrentLangCode;
  Items = await _dbHelper.GetAllAsync(langCode);
}
```

**`*AddResult` enum — lives in `Dtos/*Dtos.cs`, not inline in the DbHelper:**
```csharp
// Dtos/PaymentMethodDtos.cs
public enum PaymentMethodAddResult { Created, Restored, DuplicateActive }
```

**`BuildInputsAsync` — private helper used on both GET and POST-error paths in Create/Edit PageModels:**
```csharp
// Pass existing translations on GET; pass null to re-read from Request.Form on POST error
private async Task<List<TranslationInputDto>> BuildInputsAsync(IList<PaymentMethodTranslation>? existing)
{
  var languages   = await _languageDbHelper.GetAllActiveAsync();
  var placeholder = await _translation.GetAsync("Entity.NamePlaceholder");
  return languages.Select(l => new TranslationInputDto
  {
    LanguageCode = l.LanguageCode,
    Label        = l.LanguageName,
    Value        = existing != null
        ? existing.FirstOrDefault(t => t.LanguageCode == l.LanguageCode)?.Name ?? string.Empty
        : Request.Form[$"txtName_{l.LanguageCode}"].ToString(),
    Placeholder  = placeholder
  }).ToList();
}
```

**Edit page entity name for delete confirm title** — use the English translation, fall back to the code:
```csharp
var entityName = entity.Translations.FirstOrDefault(t => t.LanguageCode == "en")?.PaymentName ?? paymentCode;
MsgDeleteConfirmTitle = $"{await _translation.GetAsync("Confirm.DeleteTitle")} {entityName}";
```

### Localization — every user-facing string in .cshtml must use `T.GetAsync`

This applies to **all** visible text — no exceptions:

- Page titles, card headings, section headings, subtitle/hint text
- Form `<label>` elements and `placeholder` attributes
- Button and link text
- Table headers (`<th>`)
- Badge and status labels
- Modal titles
- SweetAlert message text

**Inline HTML** — use `@await T.GetAsync("key")` directly:
```cshtml
<label class="form-label">@await T.GetAsync("FieldName")</label>
<input placeholder="@await T.GetAsync("FieldName.Placeholder")" />
<th>@await T.GetAsync("ColumnHeader")</th>
<h5 class="mb-0">@await T.GetAsync("Section.Title")</h5>
<small class="text-muted">@await T.GetAsync("Section.Subtitle")</small>
```

**JavaScript strings** (chart series names, axis titles, tooltip text, SweetAlert messages) must be pre-declared as Razor variables in a top-level `@{ }` block, then referenced with `'@varName'` inside script:
```cshtml
@{
  var lblOrders      = await T.GetAsync("Orders");
  var lblToggleTitle = await T.GetAsync("Thing.ToggleStatusTitle");
}

@section PageScripts {
<script>
  series: [{ name: '@lblOrders', data: ordersData }]
  var msgTitle = '@lblToggleTitle';
</script>
}
```

**C# (PageModels and services):**
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
UseMiddleware<MaintenanceMiddleware>      ← kicks out non-SuperAdmin when maintenance is active
UseMiddleware<SessionTrackingMiddleware>  ← logs page access and updates session
MapRazorPages
```

---

## Localization

Supported cultures: `en` (English), `zh-Hans` (Simplified Chinese). Language stored in `lang` cookie.  
All translations live in the `language_resources` DB table — never hard-code user-facing strings.  
Use `MessageConstants.*` keys when calling `TranslationService.GetAsync(key)`.

### Language selector on Login page
The login page renders a `<select>` populated from the `languages` DB table (active records only). On change it writes both the ASP.NET Core culture cookie and the `lang` cookie, then reloads the page:

```html
<div class="input-group">
  <label class="input-group-text" for="inputGroupSelect01">@await T.GetAsync("Language")</label>
  <select class="form-select" id="inputGroupSelect01" asp-for="ddlLanguage" onchange="changeCulture(this.value)">
    @foreach (var lang in Model.Languages)
    {
      <option value="@lang.LanguageCode" selected="@(lang.LanguageCode == currentCulture ? "selected" : null)">
        @lang.NativeName
      </option>
    }
  </select>
</div>
```

```javascript
function changeCulture(culture) {
  document.cookie = ".AspNetCore.Culture=c=" + culture + "|uic=" + culture + "; path=/";
  document.cookie = "lang=" + culture + "; path=/";
  window.location.href = window.location.pathname;
}
```

The `currentCulture` variable comes from `CultureInfo.CurrentUICulture.TwoLetterISOLanguageName` declared in a Razor `@{ }` block at the top of the page. The PageModel exposes `List<Language> Languages` loaded via `LanguageDbHelper.GetAllActiveAsync()`.

---

## Razor (.cshtml) Comments

Always use Razor comment syntax in `.cshtml` files — never HTML comments for code notes, and never Handlebars syntax:

```cshtml
@* This is a Razor comment — not rendered in HTML output *@

@* ── Section label ────────────────────────────────────── *@
```

| Syntax | Renders in HTML? | Use for |
|---|---|---|
| `@* ... *@` | No | All comments in `.cshtml` |
| `<!-- ... -->` | Yes | Intentional HTML comments only |
| `{{!-- ... --}}` | Yes (as plain text) | Never — this is Handlebars, not Razor |

---

## Always

- Use `async/await` for all database and I/O operations
- Use `DateTime.UtcNow` for all timestamp fields (`created_at`, `updated_at`, etc.)
- In `CreateAsync`, always explicitly assign **all** entity fields plus set `CreatedAt = DateTime.UtcNow`, `CreatedBy = createdBy`, `UpdatedAt = DateTime.UtcNow`, `UpdatedBy = createdBy` — use an object initializer so nothing is silently omitted
- In `UpdateAsync`, accept a model object (not individual field parameters) as the first argument, fetch the tracked record inside the helper, then assign from the passed object field by field — never call `_db.Update(entity)` directly on the detached input
- Always set `HasDefaultValueSql("now() AT TIME ZONE 'utc'")` on every `created_at` / `updated_at` column in `AppDbContext` — and use `TIMESTAMPTZ NOT NULL DEFAULT (now() AT TIME ZONE 'utc')` in the raw SQL CREATE TABLE script
- Map DB columns explicitly with `.HasColumnName("snake_case")` in `AppDbContext`
- Keep Models as plain POCOs — no methods, no business logic
- Put all EF queries in `DbHelper` subclasses, not in PageModels or Services — use `AdminDbHelper` for admin entities, `CustomerDbHelper` for customer entities
- Register new DbHelpers and Services as `Scoped` in `Program.cs`
- Use `string.Empty` as default for string properties in models
- Use `Constants/` classes for any magic strings or numbers used in multiple places
- Use `DbSet<T>` with expression-bodied property syntax: `public DbSet<Thing> Things => Set<Thing>();`
- Create a new `*DbHelper` class per entity group (one for Admin, one for Customer, etc.)
- After model changes, provide a raw SQL script for pgAdmin — do not suggest `dotnet ef migrations`. Save all generated `.sql` files to `D:\CRMCore\Script\` (not the project root)
- Use soft delete — set `Status = StatusConstants.Deleted` instead of physically deleting records, unless explicitly told otherwise
- Use `long` (C#) / `BIGSERIAL` (SQL) for PKs on high-volume tables (access logs, audit trails, job output) — `int`/`SERIAL` overflows at ~2 billion rows
- When a middleware's `InvokeAsync` needs a scoped service (e.g. `PageAccessDbHelper`), inject it as a method parameter — not via the constructor. Constructor injection in middleware produces a singleton-scoped instance which breaks scoped EF Core contexts:

```csharp
// Correct — method injection
public async Task InvokeAsync(HttpContext context, PageAccessDbHelper pageAccessDbHelper) { ... }

// Wrong — constructor injection creates a singleton instance
public MyMiddleware(RequestDelegate next, PageAccessDbHelper db) { ... }
```
- All `<select>` element names and `[BindProperty]` properties for dropdowns use `ddl` prefix — e.g. `ddlLanguage`, `ddlStatus`
- All UI-facing strings go through `TranslationService.GetAsync(key)` — no hardcoded strings in `.cshtml` or PageModels
- Use SweetAlert2 for all alert/notification messages — vendor file at `~/vendor/libs/sweetalert2/sweetalert2.dist.js`, never use `alert()` or inline Bootstrap alerts
- New pages follow the folder structure: admin pages under `Areas/Admin/Pages/`, customer pages under `Areas/Customer/Pages/`
- All protected admin pages inherit from `AdminPageModel`; all protected customer pages inherit from `CustomerPageModel` — never use a bare `[Authorize]` attribute on individual pages
- All DbHelper methods must wrap their body in `ExecuteAsync(...)` — never call `_db.*` directly in a DbHelper method
- Use named handlers for CRUD: `OnPostCreateAsync`, `OnPostUpdateAsync`, `OnPostDeleteAsync` with matching `asp-page-handler` on form buttons
- After generating any admin CRUD module (Index + Create + Edit pages), immediately output a `language_resources` SQL upsert covering every `T.GetAsync(...)` key used across all new `.cshtml` files — page titles, labels, placeholders, hints, button text, column headers, and any module-specific keys. Format: `INSERT INTO language_resources (language_code, key, value) VALUES (...) ON CONFLICT (language_code, key) DO UPDATE SET value = EXCLUDED.value;`. Cover both `en` and `zh-Hans` rows.

## Never

- Never use Razor reserved keywords as variable names in `.cshtml` files — `section`, `functions`, `namespace`, `page`, `model`, `inherits`, `helper` are all reserved. Using any of these as a `@foreach` loop variable (e.g. `var section in Model.Items`) causes Razor to misparse `@section.Property` as a malformed directive, producing "The 'section' directive must appear at the start of the line" errors. Use descriptive alternatives: `stype` instead of `section`, `func` → `funcItem`, etc.
- Never add MVC controllers — this project is Razor Pages only
- Never query `AppDbContext` directly inside a PageModel — use a `DbHelper`
- Never hard-code user-facing strings — use `TranslationService` + `MessageConstants`
- Never store passwords as plain text — use `PasswordCryptoHelper` (AES) for now
- Never use `AddTransient` for `DbHelper` or `Service` classes — use `AddScoped`
- Never put business logic inside Model classes
- Never edit files under `wwwroot/vendor/` — those are third-party libs
- Never edit `*.dist.js` or `*.dist.css` files directly — edit the source and rebuild via Webpack/Gulp
- Never skip `UseAuthentication` / `UseAuthorization` in the pipeline
- Never physically delete records — always soft delete via `Status = StatusConstants.Deleted`
- Never reorder the middleware pipeline without understanding the dependencies
- Never inject `AppDbContext` or any scoped `DbHelper` into a `BackgroundService` constructor — use `IServiceProvider.CreateScope()` inside the job body instead
- Never use `[Authorize]` directly on a page model — use `AdminPageModel` or `CustomerPageModel` as the base class
- Never commit real SMTP passwords or connection strings — move secrets to `appsettings.Development.json` or User Secrets
- Never assume `permissions.menu_id` — the actual column is `module` (varchar), which stores `menus.menu_code`. The EF relationship is `HasForeignKey(p => p.Module).HasPrincipalKey(m => m.MenuCode)`. Adding a `MenuId` (int) property to `Permission` will produce a "column does not exist" runtime error.
