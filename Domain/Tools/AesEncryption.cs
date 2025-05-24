using Shared.Tools;
namespace Domain.Tools;
public sealed class AesEncryption       
{    
    public string EncyrptString(string input, string? aesKey = null, string? aesIv = null)
    {
        aesKey ??= AesKey.Default;
        aesIv ??= AesIv.Default;

        AesCrypto aes = new(aesKey, aesIv);
        return aes.Encrypt(input);
    }
    public string DecryptString(string input, string? aesKey = null, string? aesIv = null)
    {
        aesKey ??= AesKey.Default;
        aesIv ??= AesIv.Default;

        AesCrypto aes = new(aesKey, aesIv);
        return aes.Decrypt(input);
    }
}
