using Shared.Tools;
namespace DataBase.Collections.Castles;
public class ConnectionBuilder
{
    private static readonly AesCrypto aes = new(AesKey.Default, AesIv.Default);
    public static string DecryptOrGetDefault(string? connection = null)
    {
        if (connection is null)
        {
            return SQLServer.DefaultConnection;
        }

        return aes.Decrypt(connection);
    }
}
