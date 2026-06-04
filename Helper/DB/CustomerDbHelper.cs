using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Helper;
using MyApp.Models;
using Microsoft.Extensions.Logging;

namespace MyApp.Helper.DB;

public class CustomerDbHelper : DbHelper
{
  public CustomerDbHelper(AppDbContext db, AuditHelper audit, ILoggerFactory loggerFactory) : base(db, audit, loggerFactory) { }

  public async Task<Customer?> GetByEmailAsync(string email)
      => await ExecuteAsync(() => _db.Customers
          .FirstOrDefaultAsync(c => c.Email == email && c.IsActive));

  public async Task<Customer?> GetByIdAsync(int id)
      => await ExecuteAsync(async () => await _db.Customers.FindAsync(id));

  public async Task<bool> EmailExistsAsync(string email)
      => await ExecuteAsync(() => _db.Customers.AnyAsync(c => c.Email == email));

  public async Task UpdateLastLoginAsync(int customerId)
      => await ExecuteAsync(async () =>
      {
        var customer = await _db.Customers.FindAsync(customerId);
        if (customer != null)
        {
          customer.LastLogin = DateTime.UtcNow;
          await _db.SaveChangesAsync();
        }
      });

  public async Task UpdateLanguageAsync(int customerId, string languageCode)
      => await ExecuteAsync(async () =>
      {
        var customer = await _db.Customers.FindAsync(customerId);
        if (customer != null)
        {
          customer.LanguageCode = languageCode;
          await _db.SaveChangesAsync();
        }
      });
}
