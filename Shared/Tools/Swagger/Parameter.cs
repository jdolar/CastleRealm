namespace Shared.Tools.Swagger;
public sealed class Parameter
{
    public string Name { get; set; } = default!;
    public string In { get; set; } = default!;
    public string Type { get; set; } = default!;
    public bool Required { get; set; }
}
