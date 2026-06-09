using Microsoft.EntityFrameworkCore;
using MyApp.Constants.MRMR;
using MyApp.Data;

namespace MyApp.Services.MRMR;

public class ApplicationIdGeneratorService
{
  private readonly AppDbContext _db;

  public ApplicationIdGeneratorService(AppDbContext db)
  {
    _db = db;
  }

  public async Task<string> GenerateAsync(string applicationType)
  {
    var typeCode = applicationType == "Corporate" ? "C" : "I";
    var year = DateTime.UtcNow.Year.ToString();
    var prefix = $"MRMR-{typeCode}-{year}-";

    var lastId = await _db.Applications
        .Where(a => a.ApplicationId.StartsWith(prefix))
        .OrderByDescending(a => a.ApplicationId)
        .Select(a => a.ApplicationId)
        .FirstOrDefaultAsync();

    int nextSeq = 1;
    if (!string.IsNullOrEmpty(lastId))
    {
      var seqPart = lastId.Substring(prefix.Length);
      if (int.TryParse(seqPart, out int lastSeq))
        nextSeq = lastSeq + 1;
    }

    return $"{prefix}{nextSeq.ToString().PadLeft(ApplicationIdConstants.SequencePadding, '0')}";
  }
}
