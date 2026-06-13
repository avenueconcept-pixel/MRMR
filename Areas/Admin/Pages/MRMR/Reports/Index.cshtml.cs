using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using MyApp.Helper;
using MyApp.Helper.DB.MRMR;
using MyApp.Models.MRMR;

namespace MyApp.Areas.Admin.Pages.MRMR.Reports;

public class IndexModel : AdminPageModel
{
    private readonly AdminMrmrDbHelper _mrmrDb;

    public IndexModel(AdminMrmrDbHelper mrmrDb)
    {
        _mrmrDb = mrmrDb;
    }

    [BindProperty(SupportsGet = true)] public int? CategoryId { get; set; }

    public List<AwardCategory> Categories { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        Categories = await _mrmrDb.GetActiveCategoriesAsync();
        return Page();
    }

    // ── Export: Applicant List ──
    public async Task<IActionResult> OnPostExportApplicantsAsync()
    {
        var apps = await _mrmrDb.GetAllApplicationsForReportAsync();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Applicants");

        var headers = new[]
        {
            "Application ID", "Applicant Name", "NRIC/Passport", "Email",
            "Contact No", "Type", "Company", "Award Category",
            "Status", "Payment Method", "Final Submitted", "Submitted At", "Registered At"
        };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#1565C0");
            ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }

        int row = 2;
        foreach (var app in apps)
        {
            ws.Cell(row, 1).Value  = app.ApplicationId;
            ws.Cell(row, 2).Value  = app.Registrant.FullName;
            ws.Cell(row, 3).Value  = app.Registrant.NricPassport;
            ws.Cell(row, 4).Value  = app.Registrant.Email;
            ws.Cell(row, 5).Value  = app.Registrant.ContactNo;
            ws.Cell(row, 6).Value  = app.ApplicationType;
            ws.Cell(row, 7).Value  = app.Registrant.CompanyName ?? "-";
            ws.Cell(row, 8).Value  = app.AwardCategory?.Name ?? "-";
            ws.Cell(row, 9).Value  = app.Status;
            ws.Cell(row, 10).Value = app.PaymentMethod;
            ws.Cell(row, 11).Value = app.IsFinalSubmitted ? "Yes" : "No";
            ws.Cell(row, 12).Value = app.SubmittedAt?.ToString("dd/MM/yyyy HH:mm") ?? "-";
            ws.Cell(row, 13).Value = app.CreatedAt.ToString("dd/MM/yyyy HH:mm");
            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        wb.SaveAs(stream);

        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"MRMR_Applicants_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    // ── Export: Payment Summary ──
    public async Task<IActionResult> OnPostExportPaymentsAsync()
    {
        var payments = await _mrmrDb.GetAllPaymentsForReportAsync();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Payments");

        var headers = new[]
        {
            "Invoice No", "Application ID", "Applicant Name", "Award Category",
            "Payment Type", "Amount (RM)", "Method", "Status",
            "Slip Uploaded", "Verified At", "Admin Remarks"
        };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#1565C0");
            ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }

        int row = 2;
        foreach (var p in payments)
        {
            ws.Cell(row, 1).Value  = p.InvoiceNo ?? "-";
            ws.Cell(row, 2).Value  = p.Application?.ApplicationId ?? "-";
            ws.Cell(row, 3).Value  = p.Application?.Registrant?.FullName ?? "-";
            ws.Cell(row, 4).Value  = p.Application?.AwardCategory?.Name ?? "-";
            ws.Cell(row, 5).Value  = p.PaymentType;
            ws.Cell(row, 6).Value  = p.Amount;
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 7).Value  = p.Method;
            ws.Cell(row, 8).Value  = p.Status;
            ws.Cell(row, 9).Value  = p.SlipUploadedAt?.ToString("dd/MM/yyyy HH:mm") ?? "-";
            ws.Cell(row, 10).Value = p.VerifiedAt?.ToString("dd/MM/yyyy HH:mm") ?? "-";
            ws.Cell(row, 11).Value = p.AdminRemarks ?? "-";
            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        wb.SaveAs(stream);

        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"MRMR_Payments_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    // ── Export: Evaluation Results ──
    public async Task<IActionResult> OnPostExportEvaluationAsync()
    {
        var rankings = await _mrmrDb.GetRankingsForReportAsync(CategoryId);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Evaluation Results");

        var headers = new[]
        {
            "Rank", "Application ID", "Applicant Name", "Company",
            "Award Category", "Type", "Final Score", "Is Winner", "Committee Remarks",
            "Approved At"
        };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#1B5E20");
            ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }

        int row = 2;
        foreach (var r in rankings)
        {
            ws.Cell(row, 1).Value  = r.RankPosition;
            ws.Cell(row, 2).Value  = r.Application?.ApplicationId ?? "-";
            ws.Cell(row, 3).Value  = r.Application?.Registrant?.FullName ?? "-";
            ws.Cell(row, 4).Value  = r.Application?.Registrant?.CompanyName ?? "-";
            ws.Cell(row, 5).Value  = r.AwardCategory?.Name ?? "-";
            ws.Cell(row, 6).Value  = r.Application?.ApplicationType ?? "-";
            ws.Cell(row, 7).Value  = r.FinalScore;
            ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 8).Value  = r.IsApprovedWinner ? "Yes" : "No";
            ws.Cell(row, 9).Value  = r.CommitteeRemarks ?? "-";
            ws.Cell(row, 10).Value = r.ApprovedAt?.ToString("dd/MM/yyyy HH:mm") ?? "-";

            if (r.IsApprovedWinner)
                ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F5E9");

            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        wb.SaveAs(stream);

        var catSuffix = CategoryId.HasValue && CategoryId > 0 ? $"_Cat{CategoryId}" : "_All";
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"MRMR_EvaluationResults{catSuffix}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }
}
