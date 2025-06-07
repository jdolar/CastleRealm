using Microsoft.Extensions.Logging;
namespace Shared.Tools.SimpleLogger;
public class Provider : ILoggerProvider
{
    private readonly StreamWriter? _writer;
    private readonly Configuration? _config;
    public Provider(Configuration config)
    {
        if (!config!.IsFileLoggingEnabled) return;

        _config = config;
        string filePath = _config.LogFilePath ?? Path.Combine(AppContext.BaseDirectory, "log.txt");

        string? directory = Path.GetDirectoryName(filePath);
        if(!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory!);
        }

        _writer = new StreamWriter(new FileStream(
            filePath,
            FileMode.Append,
            FileAccess.Write,
            FileShare.ReadWrite),
            System.Text.Encoding.UTF8)
        {
            AutoFlush = true
        };
    }

    public ILogger CreateLogger(string categoryName) => new Logger(categoryName, _config!, _writer);
    public void Dispose() => _writer?.Dispose();
}
