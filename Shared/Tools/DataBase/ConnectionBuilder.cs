using Shared.Tools.Crypto;

namespace Shared.Tools.Database;

public class ConnectionBuilder
{
    private static readonly Aes aes = new(AesKey.Default, AesIv.Default);
    public static string DecryptOrGetDefault(string? connection = null)
    {
        if (connection is null)
        {
            return SQLServer.DefaultConnection;           
        }

        return aes.Decrypt(connection);
    }
}
