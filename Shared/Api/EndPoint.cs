using Shared.Tools.Swagger;
namespace Shared.Api;
public sealed class Endpoint
{
    public string Operation { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Facade { get; set; } = string.Empty;
    public List<Parameter> Parameters { get; set; } = new();
}
