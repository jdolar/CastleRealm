using Microsoft.Extensions.Logging;
namespace Shared.Tools.SimpleLogger;

public class Logger : ILogger
{
    private readonly string _category;
    private readonly Configuration _config;

    public Logger(string category, Configuration config)
    {
        _category = category;
        _config = config;
    }

    public IDisposable? BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= _config.MinLogLevel;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        string message = formatter(state, exception);
        string timestamp = _config.IncludeTimestamp ? $"{DateTime.UtcNow:O} - " : "";
        string severity = _config.IncludeSeverity ? $"[{logLevel}] " : "";

        string finalMessage = $"{timestamp}{severity}{message}";

        ConsoleColor color = GetColor(logLevel);
        lock (Console.Out)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(finalMessage);
            Console.ResetColor();
        }

        if (_config.EnableFileLogging && _config.FileWriter != null)
        {
            lock (_config.FileWriter)
            {
                _config.FileWriter.WriteLine(finalMessage);
            }
        }
    }

    private static ConsoleColor GetColor(LogLevel level) => level switch
    {
        LogLevel.Information => ConsoleColor.White,
        LogLevel.Debug => ConsoleColor.Cyan,
        LogLevel.Warning => ConsoleColor.Yellow,
        LogLevel.Error => ConsoleColor.Red,
        LogLevel.Critical => ConsoleColor.DarkRed,
        _ => ConsoleColor.Gray
    };
}
