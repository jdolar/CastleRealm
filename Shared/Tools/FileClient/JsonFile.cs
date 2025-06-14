using Microsoft.Extensions.Logging;
using Shared.Api;
using Shared.Tools.Swagger.Models;
using System.Text;
namespace Shared.Tools.FileClient;
public sealed class JsonFile(ILogger logger) : BaseFile(logger)
{

}
