using System.Text.Json;
using Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace UnitTests.Api;

public class ApiSmokeTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private static readonly List<object[]> _endpoints = new();

    public ApiSmokeTests(WebApplicationFactory<Program> factory)
    {
        _client = factory
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development"); // Ensure Swagger is enabled
            })
            .CreateClient();
    }

    public static IEnumerable<object[]> EndpointData => _endpoints;

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public async ValueTask InitializeAsync()
    {
        if (_endpoints.Any())
            return;

        var swaggerEndpoints = await GetSwaggerEndpoints(_client);

        foreach (var e in swaggerEndpoints)
        {
            _endpoints.Add(new object[] { e.Method, e.Path });
        }
    }

    [Theory]
    [MemberData(nameof(EndpointData))]
    public async Task All_Endpoints_Should_Respond(string method, string url)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), url);
        var response = await _client.SendAsync(request);

        Assert.True((int)response.StatusCode < 500,
            $"Failed: {method} {url} → {(int)response.StatusCode}");
    }

    private static async Task<IEnumerable<(string Method, string Path)>> GetSwaggerEndpoints(HttpClient client)
    {
        var json = await client.GetStringAsync("/swagger/v1/swagger.json");
        using var doc = JsonDocument.Parse(json);

        var paths = doc.RootElement.GetProperty("paths");

        var endpoints = new List<(string Method, string Path)>();

        foreach (var path in paths.EnumerateObject())
        {
            foreach (var method in path.Value.EnumerateObject())
            {
                endpoints.Add((method.Name.ToUpperInvariant(), path.Name));
            }
        }

        return endpoints;
    }
}
