using System.Globalization;
using System;
using System.Collections.Generic;
using MyApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
//using MyApp.Pages.JobsheetF;
using Microsoft.AspNetCore.Hosting;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static MyApp.Helper.AppConstants;
using MyApp.Data;


namespace MyApp.Helper
{

  public class SharedHelper
  {
    private readonly AppDbContext _context;
    private readonly IDbLocalizer _localizer;
    private readonly IWebHostEnvironment _env;
    private readonly IWebHostEnvironment _webHostEnvironment;
    public SharedHelper(AppDbContext context, IDbLocalizer localizer, IWebHostEnvironment webHostEnvironment)
    {
      _context = context;
      _localizer = localizer;
      _webHostEnvironment = webHostEnvironment;
    }

    public string GetCultureName(string CultureCode)
    {
      string _value = "";


      if (CultureCode == AppConstants.LanguageOption.Code_English)
      {
        _value = AppConstants.LanguageOption.Word_English;
      }
      else if (CultureCode == AppConstants.LanguageOption.Code_Chinese)
      {
        _value = AppConstants.LanguageOption.Word_Chinese;
      }
      else if (CultureCode == AppConstants.LanguageOption.Code_BahasaMalayu)
      {
        _value = AppConstants.LanguageOption.Word_BahasaMalayu;
      }

      return _value;

    }


    public async Task<List<MenuPermissions>> GetMenuAccess(Guid UserId, int MenuId)
    {
      var permissions = await _context.MenuPermissions
         .Where(p => p.UserId == UserId)
         .Where(p => p.MenuId == MenuId)
         //.Select(g => new Dto_Permission
         //{
         //  MenuId = g.MenuId,
         //  View = g.ViewAccess == AppConstants.DataStatus.Active,
         //  Edit = g.EditAccess == AppConstants.DataStatus.Active,
         //  Delete = g.DeleteAccess == AppConstants.DataStatus.Active
         //})
         .ToListAsync();

      return permissions;
    }

    




    

    public async Task<Customers?> GetCustomerDataByCustomerId(Guid customerId)
    {
      var data = await _context.Customers.FindAsync(customerId);

      return data;
    }



    public async Task<string> GetUploadFolder(string imageDir)
    {

      // 👣 Determine the target folder inside wwwroot
      var wwwRootPath = _webHostEnvironment.WebRootPath;

      string path = Path.Combine(wwwRootPath, imageDir);

      Directory.CreateDirectory(path);

      return path;
    }

    public async Task<bool> DeleteImage(string uploadPath, string imageFileName)
    {

      if (imageFileName != "")
      {
        string _UploadFolder = await GetUploadFolder(uploadPath);

        var ImageNameWithPath = Path.Combine(_UploadFolder, imageFileName);

        if (System.IO.File.Exists(ImageNameWithPath))
        {
          System.IO.File.Delete(ImageNameWithPath);
        }
      }

      return true;
    }

    public async Task<bool> UploadImage(IFormFile file, string newImageFileName, string uploadWebPath, bool isCompressed)
    {

      string imgFilePrefix = string.Empty;

      if (file != null && file.Length > 0)
      {
        var ext = Path.GetExtension(file.FileName).ToLower();
        if (ext is not (".jpg" or ".jpeg" or ".png"))
          return false;
        //return Page();
        string _UploadFolder = await GetUploadFolder(uploadWebPath);

        var NewImageNameWithPath = Path.Combine(_UploadFolder, newImageFileName);
        using var input = file.OpenReadStream();

        if (isCompressed && ext != ".png")
        {
          using var image = await SixLabors.ImageSharp.Image.LoadAsync(input);
          image.Mutate(x => x.Resize(new ResizeOptions
          {
            Mode = ResizeMode.Max,
            Size = new Size(1280, 1280)
          }));

          var encoder = new JpegEncoder { Quality = 70 };
          await image.SaveAsync(_UploadFolder, encoder);
        }
        else
        {
          using var output = System.IO.File.Create(NewImageNameWithPath);
          await input.CopyToAsync(output);
        }

      }

      return true;
    }

    public async Task<EmailTemplates?> GetEmailTemplate(string templateCode, string cultureCode)
    {
      var template = await _context.EmailTemplates
    .FirstOrDefaultAsync(t => t.TemplateKey == templateCode && t.CultureCode == cultureCode);

      return template;
    }

    public async Task<AdminUsers?> GetAdminUserDataByUserId(Guid userId)
    {
      var adminUser = await _context.AdminUsers
         .FirstOrDefaultAsync(u => u.UserId == userId);

      return adminUser;
    }

    public async Task<AdminUsers?> GetAdminUserDataByUsername(string username)
    {
      var adminUser = await _context.AdminUsers
         .FirstOrDefaultAsync(u => u.Username == username);

      return adminUser;
    }


    public async Task<string?> GetDepartmentCodeByUserId(Guid userId)
    {

      string? Output = "";
      var adminUser = await _context.AdminUsers
         .FirstOrDefaultAsync(u => u.UserId == userId);

      if (adminUser != null)
      {
        Output = adminUser.DepartmentCode;

      }
      return Output;
    }



    public async Task<string?> Get_SystemDefaults_Value_ToString(string keyCode)
    {
      if (string.IsNullOrWhiteSpace(keyCode))
        return null;

      var entry = await _context.SystemDefaults
          .Where(x => x.KeyCode == keyCode)
          .Select(x => x.KeyValue)
          .FirstOrDefaultAsync();

      return entry;
    }

    public async Task<int?> Get_SystemDefaults_Value_ToInt(string keyCode)
    {
      var value = await Get_SystemDefaults_Value_ToString(keyCode);
      return int.TryParse(value, out var result) ? result : null;
    }

    public async Task<double?> Get_SystemDefaults_Value_ToDouble(string keyCode)
    {
      var value = await Get_SystemDefaults_Value_ToString(keyCode);
      return double.TryParse(value, out var result) ? result : null;
    }


    public int Get_RandomNumber()
    {
      var random = new Random();
      var cacheBuster = random.Next(100000, 999999); // Generates a 6-digit random number

      return cacheBuster;
    }


    public async Task<string?> GetBranchByUserId(Guid userId)
    {

      string? Output = "";
      var branchuser = await _context.BranchUser.Where(p => p.UserId == userId).ToListAsync();
      for (int i = 0; i < branchuser.Count; i++)
      {
        var _branchid = branchuser[i].BranchId;
        var _branch = await _context.Branch.FindAsync(_branchid);
        if (_branch != null)
        {
          if (Output == "")
          {
            Output += _branch.BranchName;
          }
          else
          {
            Output += " | " + _branch.BranchName;
          }
        }
      }

      return Output;
    }

  }



}
