using Microsoft.EntityFrameworkCore;
using MyApp.Data;

namespace MyApp.Services.MRMR;

public class InvoiceService
{
  private readonly AppDbContext _db;

  public InvoiceService(AppDbContext db)
  {
    _db = db;
  }

  public async Task<string> GenerateInvoiceNoAsync()
  {
    var year = DateTime.UtcNow.Year.ToString();
    var prefix = $"INV-{year}-";

    var lastInvoice = await _db.Payments
        .Where(p => p.InvoiceNo != null && p.InvoiceNo.StartsWith(prefix))
        .OrderByDescending(p => p.InvoiceNo)
        .Select(p => p.InvoiceNo)
        .FirstOrDefaultAsync();

    int nextSeq = 1;
    if (!string.IsNullOrEmpty(lastInvoice))
    {
      var seqPart = lastInvoice.Substring(prefix.Length);
      if (int.TryParse(seqPart, out int lastSeq))
        nextSeq = lastSeq + 1;
    }

    return $"{prefix}{nextSeq.ToString().PadLeft(5, '0')}";
  }
}
