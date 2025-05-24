using System.Security.Cryptography;
using System.Text;
namespace Shared.Tools;
#region Constants
public sealed class AesKey
{
    // AES-128 Crypto
    // 16 bytes key for AES-128 (128-bit key)
    // Same Key used for encryption 16 bytes for AES-128, 24 for AES-192, 32 for AES-256
    public const string Default = "1234567890123456";
}
public sealed class AesIv
{
    // AES-128 Crypto
    // 16 bytes IV
    // Same IV used for encryption 16 bytes for AES
    public const string Default = "1234567890123456";
}
#endregion
public sealed class AesCrypto
{
    private readonly ICryptoTransform encryptor, decryptor;
    public AesCrypto(string key, string iv)
    {
        using Aes aesAlg = Aes.Create();
        aesAlg.Key = Encoding.UTF8.GetBytes(key);
        aesAlg.IV = Encoding.UTF8.GetBytes(iv);

        encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
        decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
    }
    public string Encrypt(string input)
    {
        try
        {
            using MemoryStream msEncrypt = new();
            using (CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                using StreamWriter swEncrypt = new(csEncrypt);
                swEncrypt.Write(input);
            }

            return Convert.ToBase64String(msEncrypt.ToArray());
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Encryption failed", ex);
        }
    }
    public string Decrypt(string input)
    {
        try
        {
            using MemoryStream msDecrypt = new(Convert.FromBase64String(input));
            using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new(csDecrypt);
            return srDecrypt.ReadToEnd();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Decryption failed", ex);
        }
    }
}
