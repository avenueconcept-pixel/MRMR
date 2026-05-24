using Microsoft.AspNetCore.Mvc;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.AuditLogs;

public class DetailModel : AdminPageModel
{
  private readonly AuditLogDbHelper   _auditLogDb;
  private readonly TranslationService _translation;

  public List<AuditLog> AuditLogs { get; set; } = new();
  public string         TableName { get; set; } = string.Empty;
  public string         RecordId  { get; set; } = string.Empty;

  public DetailModel(AuditLogDbHelper auditLogDb, TranslationService translation)
  {
    _auditLogDb  = auditLogDb;
    _translation = translation;
  }

  public async Task<IActionResult> OnGetAsync(string tableName, string recordId)
  {
    if (string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(recordId))
      return RedirectToPage(Routes.AdminAuditLogs);

    TableName = tableName;
    RecordId  = recordId;
    AuditLogs = await _auditLogDb.GetByRecordAsync(tableName, recordId);

    return Page();
  }
}
