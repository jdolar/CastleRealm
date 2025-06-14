using Microsoft.Extensions.Logging;
namespace Shared.Tools.FileClient;
public sealed class LogFile : BaseFile
{
    public StreamWriter Writer;
    public LogFile(ILogger? logger = null) : base(logger) // Pass null if you don't need a logger here
    {
        Writer = new StreamWriter(new FileStream(
            Path.Combine(AppContext.BaseDirectory, "log.txt"),
            FileMode.Append,
            FileAccess.Write,
            FileShare.ReadWrite),
            System.Text.Encoding.UTF8)
        {
            AutoFlush = true
        };
    }
}