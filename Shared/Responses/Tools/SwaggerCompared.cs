using Shared.Requests;
namespace Shared.Responses.Tools;
public sealed class SwaggerCompared : IResponse
{
    public int BytesGenerated { get; set; } = new();
    public string FilePath { get; set; } = string.Empty;
    public SwaggerCompared(string path, int bytesGenerated)
    { 
        BytesGenerated = bytesGenerated;
        FilePath = path;
    }
}
