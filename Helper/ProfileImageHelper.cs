namespace MyApp.Helper;

public static class ProfileImageHelper
{
  public static async Task<string> SaveProfileImageAsync(IFormFile file, string username, string uploadPath)
  {
    var ext = Path.GetExtension(file.FileName).ToLower();
    if (ext is not (".jpg" or ".jpeg" or ".png"))
      throw new InvalidOperationException("Only .jpg, .jpeg, and .png files are allowed.");

    if (file.Length > 2 * 1024 * 1024)
      throw new InvalidOperationException("File size must not exceed 2MB.");

    Directory.CreateDirectory(uploadPath);

    var sanitized = new string(username.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
    var filename  = $"{Guid.NewGuid():N}_{sanitized}{ext}";
    var fullPath  = Path.Combine(uploadPath, filename);

    using var stream = System.IO.File.Create(fullPath);
    await file.CopyToAsync(stream);

    return filename;
  }
}
