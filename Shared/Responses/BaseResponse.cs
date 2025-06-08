using Shared.Requests;
namespace Shared.Responses;
public class BaseResponse : IResponse
{
    public string? ErrorMessage { get; set; }
    public string? StatusCode { get; set; }
}
