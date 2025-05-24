using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Text.Json;
using Xunit;

namespace UnitTests.Api
{
    public class Smoke : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        public Smoke(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        public static async Task<IEnumerable<(string Method, string Path)>> GetSwaggerEndpoints(HttpClient client)
        {
            var response = await client.GetStringAsync("/swagger/v1/swagger.json");
            var json = JsonDocument.Parse(response);

            var paths = json.RootElement.GetProperty("paths");

            var endpoints = new List<(string, string)>();
            foreach (var path in paths.EnumerateObject())
            {
                foreach (var method in path.Value.EnumerateObject())
                {
                    endpoints.Add((method.Name.ToUpper(), path.Name));
                }
            }

            return endpoints;
        }

        //[Fact]
        //[MemberData(nameof(EndpointData), MemberType = typeof(Smoke))]
        //public async Task All_Endpoints_Should_Respond(string method, string url)
        //{
        //    var request = new HttpRequestMessage(new HttpMethod(method), url);
        //    var response = await _client.SendAsync(request);

        //    Assert.True((int)response.StatusCode < 500, $"Failed: {method} {url} → {(int)response.StatusCode}");
        //}
    }
}
