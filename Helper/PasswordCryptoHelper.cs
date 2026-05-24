using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace MyApp.Helper
{
public static class PasswordCryptoHelper
{
  private static string _encryptionKey = string.Empty;

  public static void Configure(IConfiguration configuration)
  {
    _encryptionKey = configuration["Security:EncryptionKey"]
        ?? throw new InvalidOperationException(
            "EncryptionKey is not configured. Add 'Security:EncryptionKey' to appsettings.json.");
  }

  private static string EncryptionKey
  {
    get
    {
      if (string.IsNullOrEmpty(_encryptionKey))
        throw new InvalidOperationException(
            "PasswordCryptoHelper is not configured. Call Configure() at startup.");
      return _encryptionKey;
    }
  }

  public static string Encrypt(string plainText)
  {
    byte[] clearBytes = Encoding.UTF8.GetBytes(plainText);
    using (Aes aes = Aes.Create())
    {
      var pdb = new Rfc2898DeriveBytes(
     password: EncryptionKey,
     salt: new byte[] {
        0x49, 0x76, 0x61, 0x6e,
        0x20, 0x4d, 0x65, 0x64,
        0x76, 0x65, 0x64, 0x65,
        0x76, 0x20, 0x4b, 0x65
     },
     iterations: 100_000, // Recommended: 100,000 or more
     hashAlgorithm: HashAlgorithmName.SHA256 // More secure than SHA1
 );


      aes.Key = pdb.GetBytes(32);
      aes.IV = pdb.GetBytes(16);

      using var ms = new MemoryStream();
      using var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
      cs.Write(clearBytes, 0, clearBytes.Length);
      cs.Close();

      return Convert.ToBase64String(ms.ToArray());
    }
  }

  public static string Decrypt(string cipherText)
  {
    byte[] cipherBytes = Convert.FromBase64String(cipherText);
    using (Aes aes = Aes.Create())
    {
      var pdb = new Rfc2898DeriveBytes(
    password: EncryptionKey,
    salt: new byte[] {
        0x49, 0x76, 0x61, 0x6e,
        0x20, 0x4d, 0x65, 0x64,
        0x76, 0x65, 0x64, 0x65,
        0x76, 0x20, 0x4b, 0x65
    },
    iterations: 100_000, // Recommended: 100,000 or more
    hashAlgorithm: HashAlgorithmName.SHA256 // More secure than SHA1
);


      aes.Key = pdb.GetBytes(32);
      aes.IV = pdb.GetBytes(16);

      using var ms = new MemoryStream();
      using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write);
      cs.Write(cipherBytes, 0, cipherBytes.Length);
      cs.Close();

      return Encoding.UTF8.GetString(ms.ToArray());
    }
  }
}
}

