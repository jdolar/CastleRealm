using Microsoft.AspNetCore.Routing;
namespace Shared.Api;
public interface IEndPoint
{
    public string Path { get; }
    void ConfigureRoutes(IEndpointRouteBuilder app);
}
