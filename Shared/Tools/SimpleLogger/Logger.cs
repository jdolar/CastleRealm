using Microsoft.Extensions.Logging;
namespace Shared.Tools.SimpleLogger;
public sealed class Logger : ILogger
{
    private readonly string _category;
    private readonly Configuration _config;
    private readonly StreamWriter? _writer;

    public Logger(string category, Configuration config, StreamWriter? writer)
    {
        _category = category;
        _config = config;
        _writer = writer;
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel >= _config.MinimumLogLevel;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        string message = formatter(state, exception);
        string timestamp = _config.IncludeTimestamp ? $"{DateTime.UtcNow} - " : "";      
        string severity = _config.IncludeLogLevel ? $"[{logLevel}] " : "";
        string category = _config.IncludeCategory ? $"[{_category}]" : "";

        string finalMessage = $"{timestamp}{severity}{category}{message}";

        ConsoleColor color = GetColor(logLevel);
        lock (Console.Out)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(finalMessage);
            Console.ResetColor();
        }

        if (_config.IsFileLoggingEnabled && _writer != null)
        {
            lock (_writer!)
            {
                _writer.WriteLine(finalMessage);
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
    IDisposable? ILogger.BeginScope<TState>(TState state)
    {
        return NullScope.Instance;
    }
}
public sealed class NullScope : IDisposable
{
    public static NullScope Instance { get; } = new NullScope();
    private NullScope() { }
    public void Dispose() { }
}

