namespace DataBase.Collections.Castles;
public sealed class SQLServer
{
    // SQL Server
    public const string Host = "DESKTOP-2ORAPH8";
    public const string Instance = "SQLHOME";
    public const string Database = "PerfSvc";
    public const string Format = "Server={0}\\{1};Database={2};Trusted_Connection=true;TrustServerCertificate=true";
    public static string DefaultConnection = FormatConnectionString(Host, Instance, Database);
    public static string FormatConnectionString(string host, string instance, string database) => string.Format(Format, host, instance, database);
}
