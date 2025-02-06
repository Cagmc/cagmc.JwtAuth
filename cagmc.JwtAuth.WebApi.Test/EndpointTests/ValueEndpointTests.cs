using System.Net;
using System.Net.Http.Json;
using cagmc.JwtAuth.WebApi.Service;
using Microsoft.AspNetCore.Mvc.Testing;

namespace cagmc.JwtAuth.WebApi.Test.EndpointTests;

public sealed class ValueEndpointTests(WebApplicationFactory<Program> factory) : TestBase(factory)
{
    [Fact]
    public async Task GetAnonymous()
    {
        // Arrange
        var client = factory.CreateClient();
        
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
        var client = factory.CreateClient();
        
        // Act
        var response = await client.GetAsync("/api/values/authenticated");
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task GetAuthenticated_Succeeds()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync("admin");
        
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
        var client = await GetAuthenticatedClientAsync("customer");
        
        // Act
        var response = await client.GetAsync("/api/values/admin");
        
        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    
    [Fact]
    public async Task GetAdmin_Succeeds()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync("admin");
        
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
        var client = await GetAuthenticatedClientAsync("admin");
        
        // Act
        var response = await client.GetAsync("/api/values/read");
        
        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    
    [Fact]
    public async Task GetRead_Success()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync("reader");
        
        // Act
        var response = await client.GetAsync("/api/values/read");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task GetEdit_Fails()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync("reader");
        
        // Act
        var response = await client.GetAsync("/api/values/edit");
        
        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    
    [Fact]
    public async Task GetEdit_Success()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync("editor");
        
        // Act
        var response = await client.GetAsync("/api/values/edit");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}