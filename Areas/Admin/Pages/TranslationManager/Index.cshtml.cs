using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using MyApp.Constants;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.TranslationManager;

public class IndexModel : AdminPageModel
{
  private readonly TranslationDbHelper _dbHelper;
  private readonly TranslationService  _translation;

  public List<TranslationGridRowDto> GridRows      { get; set; } = new();
  public List<string>                LanguageCodes { get; set; } = new();

  public string MsgDeleteConfirmTitle { get; set; } = string.Empty;
  public string MsgDeleteConfirmText  { get; set; } = string.Empty;
  public string MsgDeleteConfirmBtn   { get; set; } = string.Empty;
  public string MsgCancelBtn          { get; set; } = string.Empty;
  public string MsgDeleteSuccess      { get; set; } = string.Empty;
  public string MsgDeleteError        { get; set; } = string.Empty;
  public string MsgSaveSuccess        { get; set; } = string.Empty;
  public string MsgSaveError          { get; set; } = string.Empty;
  public string MsgImportSuccess      { get; set; } = string.Empty;
  public string MsgImportError        { get; set; } = string.Empty;
  public string MsgKeyExists          { get; set; } = string.Empty;
  public string MsgImportInserted     { get; set; } = string.Empty;
  public string MsgImportUpdated      { get; set; } = string.Empty;
  public string MsgImportSkipped      { get; set; } = string.Empty;

  public IndexModel(TranslationDbHelper dbHelper, TranslationService translation)
  {
    _dbHelper    = dbHelper;
    _translation = translation;
  }

  public async Task OnGetAsync()
  {
    AlertMessageType = "";

    LanguageCodes = await _dbHelper.GetActiveLanguageCodesAsync();
    var allRows   = await _dbHelper.GetAllAsync();

    GridRows = allRows
        .GroupBy(r => r.Key)
        .Select(g => new TranslationGridRowDto
        {
          Key    = g.Key,
          Values = g.ToDictionary(r => r.LanguageCode, r => r.Value)
        })
        .OrderBy(r => r.Key)
        .ToList();

    var entityName        = await _translation.GetAsync("Menu.TranslationManager");
    MsgDeleteConfirmTitle = $"{await _translation.GetAsync("Confirm.DeleteTitle")} {entityName}";
    MsgDeleteConfirmText  = await _translation.GetAsync("Confirm.DeleteText");
    MsgDeleteConfirmBtn   = await _translation.GetAsync("Btn.YesDelete");
    MsgCancelBtn          = await _translation.GetAsync("Btn.Cancel");
    MsgDeleteSuccess      = await _translation.GetAsync(MessageConstants.DeleteSuccess);
    MsgDeleteError        = await _translation.GetAsync(MessageConstants.DeleteError);
    MsgSaveSuccess        = await _translation.GetAsync(MessageConstants.SaveSuccess);
    MsgSaveError          = await _translation.GetAsync(MessageConstants.SaveError);
    MsgImportSuccess      = await _translation.GetAsync(MessageConstants.ImportSuccess);
    MsgImportError        = await _translation.GetAsync(MessageConstants.ImportError);
    MsgKeyExists          = await _translation.GetAsync(MessageConstants.KeyExists);
    MsgImportInserted     = await _translation.GetAsync("Translation.ImportInserted");
    MsgImportUpdated      = await _translation.GetAsync("Translation.ImportUpdated");
    MsgImportSkipped      = await _translation.GetAsync("Translation.ImportSkipped");
  }

  public async Task<IActionResult> OnPostSaveRowAsync([FromBody] List<TranslationRowDto> rows)
  {
    try
    {
      await _dbHelper.UpsertBatchAsync(rows, CurrentUsername);
      _translation.ClearCache();
      var msg = await _translation.GetAsync(MessageConstants.SaveSuccess);
      return new JsonResult(new { success = true, message = msg });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.SaveError);
      return new JsonResult(new { success = false, message = msg });
    }
  }

  public async Task<IActionResult> OnPostAddKeyAsync([FromBody] List<TranslationRowDto> rows)
  {
    try
    {
      if (rows.Count == 0 || string.IsNullOrWhiteSpace(rows[0].Key))
      {
        var errMsg = await _translation.GetAsync(MessageConstants.SaveError);
        return new JsonResult(new { success = false, message = errMsg });
      }

      var key = rows[0].Key.Trim();

      if (await _dbHelper.KeyExistsAsync(key))
      {
        var errMsg = await _translation.GetAsync(MessageConstants.KeyExists);
        return new JsonResult(new { success = false, message = errMsg, keyExists = true });
      }

      var normalized = rows.Select(r => new TranslationRowDto
      {
        Key          = key,
        LanguageCode = r.LanguageCode,
        Value        = r.Value
      }).ToList();

      await _dbHelper.UpsertBatchAsync(normalized, CurrentUsername);
      _translation.ClearCache();
      var msg = await _translation.GetAsync(MessageConstants.SaveSuccess);
      return new JsonResult(new { success = true, message = msg });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.SaveError);
      return new JsonResult(new { success = false, message = msg });
    }
  }

  public async Task<IActionResult> OnPostDeleteKeyAsync(string key)
  {
    try
    {
      await _dbHelper.DeleteByKeyAsync(key, CurrentUsername);
      _translation.ClearCache();
      var msg = await _translation.GetAsync(MessageConstants.DeleteSuccess);
      return new JsonResult(new { success = true, message = msg });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.DeleteError);
      return new JsonResult(new { success = false, message = msg });
    }
  }

  public async Task<IActionResult> OnGetExportAsync()
  {
    var langCodes = await _dbHelper.GetActiveLanguageCodesAsync();
    var allRows   = await _dbHelper.GetAllAsync();

    var grouped = allRows
        .GroupBy(r => r.Key)
        .OrderBy(g => g.Key)
        .Select(g => new
        {
          Key    = g.Key,
          Values = g.ToDictionary(r => r.LanguageCode, r => r.Value)
        })
        .ToList();

    using var wb = new XLWorkbook();
    var ws = wb.Worksheets.Add("Translations");

    ws.Cell(1, 1).Value = "Key";
    for (int i = 0; i < langCodes.Count; i++)
      ws.Cell(1, i + 2).Value = langCodes[i];

    var headerRange = ws.Range(1, 1, 1, langCodes.Count + 1);
    headerRange.Style.Font.Bold = true;
    headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
    headerRange.Style.Font.FontColor = XLColor.White;

    for (int row = 0; row < grouped.Count; row++)
    {
      ws.Cell(row + 2, 1).Value = grouped[row].Key;
      for (int col = 0; col < langCodes.Count; col++)
      {
        var lang = langCodes[col];
        ws.Cell(row + 2, col + 2).Value =
            grouped[row].Values.TryGetValue(lang, out var v) ? v : string.Empty;
      }
    }

    ws.Columns().AdjustToContents();

    using var ms = new MemoryStream();
    wb.SaveAs(ms);
    var fileName = $"translations_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
    return File(ms.ToArray(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        fileName);
  }

  public async Task<IActionResult> OnPostImportAsync()
  {
    var file = Request.Form.Files["fileImport"];
    if (file == null || file.Length == 0)
    {
      var errMsg = await _translation.GetAsync(MessageConstants.ImportError);
      return new JsonResult(new { success = false, message = errMsg });
    }

    var result = new ImportResultDto();

    try
    {
      using var stream = file.OpenReadStream();
      using var wb = new XLWorkbook(stream);
      var ws = wb.Worksheets.First();

      var lastCol = ws.LastColumnUsed()?.ColumnNumber() ?? 1;
      if (lastCol < 2)
      {
        var errMsg = await _translation.GetAsync(MessageConstants.ImportError);
        return new JsonResult(new { success = false, message = errMsg });
      }

      var importLangCodes = new List<string>();
      for (int col = 2; col <= lastCol; col++)
      {
        var lc = ws.Cell(1, col).GetString().Trim();
        if (!string.IsNullOrEmpty(lc))
          importLangCodes.Add(lc);
      }

      if (importLangCodes.Count == 0)
      {
        var errMsg = await _translation.GetAsync(MessageConstants.ImportError);
        return new JsonResult(new { success = false, message = errMsg });
      }

      var lastRow     = ws.LastRowUsed()?.RowNumber() ?? 1;
      var rowsToUpsert = new List<TranslationRowDto>();

      for (int row = 2; row <= lastRow; row++)
      {
        var key = ws.Cell(row, 1).GetString().Trim();
        if (string.IsNullOrWhiteSpace(key))
        {
          result.Skipped++;
          continue;
        }

        for (int col = 0; col < importLangCodes.Count; col++)
        {
          var val = ws.Cell(row, col + 2).GetString().Trim();
          if (string.IsNullOrWhiteSpace(val))
          {
            result.Skipped++;
            continue;
          }
          rowsToUpsert.Add(new TranslationRowDto
          {
            Key          = key,
            LanguageCode = importLangCodes[col],
            Value        = val
          });
        }
      }

      var existingSet = (await _dbHelper.GetAllAsync())
          .Select(r => $"{r.Key}:{r.LanguageCode}")
          .ToHashSet();

      foreach (var r in rowsToUpsert)
      {
        if (existingSet.Contains($"{r.Key}:{r.LanguageCode}"))
          result.Updated++;
        else
          result.Inserted++;
      }

      await _dbHelper.UpsertBatchAsync(rowsToUpsert, CurrentUsername);
      _translation.ClearCache();

      return new JsonResult(new { success = true, result });
    }
    catch (Exception ex)
    {
      result.Errors.Add(ex.Message);
      var errMsg = await _translation.GetAsync(MessageConstants.ImportError);
      return new JsonResult(new { success = false, message = errMsg, result });
    }
  }
}
