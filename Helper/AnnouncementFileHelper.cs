using MyApp.Constants;

namespace MyApp.Helper;

public static class AnnouncementFileHelper
{
  private static readonly HashSet<string> AllowedExtensions =
      new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".pdf" };

  public static async Task<(string FileName, string FileType)> SaveAsync(
      IFormFile file, string uploadPath)
  {
    var ext = Path.GetExtension(file.FileName).ToLower();

    if (!AllowedExtensions.Contains(ext))
      throw new InvalidOperationException("Only .jpg, .jpeg, .png, .gif and .pdf files are allowed.");

    if (file.Length > AnnouncementConstants.MaxFileSizeBytes)
      throw new InvalidOperationException("File exceeds 5 MB limit.");

    Directory.CreateDirectory(uploadPath);

    var baseName  = Path.GetFileNameWithoutExtension(file.FileName);
    var sanitized = new string(baseName.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-').ToArray());
    if (sanitized.Length == 0) sanitized = "file";

    var fileName = $"{Guid.NewGuid():N}_{sanitized}{ext}";
    var fullPath = Path.Combine(uploadPath, fileName);

    using var stream = System.IO.File.Create(fullPath);
    await file.CopyToAsync(stream);

    var fileType = ext == ".pdf" ? AnnouncementConstants.FileTypePdf : AnnouncementConstants.FileTypeImage;
    return (fileName, fileType);
  }
}
