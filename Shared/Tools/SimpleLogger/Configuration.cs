using Microsoft.Extensions.Logging;
namespace Shared.Tools.SimpleLogger;
public class Configuration
{
    public LogLevel MinLogLevel { get; set; } = LogLevel.Information;
    public bool IncludeTimestamp { get; set; } = true;
    public bool IncludeSeverity { get; set; } = true;
    public bool EnableFileLogging { get; set; } = false;
    public string? LogFilePath { get; set; }
    public StreamWriter? FileWriter { get; set; }
public override string ToString()
    {
        return $"MinLogLevel={MinLogLevel}, IncludeTimestamp={IncludeTimestamp}, IncludeSeverity={IncludeSeverity}, EnableFileLogging={EnableFileLogging}, LogFilePath={LogFilePath}";
    }
}
