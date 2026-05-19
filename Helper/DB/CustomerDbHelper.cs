using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;

namespace MyApp.Helper.DB;

public class CustomerDbHelper : DbHelper
{
  public CustomerDbHelper(AppDbContext db) : base(db) { }

  public async Task<Customer?> GetByEmailAsync(string email)
      => await _db.Customers
          .FirstOrDefaultAsync(c => c.Email == email && c.IsActive);

  public async Task<Customer?> GetByIdAsync(int id)
      => await _db.Customers.FindAsync(id);

  public async Task<bool> EmailExistsAsync(string email)
      => await _db.Customers.AnyAsync(c => c.Email == email);

  public async Task UpdateLastLoginAsync(int customerId)
  {
    var customer = await _db.Customers.FindAsync(customerId);
    if (customer != null)
    {
      customer.LastLogin = DateTime.UtcNow;
      await _db.SaveChangesAsync();
    }
  }

  public async Task UpdateLanguageAsync(int customerId, string languageCode)
  {
    var customer = await _db.Customers.FindAsync(customerId);
    if (customer != null)
    {
      customer.LanguageCode = languageCode;
      await _db.SaveChangesAsync();
    }
  }
}
