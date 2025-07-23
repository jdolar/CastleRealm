using Shared.Tools.Swagger.Models;
namespace Shared.Api;
public sealed class Endpoint
{  
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Title { get; set; } = string.Empty;
    public List<Parameter>? Parameters { get; set; } = new();
    public List<Parameter>? RequestBody { get; set; } = new();
}