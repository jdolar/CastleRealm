using Xunit;
using Shared.Tools;
namespace UnitTests.Shared.Tools;
public class Crypto
{  
    [Fact]
    public void Encrypt__WithDefaultKey_WithDefaultIv_NotEmpty_Correct()
    {
        AesCrypto aes = new(AesKey.Default, AesIv.Default);
        string value = "I am test, so this run is on me!";
        
        string encrypted = aes.Encrypt(value);
        Assert.NotEmpty(encrypted);
        
        string decrypted = aes.Decrypt(encrypted);
        Assert.NotEmpty(decrypted);
        Assert.Equal(value, decrypted);
    }

    [Fact]
    public void EncryptDecrypt_WithSetKey_WithSetIv_NotEmpty_Correct()
    {
        string key = "1234567890123456";
        string iv = "1234567890123456";
        string value = "I am test, so this run is on me!";
        AesCrypto aes = new(key, iv);

        string encrypted = aes.Encrypt(value);
        Assert.NotEmpty(encrypted);
        
        string decrypted = aes.Decrypt(encrypted);
        Assert.NotEmpty(decrypted);
        Assert.Equal(value, decrypted);
    }
}
