using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;
using System.Security.Cryptography;

namespace MyApp.Helper.DB;

public class PasswordResetTokenDbHelper : DbHelper
{
  public PasswordResetTokenDbHelper(AppDbContext db) : base(db) { }

  public async Task<PasswordResetToken> CreateAsync(string userType, int userId)
  {
    var existing = await _db.PasswordResetTokens
        .Where(t => t.UserType == userType && t.UserId == userId && !t.IsUsed)
        .ToListAsync();
    foreach (var t in existing) t.IsUsed = true;

    var token = new PasswordResetToken
    {
      UserType = userType,
      UserId = userId,
      Token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLower(),
      ExpiresAt = DateTime.Now.AddHours(24),
      IsUsed = false,
      CreatedAt = DateTime.Now
    };

    _db.PasswordResetTokens.Add(token);
    await _db.SaveChangesAsync();
    return token;
  }

  public async Task<PasswordResetToken?> GetValidTokenAsync(string token)
      => await _db.PasswordResetTokens
          .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed && t.ExpiresAt > DateTime.Now);

  public async Task MarkUsedAsync(int id)
  {
    var token = await _db.PasswordResetTokens.FindAsync(id);
    if (token != null)
    {
      token.IsUsed = true;
      await _db.SaveChangesAsync();
    }
  }
}
