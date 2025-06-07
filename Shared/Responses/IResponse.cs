using Microsoft.AspNetCore.Routing;
namespace Shared.Requests;
public interface IResponse
{
    public string Path { get; }
    void ConfigureRoutes(IEndpointRouteBuilder app);
}
