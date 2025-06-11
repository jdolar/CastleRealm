using Shared.Tools.Swagger;
namespace Shared.Api;
public sealed class Endpoint
{
    public string? Operation { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Tags { get; set; } = string.Empty;
    public string MachParameter { get; set; } = string.Empty;
    public string Segments { get; set; } = string.Empty;
    public string? Title { get; set; } = string.Empty;
    public List<Parameter> Parameters { get; set; } = new();
    public List<Parameter> RequestBody { get; set; } = new();
}
/*
Path (basicaly url)
Name(matching attribut)
Method (POST, GET, PUT, DELETE)
Operation (in one facade maybe usefull info, can always remove it later)
Parameters
    Name    
    Type (string, int, etc)
    Required
    In (query, body, path, header)
 */