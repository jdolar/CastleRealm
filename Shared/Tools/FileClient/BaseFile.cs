using Microsoft.Extensions.Logging;
using System.Text;
namespace Shared.Tools.FileClient;
public class BaseFile(ILogger? logger)
{
    private readonly ILogger? _logger = logger;
    public async Task<string> ReadAllTextAsync(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            _logger?.LogError("❌ File path is null or empty.");
            return string.Empty;
        }
        if (!File.Exists(path))
        {
            _logger?.LogError("❌ File does not exist at {0}", path);
            return string.Empty;
        }

        try
        {
            return await File.ReadAllTextAsync(path);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "❌ Failed to read file at {0}", path);
            return string.Empty;
        }
    }
    public async Task<int> WriteAsync(string path, string? content)
    {
        if (string.IsNullOrEmpty(content))
        {
            _logger?.LogError("❌ Content is null or empty.");
            return 0;
        }

        if (string.IsNullOrEmpty(path))
        {
            _logger?.LogError("❌ File path is null or empty.");
            return 0;
        }

        try
        {
            using FileStream fileStream = new(path, FileMode.Create, FileAccess.Write);
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            await fileStream.WriteAsync(bytes, 0, bytes.Length);
            return bytes.Length;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "❌ Failed to write file at {0}", path);          
        }

        return 0;
    }
}