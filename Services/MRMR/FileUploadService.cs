using MyApp.Constants.MRMR;

namespace MyApp.Services.MRMR;

public class FileUploadService
{
  private readonly IWebHostEnvironment _env;
  private readonly IConfiguration _config;

  public FileUploadService(IWebHostEnvironment env, IConfiguration config)
  {
    _env = env;
    _config = config;
  }

  public async Task<string> SaveApplicationDocumentAsync(IFormFile file, string applicationId, string documentType)
  {
    ValidateFile(file);

    var uploadDir = _config["UploadPaths:ApplicationDocuments"] ?? "uploads/application-documents";
    var appFolder = Path.Combine(_env.WebRootPath, uploadDir.Replace('/', Path.DirectorySeparatorChar), applicationId);
    Directory.CreateDirectory(appFolder);

    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
    var filename = $"{documentType}_{Guid.NewGuid():N}{ext}";
    var fullPath = Path.Combine(appFolder, filename);

    using var stream = new FileStream(fullPath, FileMode.Create);
    await file.CopyToAsync(stream);

    return Path.Combine(uploadDir, applicationId, filename).Replace('\\', '/');
  }

  public void DeleteFile(string relativePath)
  {
    if (string.IsNullOrEmpty(relativePath)) return;
    var fullPath = Path.Combine(_env.WebRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
    if (File.Exists(fullPath)) File.Delete(fullPath);
  }

  private static void ValidateFile(IFormFile file)
  {
    if (file == null || file.Length == 0)
      throw new InvalidOperationException("No file provided.");

    var maxBytes = SubmissionConstants.MaxFileSizeMb * 1024 * 1024;
    if (file.Length > maxBytes)
      throw new InvalidOperationException($"File exceeds maximum size of {SubmissionConstants.MaxFileSizeMb}MB.");

    var allowedExtensions = SubmissionConstants.AllowedFileExtensions.Split(',');
    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
    if (!allowedExtensions.Contains(ext))
      throw new InvalidOperationException($"File type '{ext}' is not allowed.");
  }
}
