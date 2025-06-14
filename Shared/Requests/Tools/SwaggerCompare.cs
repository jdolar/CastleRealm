namespace Shared.Requests.Tools;
public sealed class SwaggerCompare : IRequest
{
    public List<string> Facades { get; set; } = new();
    public string FilesPath { get; set; } = string.Empty;
}
