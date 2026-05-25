using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Dtos;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class StateDbHelper : DbHelper
{
  public StateDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory) : base(db, audit, loggerFactory) { }

  public async Task<List<State>> GetAllAsync(string countryCode, string languageCode)
      => await ExecuteAsync(async () =>
      {
        var items = await _db.States
            .Where(s => s.CountryCode == countryCode && s.Status != StatusConstants.Deleted)
            .Include(s => s.Translations.Where(t => t.LanguageCode == languageCode))
            .ToListAsync();
        return items
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Translations.FirstOrDefault()?.StateName ?? s.StateCode)
            .ToList();
      });

  public async Task<State?> GetByIdAsync(int id)
      => await ExecuteAsync(() => _db.States
          .Include(s => s.Translations)
          .Include(s => s.Country).ThenInclude(c => c.Translations)
          .FirstOrDefaultAsync(s => s.Id == id && s.Status != StatusConstants.Deleted));

  public async Task<List<State>> GetAllActiveAsync(string countryCode, string languageCode)
      => await ExecuteAsync(async () =>
      {
        var items = await _db.States
            .Where(s => s.CountryCode == countryCode && s.Status == StatusConstants.Active)
            .Include(s => s.Translations.Where(t => t.LanguageCode == languageCode))
            .ToListAsync();
        return items
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Translations.FirstOrDefault()?.StateName ?? s.StateCode)
            .ToList();
      });

  public async Task<List<State>> GetActiveByCountryAsync(string countryCode, string languageCode = "en")
      => await ExecuteAsync(async () =>
      {
        var items = await _db.States
            .Where(s => s.CountryCode == countryCode && s.Status == StatusConstants.Active)
            .Include(s => s.Translations.Where(t => t.LanguageCode == languageCode))
            .ToListAsync();
        return items
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Translations.FirstOrDefault()?.StateName ?? s.StateCode)
            .ToList();
      });

  public async Task<StateAddResult> AddAsync(State state, List<StateTranslation> translations, string createdBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.States
            .FirstOrDefaultAsync(s => s.CountryCode == state.CountryCode && s.StateCode == state.StateCode);

        if (existing != null && existing.Status == StatusConstants.Deleted)
        {
          existing.SortOrder = state.SortOrder;
          existing.Status    = StatusConstants.Active;
          existing.UpdatedAt = DateTime.UtcNow;
          existing.UpdatedBy = createdBy;

          var existingTranslations = await _db.StateTranslations
              .Where(t => t.StateId == existing.Id)
              .ToListAsync();

          foreach (var translation in translations)
          {
            translation.StateId = existing.Id;
            var existingT = existingTranslations.FirstOrDefault(x => x.LanguageCode == translation.LanguageCode);
            if (existingT != null)
              existingT.StateName = translation.StateName;
            else
              _db.StateTranslations.Add(translation);
          }

          await _db.SaveChangesAsync();
          await _audit.LogActionAsync("states", existing.Id.ToString(), AuditConstants.Actions.Restore, createdBy, remarks: "Restored from deleted");
          return StateAddResult.Restored;
        }

        if (existing != null)
          return StateAddResult.DuplicateActive;

        state.CreatedAt = DateTime.UtcNow;
        state.CreatedBy = createdBy;
        state.UpdatedAt = DateTime.UtcNow;
        state.UpdatedBy = createdBy;

        _db.States.Add(state);
        await _db.SaveChangesAsync();

        foreach (var t in translations)
          t.StateId = state.Id;
        _db.StateTranslations.AddRange(translations);
        await _db.SaveChangesAsync();

        await _audit.LogInsertAsync("states", state.Id.ToString(), state, createdBy);
        return StateAddResult.Created;
      });

  public async Task UpdateAsync(State state, List<StateTranslation> translations, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var existing = await _db.States.FindAsync(state.Id);
        if (existing == null) return;

        var old = new State
        {
          Id          = existing.Id,
          CountryCode = existing.CountryCode,
          StateCode   = existing.StateCode,
          SortOrder   = existing.SortOrder,
          Status      = existing.Status
        };

        existing.SortOrder = state.SortOrder;
        existing.Status    = state.Status;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedBy = updatedBy;

        var existingTranslations = await _db.StateTranslations
            .Where(t => t.StateId == state.Id)
            .ToListAsync();

        foreach (var translation in translations)
        {
          translation.StateId = state.Id;
          var existingT = existingTranslations.FirstOrDefault(x => x.LanguageCode == translation.LanguageCode);
          if (existingT != null)
            existingT.StateName = translation.StateName;
          else
            _db.StateTranslations.Add(translation);
        }

        await _db.SaveChangesAsync();
        await _audit.LogUpdateAsync("states", existing.Id.ToString(), old, existing, updatedBy);
      });

  public async Task UpdateStatusAsync(int id, string status, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var state = await _db.States.FindAsync(id);
        if (state != null)
        {
          var oldStatus = state.Status;
          state.Status    = status;
          state.UpdatedAt = DateTime.UtcNow;
          state.UpdatedBy = updatedBy;
          await _db.SaveChangesAsync();
          var action = status == StatusConstants.Deleted
              ? AuditConstants.Actions.Delete
              : AuditConstants.Actions.Update;
          await _audit.LogActionAsync("states", id.ToString(), action, updatedBy,
              remarks: $"Status changed from {oldStatus} to {status}");
        }
      });
}
