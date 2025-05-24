using System.Security.Cryptography;
using System.Text;
namespace Shared.Tools.Crypto;
public sealed class Aes
{
    private readonly ICryptoTransform encryptor, decryptor;
    public Aes(string key, string iv)
    {
        using System.Security.Cryptography.Aes aesAlg = System.Security.Cryptography.Aes.Create();
        aesAlg.Key = Encoding.UTF8.GetBytes(key);  // Same Key used for encryption 16 bytes for AES-128, 24 for AES-192, 32 for AES-256
        aesAlg.IV = Encoding.UTF8.GetBytes(iv);   // Same IV used for encryption 16 bytes for AES

        encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
        decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
    }

    // Encrypt method
    public string Encrypt(string input)
    {
        using MemoryStream msEncrypt = new();
        using (CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write))
        {
            using StreamWriter swEncrypt = new(csEncrypt);
            swEncrypt.Write(input);
        }

        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    // Decrypt method
    public string Decrypt(string input)
    {
        using MemoryStream msDecrypt = new(Convert.FromBase64String(input));
        using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
        using StreamReader srDecrypt = new(csDecrypt);
        return srDecrypt.ReadToEnd();
    }
}
