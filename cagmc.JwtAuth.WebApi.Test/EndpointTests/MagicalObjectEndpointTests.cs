using System.Net;

using Xunit.Abstractions;

namespace cagmc.JwtAuth.WebApi.Test.EndpointTests;

public sealed class MagicalObjectEndpointTests(ITestOutputHelper testOutputHelper, WebApiFactory factory)
    : TestBase(testOutputHelper, factory)
{
    [Fact]
    public async Task GetMagicalObjects()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync("admin@cagmc.com");

        // Act
        var response = await client.GetAsync("/api/magical-objects");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}