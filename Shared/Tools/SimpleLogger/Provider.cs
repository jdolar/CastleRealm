using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Tools.FileClient;
namespace Shared.Tools.SimpleLogger;
public class Provider : ILoggerProvider
{
    private readonly Configuration? _config;
    private readonly LogFile? _logFile;
    public Provider(Configuration config)
    {
        if (!config!.IsFileLoggingEnabled) return;

        _config = config;
        _logFile = new();
        string filePath = _config.LogFilePath ?? Path.Combine(AppContext.BaseDirectory, "log.txt");

        string? directory = Path.GetDirectoryName(filePath);
        if(!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory!);
        }
    }

    public ILogger CreateLogger(string categoryName)
    {
        if (_config == null)
            return NullLogger.Instance;

        return new Logger(categoryName, _config, _logFile?.Writer);
    }
    public void Dispose() => _logFile?.Writer?.Dispose();
}
