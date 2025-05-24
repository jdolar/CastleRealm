using Xunit;
using Domain.Tools;
namespace UnitTests.Domain.Tools;
public class Crypto
{
    private readonly AesEncryption aes = new();
    private const string encryptedValue = "pU0KDMI3g8MNKIug6EIWPGcoBC7cQmFZvsyG/KjSUQo=";
    private const string decryptedValue = "decoded aes value";
   
    [Fact]
    public void EncryptString_NotEmpty_Correct()
    {
        string encrypted = aes.EncyrptString(decryptedValue);
        Assert.NotEmpty(encrypted);
        Assert.Equal(encryptedValue, encrypted);
    }
    [Fact]
    public void DecryptString_NotEmpty_Correct()
    {
        string decrypted = aes.DecryptString(encryptedValue);
        Assert.NotEmpty(decrypted);
        Assert.Equal(decryptedValue, decrypted);
    }
    [Fact]
    public void EncryptDecryptString_NotEmpty_Correct()
    {
        string value = "I am test, so this run is on me!";
       
        string encrypted = aes.EncyrptString(value);
        Assert.NotEmpty(encrypted);
       
        string decrypted = aes.DecryptString(encrypted);
        Assert.NotEmpty(decrypted);
        Assert.Equal(value, decrypted);
    }
}
