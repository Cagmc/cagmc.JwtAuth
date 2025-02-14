using System.Net;
using Xunit.Abstractions;

namespace cagmc.JwtAuth.WebApi.Test.EndpointTests;

public sealed class ValueEndpointTests(ITestOutputHelper testOutputHelper, WebApiFactory factory) : TestBase(testOutputHelper, factory)
{
    [Fact]
    public async Task GetAnonymous()
    {
        // Arrange
        var client = Factory.CreateClient();
        
        // Act
        var response = await client.GetAsync("/api/values/anonymous");
        
        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task GetAuthenticated_Fails()
    {
        // Arrange
        var client = Factory.CreateClient();
        
        // Act
        var response = await client.GetAsync("/api/values/authenticated");
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task GetAuthenticated_Succeeds()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync("admin@cagmc.com");
        
        // Act
        var response = await client.GetAsync("/api/values/authenticated");
        
        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task GetAdmin_Fails()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync("reader@cagmc.com");
        
        // Act
        var response = await client.GetAsync("/api/values/admin");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task GetAdmin_Succeeds()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync("admin@cagmc.com");
        
        // Act
        var response = await client.GetAsync("/api/values/admin");
        
        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetRead_Fails()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync("admin@cagmc.com");
        
        // Act
        var response = await client.GetAsync("/api/values/read");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task GetRead_Success()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync("reader@cagmc.com");
        
        // Act
        var response = await client.GetAsync("/api/values/read");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task GetEdit_Fails()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync("reader@cagmc.com");
        
        // Act
        var response = await client.GetAsync("/api/values/edit");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task GetEdit_Success()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync("editor@cagmc.com");
        
        // Act
        var response = await client.GetAsync("/api/values/edit");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}