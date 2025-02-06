using System.Net;
using System.Net.Http.Json;
using cagmc.JwtAuth.WebApi.Service;
using Microsoft.AspNetCore.Mvc.Testing;

namespace cagmc.JwtAuth.WebApi.Test.EndpointTests;

public sealed class AccountEndpointTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task LoginAsync()
    {
        // Arrange
        var client = factory.CreateClient();

        var loginRequest = new LoginRequest
        {
            Username = "admin",
            Password = "<PASSWORD>"
        };
        
        // Act
        var httpResponseMessage = await client.PostAsJsonAsync("/api/accounts/login", loginRequest);
        
        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();
        
        Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
    }
    
    [Fact]
    public async Task LogoutAsync()
    {
        // Arrange
        var client = factory.CreateClient();
        
        var loginRequest = new LoginRequest
        {
            Username = "admin",
            Password = "<PASSWORD>"
        };
        
        var httpResponseMessage = await client.PostAsJsonAsync("/api/accounts/login", loginRequest);
        
        httpResponseMessage.EnsureSuccessStatusCode();
    
        var loginResponse = await httpResponseMessage.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResponse);
        
        var token = loginResponse.Token;
        Assert.False(string.IsNullOrEmpty(token), "Token should not be null or empty.");
    
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        // Act
        var logoutResponse = await client.PostAsync("/api/accounts/logout", null);
    
        // Assert
        logoutResponse.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);
    }

    [Fact]
    public async Task RefreshTokenAsync()
    {
        // Arrange
        var client = factory.CreateClient();
        
        var loginRequest = new LoginRequest
        {
            Username = "admin",
            Password = "<PASSWORD>"
        };
        
        var httpResponseMessage = await client.PostAsJsonAsync("/api/accounts/login", loginRequest);
        
        httpResponseMessage.EnsureSuccessStatusCode();
    
        var loginResponse = await httpResponseMessage.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResponse);
        
        var token = loginResponse.Token;
        Assert.False(string.IsNullOrEmpty(token), "Token should not be null or empty.");

        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = loginResponse!.RefreshToken,
        };
        
        // Act
        var refreshTokenResponse = await client.PostAsJsonAsync("/api/accounts/refresh", refreshRequest);
        
        // Assert
        refreshTokenResponse.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, refreshTokenResponse.StatusCode);
        
        var refreshTokenResponseContent = await refreshTokenResponse.Content.ReadFromJsonAsync<RefreshTokenResponse>();
        Assert.False(string.IsNullOrEmpty(refreshTokenResponseContent?.Token), "Token should not be null or empty.");
    }
}