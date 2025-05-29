using Microsoft.Extensions.Logging;
namespace Shared.Tools.SimpleLogger;
public class Configuration
{
    public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;
    public bool IncludeTimestamp { get; set; } = true;
    public bool IncludeLogLevel { get; set; } = true;
    public bool IsFileLoggingEnabled { get; set; } = false;
    public string? LogFilePath { get; set; }
    public override string ToString()
    {
        return string.Format("MinimumLogLevel={0} IncludeTimestamp={1} IncludeLogLevel={2} IsFileLoggingEnabled={3} LogFilePath={4}",
            MinimumLogLevel, IncludeTimestamp, IncludeLogLevel, IsFileLoggingEnabled, LogFilePath);
    }
}
