# CLAUDE.md — Project Guidelines for MRMR

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
- **GitHub Repository:** https://github.com/avenueconcept-pixel/MRMR

---

## Architecture Pattern

This project uses **ASP.NET Core Razor Pages** — there are no MVC controllers.

```
PageModel (.cshtml.cs)
  └── DbHelper (Helper/DB/)      ← repository-style EF Core queries
        └── AppDbContext          ← main EF Core DbContext (PostgreSQL)
        └── AuditDbContext        ← audit EF Core DbContext (mrmr_audit DB)
  └── Service (Services/)        ← cross-cutting logic (email, translation)
```

**Two DbContexts:** `AppDbContext` is the main application database. `AuditDbContext` targets the separate `mrmr_audit` database and holds audit/session/access-log tables (`audit_logs`, `user_sessions`, `page_access_history`, `page_access_history_archive`). Both are registered with their own connection strings in `Program.cs`.

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

**Why:** A folder named `Department/` produces namespace `MyApp.Areas.Admin.Pages.Department`, making `Department` refer to the namespace rather than `MyApp.Models.Department`. Using the plural form avoids the collision entirely.

**Exception — compound names identical to model class:** resolve with a using alias at the top of every `.cshtml.cs` in that folder:
```csharp
using PageAccessHistoryModel = MyApp.Models.PageAccessHistory;
```

### Database
| Thing | Convention | Example |
|---|---|---|
| Table names | snake_case | `admin_users`, `language_resources` |
| Column names | snake_case, explicit via Fluent API | `full_name`, `is_active`, `created_at` |
| Identity PKs | `.UseIdentityColumn()` | `id` |

---

## Coding Patterns

### CRUD handler naming
Use named handlers — never overload a single `OnPost`:

```csharp
public async Task<IActionResult> OnPostCreateAsync() { ... }
public async Task<IActionResult> OnPostUpdateAsync() { ... }
public async Task<IActionResult> OnPostDeleteAsync() { ... }
```

Wire up with `asp-page-handler="Create"`, `asp-page-handler="Update"`, `asp-page-handler="Delete"`.

### DbHelper pattern — two variants

**Variant A — extends `DbHelper` base (standard, for `AppDbContext` entities):**

Every method must wrap its operation in `ExecuteAsync`:

```csharp
public class ThingDbHelper : DbHelper
{
  public ThingDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory) : base(db, audit, loggerFactory) { }

  public async Task<Thing?> GetByIdAsync(int id)
      => await ExecuteAsync(async () => await _db.Things.FindAsync(id));

  public async Task DeleteAsync(int id)
      => await ExecuteAsync(async () =>
      {
        var thing = await _db.Things.FindAsync(id);
        if (thing != null) { thing.Status = StatusConstants.Deleted; await _db.SaveChangesAsync(); }
      });
}
```

**DbHelper method naming — use generic, entity-agnostic names within each class:**

| Method | Not |
|---|---|
| `GetAllAsync` | `GetAllThingsAsync` |
| `GetByIdAsync` | `GetThingByIdAsync` |
| `AddAsync` | `AddThingAsync` |
| `UpdateAsync` | `UpdateThingAsync` |
| `DeleteAsync` | `DeleteThingAsync` |

**Variant B — standalone (for `AuditDbContext`-only helpers):**

Do **not** extend the base `DbHelper`. Write standalone with own `ExecuteAsync` wrappers — see `UserSessionDbHelper` and `PageAccessDbHelper` for the full boilerplate. Inject `AuditDbContext` + `ILoggerFactory` directly.

Register in `Program.cs`:
```csharp
builder.Services.AddScoped<ThingDbHelper>();
```

**`UpdateAsync` signature — always accept a model object, never individual field parameters:**

```csharp
public async Task UpdateAsync(Thing thing, string updatedBy) { ... }
```

Inside `UpdateAsync`, always fetch the tracked entity first, then assign from the passed object — never call `_db.Update()` on the detached input directly.

**When adding, renaming, or removing a field on any Model or PageModel, always check the corresponding DbHelper and update every affected method** — missing a field causes silent data loss.

### Models — plain POCOs, no logic
```csharp
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

Timestamp columns must always default to UTC:

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

**Non-PK FK relationships** — when a FK references a unique non-PK column, add `HasPrincipalKey`:

```csharp
entity.HasOne(e => e.System)
      .WithMany()
      .HasForeignKey(e => e.SystemCode)
      .HasPrincipalKey(e => e.SystemCode)   // ← required when FK targets a non-PK unique column
      .OnDelete(DeleteBehavior.Cascade);
```

Omitting `HasPrincipalKey` causes a runtime `column does not exist` error.

Raw SQL timestamp columns:
```sql
created_at  TIMESTAMPTZ NOT NULL DEFAULT (now() AT TIME ZONE 'utc'),
updated_at  TIMESTAMPTZ NOT NULL DEFAULT (now() AT TIME ZONE 'utc'),
```

### Services — inject via constructor
Inject `AppDbContext`, `IMemoryCache`, `IHttpContextAccessor` as needed. Register as `Scoped` in `Program.cs`.

### Constants — static classes, `const string` only
```csharp
public static class ThingConstants
{
  public const string Active = "active";
}
```

### Page authorization — inherit from area base model
```csharp
public class IndexModel : AdminPageModel { ... }        // admin area
public class DashboardModel : CustomerPageModel { ... } // customer area
public class DashboardModel : ApplicantPageModel { ... }// applicant portal (auth required)
```

`ApplicantPageModel` lives in `Helper/ApplicantPageModel.cs` and applies `[Authorize(AuthenticationSchemes = AuthSchemeConstants.Applicant)]`. Use it for all applicant portal pages that require login.

Public pages (Login, ForgotPassword, ResetPassword) inherit from `BasePageModel` or `PageModel` directly. `BasePageModel` exposes `AlertMessageContent`, `AlertMessageType`, and `AlertMessageTitle` — **not** `AlertMessage`.

### Applicant area layouts

`_ViewStart.cshtml` in the Applicant area defaults to `_PublicLayout`. Portal pages (post-login) must override this:
```cshtml
@{
    Layout = "Layouts/_PortalLayout";
}
```

### PageModel result helpers — no Controller shorthand

`PageModel` is **not** a controller. `Ok()`, `BadRequest()`, and `StatusCode()` do not exist. Use the result types directly:
```csharp
return new OkResult();
return new BadRequestObjectResult("message");
return new StatusCodeResult(500);
return new JsonResult(new { success = true });
```

### BackgroundService — scoped services via CreateScope

Use `IServiceProvider.CreateScope()` to resolve scoped services per operation:
```csharp
using var scope  = _serviceProvider.CreateScope();
var myHelper     = scope.ServiceProvider.GetRequiredService<MyDbHelper>();
await myHelper.DoWorkAsync();
```

Use raw `NpgsqlConnection` only when targeting a DB with no registered DbContext, or for bulk deletes without loading entities.

**Guard per-interval jobs with a timestamp field:**
```csharp
private DateTime _lastRun = DateTime.MinValue;
// Inside loop:
if (DateTime.UtcNow.Date > _lastRun.Date) { /* do work */ _lastRun = DateTime.UtcNow; }
await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
```

All background jobs live in `Services/LogCleanupService.cs` — do not create additional `BackgroundService` classes.

### MsSqlHelper — read-only secondary SQL Server connections

Subclass `MsSqlHelper` in `Helper/DB/`. Use raw `SqlConnection` — not EF Core:

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

- Connection string key: `MsSqlConnection` in `appsettings.json`
- Register as Scoped. `OpenAsync()` and `ExecuteAsync` overloads are provided by base class.
- Placeholder `PaymentStatusMsSqlHelper` exists in `Helper/DB/` — extend when Order module is built.

### SystemSettingService — typed access to system_settings

Inject wherever a configurable parameter is needed:
```csharp
var maxAmount  = await _settingService.GetAsDecimalAsync("Wallet.MaxAdjustmentAmount", 10000);
var retryLimit = await _settingService.GetAsIntAsync("Wallet.PayoutRetryLimit", 3);
var isEnabled  = await _settingService.GetAsBoolAsync("Feature.SomeFlag", false);
var rankCode   = await _settingService.GetAsync("Member.DefaultRankCode");
_settingService.ClearCache(); // call after any admin update
```

Methods: `GetAsync`, `GetAsIntAsync`, `GetAsDecimalAsync`, `GetAsBoolAsync`, `ClearCache()`. Cache TTL is 1 hour.

### File upload — profile images and attachments

- Upload paths declared in `appsettings.json` under `UploadPaths`, read via `IConfiguration["UploadPaths:Key"]`
- Save via `ProfileImageHelper.SaveProfileImageAsync(file, username, fullPath)`
- **Filename format:** `{guid}_{sanitizedUsername}{ext}`
- **Allowed types:** `.jpg`, `.jpeg`, `.png` — validated server-side + client-side
- **Max size:** 2MB — validated server-side + client-side
- Forms with file inputs must declare `enctype="multipart/form-data"`
- `[BindProperty] public IFormFile? fileProfileImage { get; set; }`
- Display with fallback: `string.IsNullOrEmpty(Model.ProfileImage) ? "/images/default-avatar.png" : $"/uploads/admin-profiles/{Model.ProfileImage}"`
- **On update:** delete old physical file before saving new one
- **On soft delete:** do NOT delete the physical file

---

### Password fields — always include show/hide toggle
Every `<input type="password">` must have a show/hide toggle button. Use a plain Bootstrap 5 `input-group` button with a Remix icon — never use `input-group-merge`:

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

Toggle JS: on click, toggle `input.type` between `password`/`text` and swap `ri-eye-off-line`/`ri-eye-line` on the icon.

> **Icon note:** Remix icons require both the base `ri` class AND the specific icon class (e.g. `ri ri-eye-off-line`). Omitting `ri` renders an empty box.

If a page has multiple password fields, give each toggle button and icon a unique `id`.

### Quill rich text editor

Load compiled files only — never reference raw sources:
- JS: `~/vendor/libs/quill/quill.dist.js`
- CSS: `~/vendor/libs/quill/editor.dist.css`

**Pattern:** one hidden `<input>` stores HTML (submitted with form), one `<div>` is the visible editor. Sync Quill → hidden input on `submit`.

```html
<input type="hidden" name="txtBody_@lang.LanguageCode" id="hdnBody_@lang.LanguageCode" value="@existingHtml" />
<div id="editor_@lang.LanguageCode" style="height:200px;"></div>
```

```javascript
var editor = new Quill('#editor_en', { theme: 'snow' });
// Pre-fill on Edit:
editor.clipboard.dangerouslyPasteHTML(document.getElementById('hdnBody_en').value);
// Sync on submit:
document.querySelector('form').addEventListener('submit', function () {
  document.getElementById('hdnBody_en').value = editor.root.innerHTML;
});
```

When multiple languages are shown as Bootstrap tabs, initialise one Quill per language (`var editors = {}` keyed by language code) and sync all in the submit handler.

**Multiple Quill instances across multiple forms:** key editors by unique element ID, scope sync to the submitting form using `form.contains(hdn)`. Naming convention: `hdn` prefix ↔ `editor` prefix with same suffix (e.g. `hdnNew_en` / `editorNew_en`).

---

### Date/time range inputs

- Use `type="datetime-local"` inputs
- **Populating on GET:** convert UTC → local with `entity.StartAt.ToUserLocalTime(UserTimezone, AppConstants.DateTimeInputFormat)`
- **Parsing on POST:** `DateTime.TryParseExact(txtStartAt, AppConstants.DateTimeInputFormat, ...)` then `.ToUtcFromUserTimezone(UserTimezone)`
- `AppConstants.DateTimeInputFormat` = `"yyyy-MM-dd HH:mm"`

---

### Index pages — always reset AlertMessageType in OnGetAsync

```csharp
public async Task OnGetAsync()
{
  AlertMessageType = "";
  Items = await _dbHelper.GetAllAsync();
}
```

Without this, a success toast from a previous action can re-fire on browser back-navigation.

### DataTables — standard pattern for all Admin listing pages

DataTables CSS/JS (CDN 1.13.6) and `wwwroot/js/admin-datatable.js` are loaded globally — do **not** import per page.

Every listing table must have a unique `id`. Call `initDataTable` in `@section PageScripts`:
```js
$(document).ready(function () {
  initDataTable('#tblThings', 3); // 3 = 0-based index of Actions column
});
```

Rules:
- Actions column is always last; its index is always passed to disable sorting
- Defaults: `pageLength: 25`, `order: [[0, 'asc']]`
- For non-standard options, call `.DataTable({...})` directly

**Row button event binding — never use inline `onclick` on DataTable rows.** DataTables rebuilds the DOM on sort/page/search. Use `data-*` attributes + delegated jQuery handlers:

```html
<button class="btn btn-sm btn-outline-primary btn-edit-thing"
        data-code="@thing.ThingCode" data-name="@thing.ThingName">
  <i class="ri ri-edit-line me-1"></i>@await T.GetAsync("Edit")
</button>
```
```javascript
$(document).on('click', '.btn-edit-thing', function () {
  openEditModal($(this).data('code'), $(this).data('name'));
});
```

Exception: `onclick` is acceptable for guaranteed safe identifiers with no user content (e.g. admin-entered uppercase alphanumeric).

### Index pages with server-side pre-filters + DataTables

Combine server-side filtering with DataTables client-side search/sort/page:
- Filter form uses `method="get"` — selected filters land in the query string
- Filter value properties use `Filter*` prefix and `[BindProperty(SupportsGet = true)]`
- Option list properties use `ddl*` prefix
- `initDataTable` is still called on the server-filtered subset
- "Clear" link points to the page with no query-string params
- Filter dropdowns include an "All" option with `Value = string.Empty` as first item

### High-volume log/audit pages — server-side pagination, no DataTables

Use server-side pagination for continuously-growing tables. Filter form uses `method="get"`. PageModel:
```csharp
[BindProperty(SupportsGet = true)] public string? FilterUsername { get; set; }
[BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
public int PageSize  => 50;
public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
```

Pagination links must carry all current filter params. Date filters: parse with `AppConstants.DateInputFormat` (`yyyy-MM-dd`), convert to UTC. For end date add `AddDays(1).AddSeconds(-1)`. DbHelper returns `(List<T> Items, int TotalCount)` tuple with filters applied at DB level before `.Skip().Take()`. Do **not** call `initDataTable` on these pages.

### Index pages — Actions column dropdown

Every Admin listing page must use a Bootstrap dropdown for row actions — never inline buttons. Trigger is icon-only (`ri-more-2-fill`). Menu opens with `dropdown-menu-end`. Edit is always first, Toggle Status always second.

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

### AJAX JSON body handlers — non-CRUD endpoints

For operations receiving a JSON payload, use `[FromBody]` and set `Content-Type: application/json`. Antiforgery token goes in `RequestVerificationToken` header:

```csharp
public async Task<IActionResult> OnPostSaveSortAsync([FromBody] List<ThingSortItem> items)
{
  try { await _thingDbHelper.SaveSortOrderAsync(items, CurrentUsername); return new JsonResult(new { success = true }); }
  catch { return new JsonResult(new { success = false, message = await _translation.GetAsync(MessageConstants.SaveError) }); }
}
```

```javascript
const res = await fetch('?handler=SaveSort', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
  body: JSON.stringify(items)
});
```

Sort DTO in `Dtos/*Dtos.cs`:
```csharp
public class ThingSortItem { public int Id { get; set; } public int SortOrder { get; set; } }
```

### SortableJS drag-to-reorder pattern

Load from CDN in `@section VendorScripts`: `https://cdnjs.cloudflare.com/ajax/libs/Sortable/1.15.2/Sortable.min.js`

Each draggable item must have `data-id="@item.Id"`. On drag end, collect ordered IDs and fire an AJAX JSON POST using the `[FromBody]` handler pattern:

```javascript
new Sortable(document.getElementById('myList'), {
  handle: '.ri-drag-move-line', animation: 150,
  onEnd: function () {
    var items = Array.from(document.querySelectorAll('#myList [data-id]')).map(function (el, i) {
      return { id: parseInt(el.dataset.id), sortOrder: i + 1 };
    });
    fetch('?handler=SaveMySort', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': document.querySelector('#formAjax input[name="__RequestVerificationToken"]').value },
      body: JSON.stringify(items)
    });
  }
});
```

### Non-`[BindProperty]` read-only page properties alongside BindProperty fields

When a value set in `OnGetAsync` must control view logic but must **not** be overwritten by unrelated POST handlers, declare it as a plain page property — not `[BindProperty]`.

```csharp
public string ProductType { get; set; } = string.Empty;           // read-only mirror — drives @if in view
[BindProperty] public string ddlProductType { get; set; } = string.Empty;  // round-trips through Update form
```

Set both in `OnGetAsync` and every `ReloadPageAsync` path. Use the plain property in `.cshtml` conditionals.

### Row action partials — passing translated strings to JS

When a row action dropdown is shared across multiple contexts, extract to `_EntityActions.cshtml`. Pass translated strings as function arguments from the partial's C# context — partials don't have access to the page's `pageMsg` JS object:

```cshtml
<button class="dropdown-item text-danger" type="button"
        onclick="confirmDelete(@thing.Id,
                               '@Html.Raw(model.MsgDeleteConfirmTitle)',
                               '@Html.Raw(model.MsgDeleteConfirmText)',
                               '@Html.Raw(model.MsgDeleteConfirmBtn)',
                               '@Html.Raw(model.MsgCancelBtn)')">
  <i class="ri ri-delete-bin-line me-1"></i>@model.LabelDelete
</button>
```

The IndexModel must expose all message properties loaded in `OnGetAsync` via `TranslationService`.

### Edit pages — audit fields and soft delete

Every Admin Edit page must display audit metadata below the form and include a soft delete button.

**PageModel properties:** `CreatedBy`, `CreatedAt`, `UpdatedBy`, `UpdatedAt` — populate from entity in `OnGetAsync`.

**Audit section in `.cshtml`** — placed inside `card-body`, after `</form>`:
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

Dates must always use `.ToUserLocalTime(Model.UserTimezone, AppConstants.DateTimeFormat)`.

### Edit pages — pageMsg JS object and soft delete

Every Admin Edit page must use the `pageMsg` JS object and include `OnPostSoftDeleteAsync`.

**PageModel properties:** `MsgDeleteConfirmTitle`, `MsgDeleteConfirmText`, `MsgDeleteConfirmBtn`, `MsgCancelBtn`, `MsgDeleteSuccess`, `MsgDeleteError`, `LabelDelete` — all loaded in `OnGetAsync` via `_translation.GetAsync(...)`.

**Handler:**
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

**Delete button** — inside form button row, pushed right with `ms-auto`:
```cshtml
<a href="#" id="btnDelete" class="btn btn-outline-danger ms-auto" data-code="@Model.EntityCode">
  <i class="ri ri-delete-bin-line me-1"></i>@Model.LabelDelete
</a>
```

**Hidden antiforgery form** — placed outside the main form:
```cshtml
<form id="formAjax" method="post">@Html.AntiForgeryToken()</form>
```

**`pageMsg` object in `@section PageScripts`:**
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

SweetAlert confirm on `btnDelete` click → `$.post('?handler=SoftDelete&entityCode=...')` → on success show success toast then `window.location.href = '@Url.Page(Routes.AdminXxx)'`.

### Sidebar navigation menu

Rendered by `Areas/Admin/Pages/Layouts/Sections/Menu/_VerticalMenu.cshtml`. Calls `MenuDbHelper.GetNavMenuAsync(roleId, isSuperAdmin)`.

**Menu level convention:**

| `level` | Role | Has URL? |
|---|---|---|
| 0 | Group header (non-clickable label) | No |
| 1 | Module (collapsible, top-level) | No |
| 2 | Sub module OR function | Only if leaf |

**Menu URL format:**
```
/Admin/Countries     ← correct
/Admin/Country       ← wrong (singular)
/Admin/Countries/Index  ← wrong (don't include /Index)
```

**`IsActive` detection:**
```csharp
bool IsActive(string? url) =>
    !string.IsNullOrEmpty(url) &&
    (current == url || current?.StartsWith(url + "/") == true);
```

Do not use `ViewContext.RouteData.Values["Page"]` — returns Razor Pages path format which doesn't match stored URLs.

**Role-based filtering:** SuperAdmin returns all menus. Other roles return only menus in `role_menus`. Uses flat DB load + in-memory tree build (NOT EF Core filtered Include/ThenInclude — silently returns empty `Children`).

**Login claims — `RoleId` and `IsSuperAdmin` stored in auth cookie.**

### Translation-enabled entities (entity + `*Translation` table)

**DbContext — cascade delete on the `HasMany` side:**
```csharp
entity.HasMany(e => e.Translations)
      .WithOne(t => t.PaymentMethod)
      .HasForeignKey(t => t.PaymentCode)
      .OnDelete(DeleteBehavior.Cascade);
```

**DbHelper — `GetAllAsync` always takes `string languageCode`:**
```csharp
public async Task<List<PaymentMethod>> GetAllAsync(string languageCode)
    => await ExecuteAsync(async () =>
        (await _db.PaymentMethods
            .Where(p => p.Status != StatusConstants.Deleted)
            .Include(p => p.Translations.Where(t => t.LanguageCode == languageCode))
            .ToListAsync())
        .OrderBy(p => p.Translations.FirstOrDefault()?.PaymentName ?? p.PaymentCode)
        .ToList());
```

**Index PageModel — use `CurrentLangCode`, never hardcode `"en"`:**
```csharp
var langCode = string.IsNullOrEmpty(CurrentLangCode) ? "en" : CurrentLangCode;
Items = await _dbHelper.GetAllAsync(langCode);
```

**`*AddResult` enum** — lives in `Dtos/*Dtos.cs`, not inline in the DbHelper.

**`BuildInputsAsync`** — private helper used on both GET and POST-error paths in Create/Edit PageModels. Pass existing translations on GET; pass `null` to re-read from `Request.Form` on POST error.

**Edit page entity name for delete confirm title** — use the English translation, fall back to the code:
```csharp
var entityName = entity.Translations.FirstOrDefault(t => t.LanguageCode == "en")?.PaymentName ?? paymentCode;
```

### Localization — every user-facing string in .cshtml must use `T.GetAsync`

**All** visible text — page titles, labels, placeholders, button text, table headers, badge labels, modal titles, SweetAlert text. No exceptions.

**JavaScript strings** must be pre-declared as Razor variables in a top-level `@{ }` block:
```cshtml
@{ var lblOrders = await T.GetAsync("Orders"); }
@section PageScripts {
<script>series: [{ name: '@lblOrders', data: ordersData }]</script>
}
```

---

### Cropper.js profile image crop + AJAX upload

Load from CDN in `VendorStyles`/`VendorScripts`:
```html
<link href="https://cdnjs.cloudflare.com/ajax/libs/cropperjs/1.6.2/cropper.min.css" rel="stylesheet" />
<script src="https://cdnjs.cloudflare.com/ajax/libs/cropperjs/1.6.2/cropper.min.js"></script>
```

**Pattern:** file input → FileReader preview → Cropper.js 1:1 crop → AJAX upload via separate `OnPostUploadImageAsync` handler. The upload form is independent from the main edit form.

Handler returns `{ success, filename, message }`. On success, update `<img src>` with cache-bust `?t=Date.now()`.

```csharp
[BindProperty] public IFormFile? fileProfileImage { get; set; }

public async Task<IActionResult> OnPostUploadImageAsync(int id)
{
  if (fileProfileImage == null || fileProfileImage.Length == 0)
    return new JsonResult(new { success = false, message = "No file." });

  var relPath  = _config["UploadPaths:EntityProfile"] ?? "uploads/entity-profiles";
  var fullPath = Path.Combine(_env.WebRootPath, relPath.Replace('/', Path.DirectorySeparatorChar));
  var entity   = await _dbHelper.GetByIdAsync(id);

  if (!string.IsNullOrEmpty(entity?.ProfileImage))
  {
    var old = Path.Combine(fullPath, entity.ProfileImage);
    if (System.IO.File.Exists(old)) System.IO.File.Delete(old);
  }

  var filename = await ProfileImageHelper.SaveProfileImageAsync(fileProfileImage, entity!.Username, fullPath);
  await _dbHelper.UpdateProfileImageAsync(id, filename, CurrentUsername);
  return new JsonResult(new { success = true, filename, message = await _translation.GetAsync(MessageConstants.UpdateSuccess) });
}
```

On soft delete: do **not** delete the physical image file.

---

### Select2 AJAX search — `OnGetSearch*Async` handler

```csharp
public async Task<IActionResult> OnGetSearchMembersAsync(string term)
{
  var results = await _memberDbHelper.SearchAsync(term ?? string.Empty);
  return new JsonResult(results.Select(r => new { id = r.Id, text = $"{r.Username} — {r.FullName}" }));
}
```

Compute URL once in `@{ }` block: `var searchUrl = Url.Page("/Members/Index", "SearchMembers", new { area = "Admin" });`

Select2 init with `ajax: { url: '@searchUrl', dataType: 'json', delay: 300, ... }`. Use `on('select2:select')` to populate a hidden `<input>` with the selected ID — the `<select>` is display only.

---

### Multi-section Manage page pattern

When an entity needs multiple independent administrative actions (change username, change rank, etc.), create a dedicated `/Manage` page. Each action is a card with its own AJAX save using a single `#formAjax` antiforgery token and a shared `saveSection(handler, data)` JS function:

```javascript
function saveSection(handler, data) {
  data.__RequestVerificationToken = $('#formAjax input[name="__RequestVerificationToken"]').val();
  $.post('?handler=' + handler + '&id=' + entityId, data, function (res) {
    if (res.success) {
      Swal.fire({ icon: 'success', title: res.message, timer: 1500, showConfirmButton: false })
        .then(function () { location.reload(); });
    } else { Swal.fire({ icon: 'error', text: res.message }); }
  });
}
```

Button calls `saveSection` inline with the form value at click time. All `[BindProperty]` per section; plain properties for display (populated in `PopulateAsync()`, never overwritten by POST). Add both `/Edit` and `/Manage` to `Constants/Routes.cs`.

---

### Self-referencing FK — EF Core Fluent API

Use `OnDelete(DeleteBehavior.Restrict)` — never `Cascade` on self-referencing FKs:

```csharp
entity.HasOne(e => e.Sponsor).WithMany().HasForeignKey(e => e.SponsorId).OnDelete(DeleteBehavior.Restrict);
entity.HasOne(e => e.BinaryParent).WithMany().HasForeignKey(e => e.BinaryParentId).OnDelete(DeleteBehavior.Restrict);
```

Do **not** use filtered `Include/ThenInclude` for self-referencing tree traversal — EF Core silently returns empty `Children`. Load flat and build the tree in memory.

---

## Dependency Injection Rules

All services registered as **Scoped** unless stateless:
```csharp
builder.Services.AddScoped<ThingDbHelper>();
builder.Services.AddScoped<ThingService>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
```

---

## Middleware Pipeline Order

Do not reorder — order is load-bearing:

```
UseSession
UseStaticFiles
UseRequestLocalization
UseRouting
UseAuthentication
UseAuthorization
UseMiddleware<MaintenanceMiddleware>
UseMiddleware<SessionTrackingMiddleware>
MapRazorPages
```

---

## Localization

Supported cultures: `en` (English), `zh-Hans` (Simplified Chinese). Language stored in `lang` cookie.  
All translations in `language_resources` DB table — never hard-code user-facing strings.  
Use `MessageConstants.*` keys when calling `TranslationService.GetAsync(key)`.

### Language selector on Login page
Renders `<select>` populated from `languages` DB table (active records only). On change writes both `.AspNetCore.Culture` cookie and `lang` cookie, then reloads. `currentCulture` comes from `CultureInfo.CurrentUICulture.TwoLetterISOLanguageName`. PageModel exposes `List<Language> Languages` via `LanguageDbHelper.GetAllActiveAsync()`.

---

## Razor (.cshtml) Comments

Always use Razor comment syntax — never HTML comments for code notes:

| Syntax | Renders in HTML? | Use for |
|---|---|---|
| `@* ... *@` | No | All comments in `.cshtml` |
| `<!-- ... -->` | Yes | Intentional HTML comments only |
| `{{!-- ... --}}` | Yes (as plain text) | Never — this is Handlebars, not Razor |

---

## Always

- Use `async/await` for all database and I/O operations
- Use `DateTime.UtcNow` for all timestamp fields
- In `CreateAsync`, explicitly assign **all** entity fields including `CreatedAt = DateTime.UtcNow`, `CreatedBy`, `UpdatedAt = DateTime.UtcNow`, `UpdatedBy`
- In `UpdateAsync`, accept a model object as first argument, fetch the tracked record inside the helper, assign field by field — never `_db.Update(entity)` on detached input
- Always set `HasDefaultValueSql("now() AT TIME ZONE 'utc'")` on every `created_at`/`updated_at` in `AppDbContext` — and `TIMESTAMPTZ NOT NULL DEFAULT (now() AT TIME ZONE 'utc')` in raw SQL
- Map DB columns explicitly with `.HasColumnName("snake_case")` in `AppDbContext`
- Keep Models as plain POCOs — no methods, no business logic
- Put all EF queries in `DbHelper` subclasses, not in PageModels or Services
- Register new DbHelpers and Services as `Scoped` in `Program.cs`
- Use `string.Empty` as default for string properties in models
- Use `Constants/` classes for any magic strings or numbers used in multiple places
- Use `DbSet<T>` with expression-bodied property syntax: `public DbSet<Thing> Things => Set<Thing>();`
- Create a new `*DbHelper` class per entity group
- After model changes, provide a raw SQL script for pgAdmin — do not suggest `dotnet ef migrations`. Save all `.sql` files to `D:\MRMR\Script\`
- Use soft delete — set `Status = StatusConstants.Deleted` instead of physically deleting
- Use `long` (C#) / `BIGSERIAL` (SQL) for PKs on high-volume tables — `int`/`SERIAL` overflows at ~2 billion rows
- When a middleware's `InvokeAsync` needs a scoped service, inject it as a **method parameter** — not via the constructor (constructor injection creates singleton-scoped instance breaking scoped EF Core contexts)
- All `<select>` names and `[BindProperty]` properties for dropdowns use `ddl` prefix
- All UI-facing strings go through `TranslationService.GetAsync(key)` — no hardcoded strings
- Use SweetAlert2 for all alerts — vendor file at `~/vendor/libs/sweetalert2/sweetalert2.dist.js`
- Admin pages inherit from `AdminPageModel`; customer pages from `CustomerPageModel`
- All DbHelper methods must wrap their body in `ExecuteAsync(...)` — never call `_db.*` directly in a DbHelper method
- Use named handlers: `OnPostCreateAsync`, `OnPostUpdateAsync`, `OnPostDeleteAsync`
- After generating any admin CRUD module, immediately output a `language_resources` SQL upsert covering every `T.GetAsync(...)` key used. Format: `INSERT INTO language_resources (language_code, key, value) VALUES (...) ON CONFLICT (language_code, key) DO UPDATE SET value = EXCLUDED.value;` — cover both `en` and `zh-Hans`

## Never

- Never use Razor reserved keywords as variable names in `.cshtml` — `section`, `functions`, `namespace`, `page`, `model`, `inherits`, `helper` are reserved. Using them as `@foreach` loop variables (e.g. `var section in Model.Items`) causes Razor to misparse `@section.Property` as a malformed directive. Use alternatives: `stype` instead of `section`, etc.
- Never use `@(condition ? "selected" : "")` inside an `<option>` attribute area — causes RZ1031. Use `@if` blocks instead.
- Never add MVC controllers — Razor Pages only
- Never query `AppDbContext` directly inside a PageModel — use a `DbHelper`
- Never hard-code user-facing strings — use `TranslationService` + `MessageConstants`
- Never store passwords as plain text — use `PasswordCryptoHelper` (AES)
- Never use `AddTransient` for `DbHelper` or `Service` classes — use `AddScoped`
- Never put business logic inside Model classes
- Never edit files under `wwwroot/vendor/` — third-party libs
- Never edit `*.dist.js` or `*.dist.css` files directly — edit source and rebuild via Webpack/Gulp
- Never skip `UseAuthentication`/`UseAuthorization` in the pipeline
- Never physically delete records — always soft delete via `Status = StatusConstants.Deleted`
- Never reorder the middleware pipeline without understanding the dependencies
- Never inject `AppDbContext` or any scoped `DbHelper` into a `BackgroundService` constructor — use `IServiceProvider.CreateScope()` instead
- Never use `[Authorize]` directly on a page model — use `AdminPageModel` or `CustomerPageModel`
- Never commit real SMTP passwords or connection strings — move secrets to `appsettings.Development.json` or User Secrets
- Never assume `permissions.menu_id` — the actual column is `module` (varchar) storing `menus.menu_code`. The EF relationship is `HasForeignKey(p => p.Module).HasPrincipalKey(m => m.MenuCode)`. Adding `MenuId` (int) to `Permission` will produce a runtime "column does not exist" error.
- Never use inline `onclick="fn('@value')"` on DataTable row buttons when the value comes from user data — use `data-*` attributes + `$(document).on('click', '.cls', ...)` instead.
