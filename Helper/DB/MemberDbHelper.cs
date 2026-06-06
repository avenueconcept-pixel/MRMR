using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Dtos;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class MemberDbHelper : DbHelper
{
  public MemberDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory)
      : base(db, audit, loggerFactory) { }

  public async Task<List<Member>> GetAllAsync(
      string? filterStatus, string? filterCountryCode,
      string? filterRankCode, bool? filterIsActivated,
      string languageCode)
      => await ExecuteAsync(async () =>
      {
        var q = _db.Members
            .Where(m => m.Status != StatusConstants.Deleted)
            .Include(m => m.Country)
                .ThenInclude(c => c!.Translations.Where(t => t.LanguageCode == languageCode))
            .Include(m => m.Sponsor)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filterStatus))
          q = q.Where(m => m.Status == filterStatus);
        if (!string.IsNullOrEmpty(filterCountryCode))
          q = q.Where(m => m.CountryCode == filterCountryCode);
        if (!string.IsNullOrEmpty(filterRankCode))
          q = q.Where(m => m.CurrentRankCode == filterRankCode);
        if (filterIsActivated.HasValue)
          q = q.Where(m => m.IsActivated == filterIsActivated.Value);

        return await q.OrderBy(m => m.FullName).ToListAsync();
      });

  public async Task<Member?> GetByIdAsync(int id)
      => await ExecuteAsync(async () =>
          await _db.Members
              .Include(m => m.Country)
              .Include(m => m.Sponsor)
              .Include(m => m.BinaryParent)
              .FirstOrDefaultAsync(m => m.Id == id));

  public async Task<List<MemberSearchResult>> SearchAsync(string term, int excludeId = 0)
      => await ExecuteAsync(async () =>
          await _db.Members
              .Where(m => m.Status != StatusConstants.Deleted
                       && m.Id != excludeId
                       && (m.Username.Contains(term) || m.FullName.Contains(term)))
              .OrderBy(m => m.Username)
              .Take(20)
              .Select(m => new MemberSearchResult
              {
                Id       = m.Id,
                Username = m.Username,
                FullName = m.FullName
              })
              .ToListAsync());

  public async Task<bool> IsUsernameUniqueAsync(string username, int excludeId = 0)
      => await ExecuteAsync(async () =>
          !await _db.Members.AnyAsync(m => m.Username == username && m.Id != excludeId));

  public async Task<MemberBinarySlot?> FindNextBinarySlotAsync(int sponsorId)
      => await ExecuteAsync(async () =>
      {
        var queue = new Queue<int>();
        queue.Enqueue(sponsorId);

        while (queue.Count > 0)
        {
          var currentId = queue.Dequeue();
          var leftChild = await _db.Members
              .FirstOrDefaultAsync(m => m.BinaryParentId == currentId
                                     && m.BinaryPosition == BinaryPositionConstants.Left);
          var rightChild = await _db.Members
              .FirstOrDefaultAsync(m => m.BinaryParentId == currentId
                                     && m.BinaryPosition == BinaryPositionConstants.Right);

          if (leftChild == null)
          {
            var parent = await _db.Members.FindAsync(currentId);
            return new MemberBinarySlot
            {
              MemberId = currentId,
              Username = parent?.Username ?? string.Empty,
              FullName = parent?.FullName ?? string.Empty,
              Position = BinaryPositionConstants.Left,
              HasLeft  = false,
              HasRight = rightChild != null
            };
          }
          if (rightChild == null)
          {
            var parent = await _db.Members.FindAsync(currentId);
            return new MemberBinarySlot
            {
              MemberId = currentId,
              Username = parent?.Username ?? string.Empty,
              FullName = parent?.FullName ?? string.Empty,
              Position = BinaryPositionConstants.Right,
              HasLeft  = true,
              HasRight = false
            };
          }
          queue.Enqueue(leftChild.Id);
          queue.Enqueue(rightChild.Id);
        }
        return null;
      });

  public async Task AddAsync(Member member)
      => await ExecuteAsync(async () =>
      {
        member.CreatedAt = DateTime.UtcNow;
        member.UpdatedAt = DateTime.UtcNow;
        member.JoinedAt  = member.JoinedAt == default ? DateTime.UtcNow : member.JoinedAt;
        _db.Members.Add(member);
        await _db.SaveChangesAsync();
      });

  public async Task UpdateAsync(Member member, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var entity = await _db.Members.FindAsync(member.Id)
            ?? throw new Exception($"Member {member.Id} not found");
        entity.FullName         = member.FullName;
        entity.IdType           = member.IdType;
        entity.IdNo             = member.IdNo;
        entity.Email            = member.Email;
        entity.PhoneCountryCode = member.PhoneCountryCode;
        entity.PhoneNumber      = member.PhoneNumber;
        entity.AddressLine1     = member.AddressLine1;
        entity.AddressLine2     = member.AddressLine2;
        entity.City             = member.City;
        entity.State            = member.State;
        entity.Postcode         = member.Postcode;
        entity.CountryCode      = member.CountryCode;
        entity.BankName         = member.BankName;
        entity.BankAccountName  = member.BankAccountName;
        entity.BankAccountNo    = member.BankAccountNo;
        entity.UpdatedBy        = updatedBy;
        entity.UpdatedAt        = DateTime.UtcNow;
        await _db.SaveChangesAsync();
      });

  public async Task UpdateProfileImageAsync(int id, string filename, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var entity = await _db.Members.FindAsync(id)
            ?? throw new Exception($"Member {id} not found");
        entity.ProfileImage = filename;
        entity.UpdatedBy    = updatedBy;
        entity.UpdatedAt    = DateTime.UtcNow;
        await _db.SaveChangesAsync();
      });

  public async Task ChangeUsernameAsync(int id, string newUsername, string changedBy)
      => await ExecuteAsync(async () =>
      {
        var entity = await _db.Members.FindAsync(id)
            ?? throw new Exception($"Member {id} not found");
        _db.MemberChangeLogs.Add(new MemberChangeLog
        {
          MemberId   = id,
          ChangeType = MemberChangeTypeConstants.Username,
          OldValue   = entity.Username,
          NewValue   = newUsername,
          ChangedBy  = changedBy,
          ChangedAt  = DateTime.UtcNow
        });
        entity.Username  = newUsername;
        entity.UpdatedBy = changedBy;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
      });

  public async Task ChangeSponsorAsync(int id, int? newSponsorId, string changedBy)
      => await ExecuteAsync(async () =>
      {
        var entity    = await _db.Members.FindAsync(id)
            ?? throw new Exception($"Member {id} not found");
        var oldSponsor = entity.SponsorId.HasValue
            ? (await _db.Members.FindAsync(entity.SponsorId.Value))?.Username ?? string.Empty
            : string.Empty;
        var newSponsor = newSponsorId.HasValue
            ? (await _db.Members.FindAsync(newSponsorId.Value))?.Username ?? string.Empty
            : string.Empty;
        _db.MemberChangeLogs.Add(new MemberChangeLog
        {
          MemberId   = id,
          ChangeType = MemberChangeTypeConstants.Sponsor,
          OldValue   = oldSponsor,
          NewValue   = newSponsor,
          ChangedBy  = changedBy,
          ChangedAt  = DateTime.UtcNow
        });
        entity.SponsorId = newSponsorId;
        entity.UpdatedBy = changedBy;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
      });

  public async Task ChangeBinaryParentAsync(int id, int? newParentId, string? newPosition, string changedBy)
      => await ExecuteAsync(async () =>
      {
        var entity    = await _db.Members.FindAsync(id)
            ?? throw new Exception($"Member {id} not found");
        var oldParent = entity.BinaryParentId.HasValue
            ? (await _db.Members.FindAsync(entity.BinaryParentId.Value))?.Username ?? string.Empty
            : string.Empty;
        var newParent = newParentId.HasValue
            ? (await _db.Members.FindAsync(newParentId.Value))?.Username ?? string.Empty
            : string.Empty;
        _db.MemberChangeLogs.Add(new MemberChangeLog
        {
          MemberId   = id,
          ChangeType = MemberChangeTypeConstants.BinaryParent,
          OldValue   = $"{oldParent} ({entity.BinaryPosition})",
          NewValue   = $"{newParent} ({newPosition})",
          ChangedBy  = changedBy,
          ChangedAt  = DateTime.UtcNow
        });
        entity.BinaryParentId = newParentId;
        entity.BinaryPosition = newPosition;
        entity.UpdatedBy      = changedBy;
        entity.UpdatedAt      = DateTime.UtcNow;
        await _db.SaveChangesAsync();
      });

  public async Task ChangeRankAsync(int id, string? newRankCode, string changedBy)
      => await ExecuteAsync(async () =>
      {
        var entity = await _db.Members.FindAsync(id)
            ?? throw new Exception($"Member {id} not found");
        _db.MemberChangeLogs.Add(new MemberChangeLog
        {
          MemberId   = id,
          ChangeType = MemberChangeTypeConstants.Rank,
          OldValue   = entity.CurrentRankCode ?? string.Empty,
          NewValue   = newRankCode ?? string.Empty,
          ChangedBy  = changedBy,
          ChangedAt  = DateTime.UtcNow
        });
        entity.CurrentRankCode = newRankCode;
        if (!string.IsNullOrEmpty(newRankCode))
        {
          var newRank = await _db.Ranks.FirstOrDefaultAsync(r => r.RankCode == newRankCode);
          var highestRank = string.IsNullOrEmpty(entity.HighestRankCode)
              ? null
              : await _db.Ranks.FirstOrDefaultAsync(r => r.RankCode == entity.HighestRankCode);
          if (newRank != null && (highestRank == null || newRank.SortOrder > highestRank.SortOrder))
            entity.HighestRankCode = newRankCode;
        }
        entity.UpdatedBy = changedBy;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
      });

  public async Task ChangeStatusAsync(int id, string newStatus, string changedBy)
      => await ExecuteAsync(async () =>
      {
        var entity = await _db.Members.FindAsync(id)
            ?? throw new Exception($"Member {id} not found");
        _db.MemberChangeLogs.Add(new MemberChangeLog
        {
          MemberId   = id,
          ChangeType = MemberChangeTypeConstants.Status,
          OldValue   = entity.Status,
          NewValue   = newStatus,
          ChangedBy  = changedBy,
          ChangedAt  = DateTime.UtcNow
        });
        entity.Status    = newStatus;
        entity.UpdatedBy = changedBy;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
      });

  public async Task<List<MemberChangeLog>> GetChangeLogsAsync(int memberId)
      => await ExecuteAsync(async () =>
          await _db.MemberChangeLogs
              .Where(l => l.MemberId == memberId)
              .OrderByDescending(l => l.ChangedAt)
              .Take(20)
              .ToListAsync());

  public async Task UpdateStatusAsync(int id, string status, string updatedBy)
      => await ExecuteAsync(async () =>
      {
        var entity = await _db.Members.FindAsync(id)
            ?? throw new Exception($"Member {id} not found");
        entity.Status    = status;
        entity.UpdatedBy = updatedBy;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
      });

  public async Task<List<Rank>> GetAllRanksAsync()
      => await ExecuteAsync(async () =>
          await _db.Ranks
              .Where(r => r.Status == StatusConstants.Active)
              .OrderBy(r => r.SortOrder)
              .ToListAsync());
}
