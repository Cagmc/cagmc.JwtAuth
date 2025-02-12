using System.Net;
using System.Net.Http.Json;
using cagmc.JwtAuth.WebApi.Common.Constants;
using cagmc.JwtAuth.WebApi.Common.Enum;
using cagmc.JwtAuth.WebApi.Service;
using Xunit.Abstractions;

namespace cagmc.JwtAuth.WebApi.Test.EndpointTests;

public sealed class AccountEndpointTests(ITestOutputHelper testOutputHelper, WebApiFactory factory) : TestBase(testOutputHelper, factory)
{
    [Theory]
    [InlineData(AuthenticationMode.Jwt)]
    [InlineData(AuthenticationMode.Cookie)]
    public async Task LoginAsync(AuthenticationMode authenticationMode)
    {
        // Arrange
        var client = Factory.CreateClient();

        var loginRequest = new LoginRequest
        {
            Username = "admin",
            Password = "<PASSWORD>",
            AuthenticationMode = authenticationMode
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
        var client = await GetAuthenticatedClientAsync("admin");
        
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
        var client = Factory.CreateClient();
        
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
            RefreshToken = loginResponse.RefreshToken,
        };
        
        // Act
        var refreshTokenResponse = await client.PostAsJsonAsync("/api/accounts/refresh", refreshRequest);
        
        // Assert
        refreshTokenResponse.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, refreshTokenResponse.StatusCode);
        
        var refreshTokenResponseContent = await refreshTokenResponse.Content.ReadFromJsonAsync<RefreshTokenResponse>();
        Assert.False(string.IsNullOrEmpty(refreshTokenResponseContent?.Token), "Token should not be null or empty.");
    }

    [Fact]
    public async Task MeAsync()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync("admin");
        
        // Act
        var meResponse = await client.GetAsync("/api/accounts/me");
        
        // Assert
        meResponse.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var viewModel = await meResponse.Content.ReadFromJsonAsync<MeViewModel>();
        
        Assert.NotNull(viewModel);
        Assert.Equal(Roles.Admin, viewModel.Role);
    }
}