using MyApp.Data;
using MyApp.Dtos;
using MyApp.Helper;
using MyApp.Models;
//using MyApp.Pages.UserF;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;


namespace MyApp.Services
{
  public class MenuService
  {
    private readonly AppDbContext _context;
    private readonly SharedHelper _sharedhelper;
    private readonly IDbLocalizer _localizer;
    //private readonly JobsheetHelper _jobsheethelper;

    public MenuService(AppDbContext context, IDbLocalizer localizer, SharedHelper sharedHelper)
    {
      //, JobsheetHelper jobsheetHelper
      _localizer = localizer;
      _context = context;
      _sharedhelper = sharedHelper;
      //_jobsheethelper = jobsheetHelper;
    }


    public async Task<List<MenuItems>> GetMenuForUserAsync(Guid UserId)
    {

      //var permittedIds = await _context.MenuPermissions
      //    .Where(p => p.UserId == UserId)
      //    .Select(p => p.MenuId)
      //    .ToListAsync();

      //var items = await _context.MenuItems
      //    .Where(m => permittedIds.Contains(m.MenuId))
      //     .Where(m => m.IsActive == AppConstants.DataStatus.Active)
      //    .ToListAsync();

      //// Build hierarchy
      //var lookup = items.ToLookup(i => i.ParentId);
      //foreach (var item in items)
      //  item.Children = lookup[item.MenuId].ToList();

      //var data = lookup[0].ToList();

      // Get permitted menu IDs for the user
      // Get permitted menu IDs for the user
      // Step 1: Get permitted menu IDs for the user
      var allItems = await _context.MenuItems
         .Where(m => m.IsActive == AppConstants.DataStatus.Active)
         .ToListAsync();

      var permittedIds = await _context.MenuPermissions
    .Where(p => p.UserId == UserId)
    .Select(p => p.MenuId)
    .ToListAsync();

      // Build a set of permitted + ancestor IDs
      var allPermitted = new HashSet<int>(permittedIds);

      void AddAncestors(int menuId)
      {
        var parentId = allItems.FirstOrDefault(m => m.MenuId == menuId)?.ParentId;
        if (parentId != null && parentId != 0 && allPermitted.Add(parentId.Value))
        {
          AddAncestors(parentId.Value);
        }
      }

      foreach (var id in permittedIds)
      {
        AddAncestors(id);
      }

      var items = allItems.Where(m => allPermitted.Contains(m.MenuId)).ToList();

      var lookup = items.ToLookup(i => i.ParentId);

      foreach (var item in items)
      {
        item.Children = lookup[item.MenuId].ToList();
      }

      var data = lookup[0].ToList(); // Now this should work!

      return data; // Root items
    }

    //public async Task<int> GetJobsheetCountInProgressAsync(Guid UserId)
    //{
    //  int JobCount = 0;
    //  var jobsheet = await _jobsheethelper.GetJobsheetDataByUserId_Status_InProgress(UserId);

    //  if (jobsheet != null)
    //  {
    //    JobCount = jobsheet.Count;
    //  }
    //  else
    //  {
    //    JobCount = 0;
    //  }


    //  return JobCount;
    //}

    public async Task <List<MenuPermissions>> GetMenuAccess(Guid UserId, int MenuId)
    {
      var permissions = await _sharedhelper.GetMenuAccess(UserId, MenuId);

      return permissions;
    }

    public async Task<bool> CheckMenuAccessType(Guid UserId, int MenuId, string AccessType)
    {
      bool CheckAccess = false;

      var MenuPermission = await _context.MenuPermissions
        .Where(m => m.MenuId == MenuId)
        .Where(m => m.UserId == UserId)
         .ToListAsync();

      if (MenuPermission.Count > 0)
      {
        if (AccessType == AppConstants.MenuAccessType.View)
        {
          CheckAccess = true;
        }
        else if (AccessType == AppConstants.MenuAccessType.Edit)
        {
          CheckAccess = true;
        }
        else if (AccessType == AppConstants.MenuAccessType.Delete)
        {
          CheckAccess = true;
        }
        else
        {
          CheckAccess = false;
        }
      }
      else
      {
        CheckAccess = false;
      }

      return CheckAccess;
    }

  }

}
