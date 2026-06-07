using Microsoft.EntityFrameworkCore;
using MyApp.Constants;
using MyApp.Data;
using MyApp.Dtos;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class WalletDbHelper : DbHelper
{
  public WalletDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory)
      : base(db, audit, loggerFactory) { }

  public async Task<WalletSummaryDto> GetSummaryAsync(int memberId)
      => await ExecuteAsync(async () =>
      {
        var member = await _db.Members
            .Include(m => m.Country)
            .FirstOrDefaultAsync(m => m.Id == memberId)
            ?? throw new Exception($"Member {memberId} not found");

        var currencyCode = member.Country?.CurrencyCode ?? "USD";

        var rate = await _db.ExchangeRates
            .Where(r => r.CurrencyCode == currencyCode && r.EffectiveDatetime <= DateTime.UtcNow)
            .OrderByDescending(r => r.EffectiveDatetime)
            .FirstOrDefaultAsync();

        var cashBalance = await _db.WalletBalances
            .Where(w => w.MemberId == memberId && w.WalletType == WalletTypeConstants.Cash)
            .Select(w => w.Balance)
            .FirstOrDefaultAsync();

        var purchaseBalance = await _db.WalletBalances
            .Where(w => w.MemberId == memberId && w.WalletType == WalletTypeConstants.Purchase)
            .Select(w => w.Balance)
            .FirstOrDefaultAsync();

        return new WalletSummaryDto
        {
          CashBalance     = cashBalance,
          PurchaseBalance = purchaseBalance,
          CurrencyCode    = currencyCode,
          CurrencySymbol  = member.Country?.CurrencySymbol ?? string.Empty,
          ExchangeRate    = rate?.RateToBase ?? 1m
        };
      });

  public async Task<decimal> GetBalanceAsync(int memberId, string walletType)
      => await ExecuteAsync(async () =>
          await _db.WalletBalances
              .Where(w => w.MemberId == memberId && w.WalletType == walletType)
              .Select(w => w.Balance)
              .FirstOrDefaultAsync());

  public async Task PostTransactionAsync(PostTransactionDto dto)
      => await ExecuteAsync(async () => await InternalPostAsync(dto));

  public async Task PostAdjustmentAsync(
      int memberId, string walletType, decimal amountUsd,
      string direction, string remark, string createdBy)
      => await ExecuteAsync(async () =>
          await InternalPostAsync(new PostTransactionDto
          {
            MemberId   = memberId,
            WalletType = walletType,
            TxnType    = walletType == WalletTypeConstants.Cash
                ? CashTxnTypeConstants.Adjustment
                : PurchaseTxnTypeConstants.Adjustment,
            AmountUsd  = amountUsd,
            Direction  = direction,
            Remark     = remark,
            CreatedBy  = createdBy
          }));

  public async Task PostTransferAsync(
      int fromMemberId, int toMemberId, decimal amountUsd, string createdBy)
      => await ExecuteAsync(async () =>
      {
        var referenceId = $"TRF-{DateTime.UtcNow:yyyyMMddHHmmss}-{fromMemberId}-{toMemberId}";
        using var dbTx = await _db.Database.BeginTransactionAsync();
        try
        {
          await InternalPostAsync(new PostTransactionDto
          {
            MemberId    = fromMemberId,
            WalletType  = WalletTypeConstants.Purchase,
            TxnType     = PurchaseTxnTypeConstants.TransferOut,
            AmountUsd   = amountUsd,
            Direction   = WalletDirectionConstants.Out,
            ReferenceId = referenceId,
            CreatedBy   = createdBy
          });

          await InternalPostAsync(new PostTransactionDto
          {
            MemberId    = toMemberId,
            WalletType  = WalletTypeConstants.Purchase,
            TxnType     = PurchaseTxnTypeConstants.TransferIn,
            AmountUsd   = amountUsd,
            Direction   = WalletDirectionConstants.In,
            ReferenceId = referenceId,
            CreatedBy   = createdBy
          });

          await dbTx.CommitAsync();
        }
        catch
        {
          await dbTx.RollbackAsync();
          throw;
        }
      });

  public async Task<TransferValidationResult> ValidateTransferAsync(
      int fromMemberId, int toMemberId, decimal amountUsd)
      => await ExecuteAsync(async () =>
      {
        if (fromMemberId == toMemberId)
          return new TransferValidationResult { IsValid = false, Message = "Cannot transfer to yourself." };

        var toMember = await _db.Members.FindAsync(toMemberId);
        if (toMember == null || toMember.Status == StatusConstants.Deleted)
          return new TransferValidationResult { IsValid = false, Message = "Target member not found." };

        var isInFamily = await IsInSponsorFamilyAsync(fromMemberId, toMemberId);
        if (!isInFamily)
          return new TransferValidationResult
          {
            IsValid = false,
            Message = "Transfer only allowed within your sponsor family network."
          };

        var balance = await GetBalanceAsync(fromMemberId, WalletTypeConstants.Purchase);
        if (balance < amountUsd)
          return new TransferValidationResult { IsValid = false, Message = "Insufficient Purchase Wallet balance." };

        return new TransferValidationResult { IsValid = true, Target = toMember };
      });

  public async Task<(List<CashWalletTransaction> Items, int Total)> GetCashHistoryAsync(
      int memberId, int page, int pageSize)
      => await ExecuteAsync(async () =>
      {
        var q = _db.CashWalletTransactions
            .Where(t => t.MemberId == memberId)
            .OrderByDescending(t => t.Id);
        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
      });

  public async Task<(List<PurchaseWalletTransaction> Items, int Total)> GetPurchaseHistoryAsync(
      int memberId, int page, int pageSize)
      => await ExecuteAsync(async () =>
      {
        var q = _db.PurchaseWalletTransactions
            .Where(t => t.MemberId == memberId)
            .OrderByDescending(t => t.Id);
        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
      });

  public async Task ArchiveOldTransactionsAsync()
      => await ExecuteAsync(async () =>
      {
        var cutoff = DateTime.UtcNow.AddMonths(-12);

        var oldCash = await _db.CashWalletTransactions
            .Where(t => t.CreatedAt < cutoff)
            .ToListAsync();
        if (oldCash.Count > 0)
        {
          var archives = oldCash.Select(t => new CashWalletTransactionArchive
          {
            Id                = t.Id,
            MemberId          = t.MemberId,
            TxnType           = t.TxnType,
            AmountUsd         = t.AmountUsd,
            Direction         = t.Direction,
            BalanceAfter      = t.BalanceAfter,
            DisplayAmount     = t.DisplayAmount,
            DisplayCurrency   = t.DisplayCurrency,
            ExchangeRate      = t.ExchangeRate,
            ReferenceId       = t.ReferenceId,
            Remark            = t.Remark,
            IncentivePeriodId = t.IncentivePeriodId,
            PeriodDate        = t.PeriodDate,
            CreatedBy         = t.CreatedBy,
            CreatedAt         = t.CreatedAt
          }).ToList();
          _db.CashWalletTransactionArchives.AddRange(archives);
          _db.CashWalletTransactions.RemoveRange(oldCash);
        }

        var oldPurchase = await _db.PurchaseWalletTransactions
            .Where(t => t.CreatedAt < cutoff)
            .ToListAsync();
        if (oldPurchase.Count > 0)
        {
          var archives = oldPurchase.Select(t => new PurchaseWalletTransactionArchive
          {
            Id                = t.Id,
            MemberId          = t.MemberId,
            TxnType           = t.TxnType,
            AmountUsd         = t.AmountUsd,
            Direction         = t.Direction,
            BalanceAfter      = t.BalanceAfter,
            DisplayAmount     = t.DisplayAmount,
            DisplayCurrency   = t.DisplayCurrency,
            ExchangeRate      = t.ExchangeRate,
            ReferenceId       = t.ReferenceId,
            Remark            = t.Remark,
            IncentivePeriodId = t.IncentivePeriodId,
            PeriodDate        = t.PeriodDate,
            CreatedBy         = t.CreatedBy,
            CreatedAt         = t.CreatedAt
          }).ToList();
          _db.PurchaseWalletTransactionArchives.AddRange(archives);
          _db.PurchaseWalletTransactions.RemoveRange(oldPurchase);
        }

        await _db.SaveChangesAsync();
      });

  public async Task<(List<AdminWalletTxnRowDto> Items, int Total)> GetAllTransactionsAsync(
      string? filterUsername,
      string? filterWalletType,
      string? filterTxnType,
      string? filterCurrencyCode,
      DateTime? filterStartUtc,
      DateTime? filterEndUtc,
      int page,
      int pageSize)
      => await ExecuteAsync(async () =>
      {
        var cashQ = _db.CashWalletTransactions
            .Join(_db.Members, t => t.MemberId, m => m.Id,
                (t, m) => new { t, m })
            .Where(x => x.m.Status != StatusConstants.Deleted);

        if (!string.IsNullOrEmpty(filterUsername))
          cashQ = cashQ.Where(x => x.m.Username.Contains(filterUsername));
        if (!string.IsNullOrEmpty(filterTxnType))
          cashQ = cashQ.Where(x => x.t.TxnType == filterTxnType);
        if (!string.IsNullOrEmpty(filterCurrencyCode))
          cashQ = cashQ.Where(x => x.t.DisplayCurrency == filterCurrencyCode);
        if (filterStartUtc.HasValue)
          cashQ = cashQ.Where(x => x.t.CreatedAt >= filterStartUtc.Value);
        if (filterEndUtc.HasValue)
          cashQ = cashQ.Where(x => x.t.CreatedAt <= filterEndUtc.Value);

        var purchaseQ = _db.PurchaseWalletTransactions
            .Join(_db.Members, t => t.MemberId, m => m.Id,
                (t, m) => new { t, m })
            .Where(x => x.m.Status != StatusConstants.Deleted);

        if (!string.IsNullOrEmpty(filterUsername))
          purchaseQ = purchaseQ.Where(x => x.m.Username.Contains(filterUsername));
        if (!string.IsNullOrEmpty(filterTxnType))
          purchaseQ = purchaseQ.Where(x => x.t.TxnType == filterTxnType);
        if (!string.IsNullOrEmpty(filterCurrencyCode))
          purchaseQ = purchaseQ.Where(x => x.t.DisplayCurrency == filterCurrencyCode);
        if (filterStartUtc.HasValue)
          purchaseQ = purchaseQ.Where(x => x.t.CreatedAt >= filterStartUtc.Value);
        if (filterEndUtc.HasValue)
          purchaseQ = purchaseQ.Where(x => x.t.CreatedAt <= filterEndUtc.Value);

        var cashRows = cashQ.Select(x => new AdminWalletTxnRowDto
        {
          Id              = x.t.Id,
          MemberId        = x.m.Id,
          MemberUsername  = x.m.Username,
          MemberFullName  = x.m.FullName,
          WalletType      = WalletTypeConstants.Cash,
          TxnType         = x.t.TxnType,
          Direction       = x.t.Direction,
          AmountUsd       = x.t.AmountUsd,
          DisplayAmount   = x.t.DisplayAmount,
          DisplayCurrency = x.t.DisplayCurrency,
          ExchangeRate    = x.t.ExchangeRate,
          BalanceAfter    = x.t.BalanceAfter,
          ReferenceId     = x.t.ReferenceId,
          Remark          = x.t.Remark,
          CreatedBy       = x.t.CreatedBy,
          CreatedAt       = x.t.CreatedAt
        });

        var purchaseRows = purchaseQ.Select(x => new AdminWalletTxnRowDto
        {
          Id              = x.t.Id,
          MemberId        = x.m.Id,
          MemberUsername  = x.m.Username,
          MemberFullName  = x.m.FullName,
          WalletType      = WalletTypeConstants.Purchase,
          TxnType         = x.t.TxnType,
          Direction       = x.t.Direction,
          AmountUsd       = x.t.AmountUsd,
          DisplayAmount   = x.t.DisplayAmount,
          DisplayCurrency = x.t.DisplayCurrency,
          ExchangeRate    = x.t.ExchangeRate,
          BalanceAfter    = x.t.BalanceAfter,
          ReferenceId     = x.t.ReferenceId,
          Remark          = x.t.Remark,
          CreatedBy       = x.t.CreatedBy,
          CreatedAt       = x.t.CreatedAt
        });

        IQueryable<AdminWalletTxnRowDto> combined;
        if (filterWalletType == WalletTypeConstants.Cash)
          combined = cashRows;
        else if (filterWalletType == WalletTypeConstants.Purchase)
          combined = purchaseRows;
        else
          combined = cashRows.Concat(purchaseRows);

        var total = await combined.CountAsync();
        var items = await combined
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
      });

  public async Task<(List<AdminWalletTxnRowDto> Items, int Total)> GetAllAdjustmentsAsync(
      string? filterMemberUsername,
      string? filterWalletType,
      string? filterCreatedBy,
      DateTime? filterStartUtc,
      DateTime? filterEndUtc,
      int page,
      int pageSize)
      => await ExecuteAsync(async () =>
      {
        var cashQ = _db.CashWalletTransactions
            .Join(_db.Members, t => t.MemberId, m => m.Id,
                (t, m) => new { t, m })
            .Where(x => x.m.Status != StatusConstants.Deleted
                     && x.t.TxnType == CashTxnTypeConstants.Adjustment);

        if (!string.IsNullOrEmpty(filterMemberUsername))
          cashQ = cashQ.Where(x => x.m.Username.Contains(filterMemberUsername));
        if (!string.IsNullOrEmpty(filterCreatedBy))
          cashQ = cashQ.Where(x => x.t.CreatedBy.Contains(filterCreatedBy));
        if (filterStartUtc.HasValue)
          cashQ = cashQ.Where(x => x.t.CreatedAt >= filterStartUtc.Value);
        if (filterEndUtc.HasValue)
          cashQ = cashQ.Where(x => x.t.CreatedAt <= filterEndUtc.Value);

        var purchaseQ = _db.PurchaseWalletTransactions
            .Join(_db.Members, t => t.MemberId, m => m.Id,
                (t, m) => new { t, m })
            .Where(x => x.m.Status != StatusConstants.Deleted
                     && x.t.TxnType == PurchaseTxnTypeConstants.Adjustment);

        if (!string.IsNullOrEmpty(filterMemberUsername))
          purchaseQ = purchaseQ.Where(x => x.m.Username.Contains(filterMemberUsername));
        if (!string.IsNullOrEmpty(filterCreatedBy))
          purchaseQ = purchaseQ.Where(x => x.t.CreatedBy.Contains(filterCreatedBy));
        if (filterStartUtc.HasValue)
          purchaseQ = purchaseQ.Where(x => x.t.CreatedAt >= filterStartUtc.Value);
        if (filterEndUtc.HasValue)
          purchaseQ = purchaseQ.Where(x => x.t.CreatedAt <= filterEndUtc.Value);

        var cashRows = cashQ.Select(x => new AdminWalletTxnRowDto
        {
          Id              = x.t.Id,
          MemberId        = x.m.Id,
          MemberUsername  = x.m.Username,
          MemberFullName  = x.m.FullName,
          WalletType      = WalletTypeConstants.Cash,
          TxnType         = x.t.TxnType,
          Direction       = x.t.Direction,
          AmountUsd       = x.t.AmountUsd,
          DisplayAmount   = x.t.DisplayAmount,
          DisplayCurrency = x.t.DisplayCurrency,
          ExchangeRate    = x.t.ExchangeRate,
          BalanceAfter    = x.t.BalanceAfter,
          ReferenceId     = x.t.ReferenceId,
          Remark          = x.t.Remark,
          CreatedBy       = x.t.CreatedBy,
          CreatedAt       = x.t.CreatedAt
        });

        var purchaseRows = purchaseQ.Select(x => new AdminWalletTxnRowDto
        {
          Id              = x.t.Id,
          MemberId        = x.m.Id,
          MemberUsername  = x.m.Username,
          MemberFullName  = x.m.FullName,
          WalletType      = WalletTypeConstants.Purchase,
          TxnType         = x.t.TxnType,
          Direction       = x.t.Direction,
          AmountUsd       = x.t.AmountUsd,
          DisplayAmount   = x.t.DisplayAmount,
          DisplayCurrency = x.t.DisplayCurrency,
          ExchangeRate    = x.t.ExchangeRate,
          BalanceAfter    = x.t.BalanceAfter,
          ReferenceId     = x.t.ReferenceId,
          Remark          = x.t.Remark,
          CreatedBy       = x.t.CreatedBy,
          CreatedAt       = x.t.CreatedAt
        });

        IQueryable<AdminWalletTxnRowDto> combined;
        if (filterWalletType == WalletTypeConstants.Cash)
          combined = cashRows;
        else if (filterWalletType == WalletTypeConstants.Purchase)
          combined = purchaseRows;
        else
          combined = cashRows.Concat(purchaseRows);

        var total = await combined.CountAsync();
        var items = await combined
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
      });

  public async Task<List<MemberWalletBalanceRowDto>> GetAllBalancesAsync(
      string? filterUsername,
      string? filterCountryCode)
      => await ExecuteAsync(async () =>
      {
        var q = _db.Members
            .Include(m => m.Country)
            .Where(m => m.Status != StatusConstants.Deleted)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filterUsername))
          q = q.Where(m => m.Username.Contains(filterUsername));
        if (!string.IsNullOrEmpty(filterCountryCode))
          q = q.Where(m => m.CountryCode == filterCountryCode);

        var members    = await q.ToListAsync();
        var memberIds  = members.Select(m => m.Id).ToList();
        var balances   = await _db.WalletBalances
            .Where(b => memberIds.Contains(b.MemberId))
            .ToListAsync();

        return members.Select(m => new MemberWalletBalanceRowDto
        {
          MemberId        = m.Id,
          Username        = m.Username,
          FullName        = m.FullName,
          CountryCode     = m.CountryCode,
          CurrencyCode    = m.Country?.CurrencyCode ?? string.Empty,
          CashBalance     = balances.FirstOrDefault(b => b.MemberId == m.Id && b.WalletType == WalletTypeConstants.Cash)?.Balance ?? 0,
          PurchaseBalance = balances.FirstOrDefault(b => b.MemberId == m.Id && b.WalletType == WalletTypeConstants.Purchase)?.Balance ?? 0,
          UpdatedAt       = balances.Where(b => b.MemberId == m.Id).Max(b => (DateTime?)b.UpdatedAt) ?? DateTime.UtcNow
        })
        .OrderByDescending(r => r.CashBalance + r.PurchaseBalance)
        .ToList();
      });

  private async Task<bool> IsInSponsorFamilyAsync(int fromMemberId, int targetMemberId)
  {
    // Walk upline chain
    var current = await _db.Members.FindAsync(fromMemberId);
    while (current?.SponsorId != null)
    {
      if (current.SponsorId == targetMemberId) return true;
      current = await _db.Members.FindAsync(current.SponsorId);
    }

    // Walk downline (BFS through sponsor tree)
    var queue = new Queue<int>();
    queue.Enqueue(fromMemberId);
    while (queue.Count > 0)
    {
      var currentId = queue.Dequeue();
      var children  = await _db.Members
          .Where(m => m.SponsorId == currentId)
          .Select(m => m.Id)
          .ToListAsync();
      foreach (var childId in children)
      {
        if (childId == targetMemberId) return true;
        queue.Enqueue(childId);
      }
    }
    return false;
  }

  private async Task InternalPostAsync(PostTransactionDto dto)
  {
    var member = await _db.Members
        .Include(m => m.Country)
        .FirstOrDefaultAsync(m => m.Id == dto.MemberId)
        ?? throw new Exception($"Member {dto.MemberId} not found");

    var currencyCode = member.Country?.CurrencyCode ?? "USD";

    var rate = await _db.ExchangeRates
        .Where(r => r.CurrencyCode == currencyCode && r.EffectiveDatetime <= DateTime.UtcNow)
        .OrderByDescending(r => r.EffectiveDatetime)
        .FirstOrDefaultAsync();

    var exchangeRate  = rate?.RateToBase ?? 1m;
    var displayAmount = exchangeRate != 0 ? dto.AmountUsd / exchangeRate : dto.AmountUsd;

    var walletBalance = await _db.WalletBalances
        .FirstOrDefaultAsync(w => w.MemberId == dto.MemberId && w.WalletType == dto.WalletType);

    var currentBalance = walletBalance?.Balance ?? 0m;
    var newBalance = dto.Direction == WalletDirectionConstants.In
        ? currentBalance + dto.AmountUsd
        : currentBalance - dto.AmountUsd;

    if (walletBalance == null)
    {
      walletBalance = new WalletBalance
      {
        MemberId   = dto.MemberId,
        WalletType = dto.WalletType,
        Balance    = newBalance,
        UpdatedAt  = DateTime.UtcNow
      };
      _db.WalletBalances.Add(walletBalance);
    }
    else
    {
      walletBalance.Balance   = newBalance;
      walletBalance.UpdatedAt = DateTime.UtcNow;
    }

    if (dto.WalletType == WalletTypeConstants.Cash)
    {
      _db.CashWalletTransactions.Add(new CashWalletTransaction
      {
        MemberId          = dto.MemberId,
        TxnType           = dto.TxnType,
        AmountUsd         = dto.AmountUsd,
        Direction         = dto.Direction,
        BalanceAfter      = newBalance,
        DisplayAmount     = displayAmount,
        DisplayCurrency   = currencyCode,
        ExchangeRate      = exchangeRate,
        ReferenceId       = dto.ReferenceId,
        Remark            = dto.Remark,
        IncentivePeriodId = dto.IncentivePeriodId,
        PeriodDate        = dto.PeriodDate,
        CreatedBy         = dto.CreatedBy,
        CreatedAt         = DateTime.UtcNow
      });
    }
    else
    {
      _db.PurchaseWalletTransactions.Add(new PurchaseWalletTransaction
      {
        MemberId          = dto.MemberId,
        TxnType           = dto.TxnType,
        AmountUsd         = dto.AmountUsd,
        Direction         = dto.Direction,
        BalanceAfter      = newBalance,
        DisplayAmount     = displayAmount,
        DisplayCurrency   = currencyCode,
        ExchangeRate      = exchangeRate,
        ReferenceId       = dto.ReferenceId,
        Remark            = dto.Remark,
        IncentivePeriodId = dto.IncentivePeriodId,
        PeriodDate        = dto.PeriodDate,
        CreatedBy         = dto.CreatedBy,
        CreatedAt         = DateTime.UtcNow
      });
    }

    await _db.SaveChangesAsync();
  }
}
