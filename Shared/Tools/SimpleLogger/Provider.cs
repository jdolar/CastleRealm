using Microsoft.Extensions.Logging;
namespace Shared.Tools.SimpleLogger;

public class Provider : ILoggerProvider
{
    private readonly Configuration _config;

    public Provider(Configuration config)
    {
        _config = config;

        if (_config.EnableFileLogging && _config.FileWriter == null)
        {
            string? filePath = _config.LogFilePath ?? Path.Combine(AppContext.BaseDirectory, "log.txt");
            _config.FileWriter = new StreamWriter(filePath, append: true) { AutoFlush = true };
        }
    }

    public ILogger CreateLogger(string categoryName)
        => new Logger(categoryName, _config);

    public void Dispose()
    {
        _config.FileWriter?.Dispose();
    }
}
