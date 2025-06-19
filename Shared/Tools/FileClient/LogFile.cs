using Microsoft.Extensions.Logging;
namespace Shared.Tools.FileClient;
public sealed class LogFile : BaseFile, IDisposable 
{
    private readonly StreamWriter _writer;
    public StreamWriter Writer => _writer;
    public LogFile(ILogger? logger = null) : base(logger) // Pass null if you don't need a logger here
    {
        _writer = new StreamWriter(new FileStream(
            Path.Combine(AppContext.BaseDirectory, "log.txt"),
            FileMode.Append,
            FileAccess.Write,
            FileShare.ReadWrite),
            System.Text.Encoding.UTF8)
        {
            AutoFlush = true
        };
    }
    public void Dispose()
    {
        _writer?.Dispose();
    }
}