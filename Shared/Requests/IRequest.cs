using Microsoft.AspNetCore.Routing;

namespace Shared.Requests;
public interface IRequest
{
    public string Path { get; }
    void ConfigureRoutes(IEndpointRouteBuilder app);
}
