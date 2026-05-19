using MyApp.Data;

namespace MyApp.Helper.DB;

public class DbHelper
{
  protected readonly AppDbContext _db;
  public DbHelper(AppDbContext db) => _db = db;
}
