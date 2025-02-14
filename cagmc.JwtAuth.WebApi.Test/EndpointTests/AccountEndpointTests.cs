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
            Username = "admin@cagmc.com",
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
        var client = await GetAuthenticatedClientAsync("admin@cagmc.com");
        
        // Act
        var logoutResponse = await client.PostAsync("/api/accounts/logout", null);
    
        // Assert
        logoutResponse.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);
    }

    [Theory]
    [InlineData(AuthenticationMode.Jwt, "/api/accounts/refresh-token")]
    [InlineData(AuthenticationMode.JwtWithCookie, "/api/accounts/refresh-token-cookie")]
    public async Task RefreshTokenAsync(AuthenticationMode authenticationMode, string endpoint)
    {
        // Arrange
        var client = Factory.CreateClient();
        
        var loginRequest = new LoginRequest
        {
            Username = "admin@cagmc.com",
            Password = "<PASSWORD>",
            AuthenticationMode = authenticationMode
        };
        
        var httpResponseMessage = await client.PostAsJsonAsync("/api/accounts/login", loginRequest);
        
        httpResponseMessage.EnsureSuccessStatusCode();
    
        RefreshTokenRequest refreshTokenRequest = null!;
        if (authenticationMode == AuthenticationMode.JwtWithCookie)
        {
            var cookie = httpResponseMessage.Headers.GetValues("Set-Cookie").FirstOrDefault();
            client.DefaultRequestHeaders.Add("Cookie", cookie!);
        }
        else
        {
            var loginResponse = await httpResponseMessage.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.NotNull(loginResponse);
        
            var token = loginResponse.Token;
            Assert.False(string.IsNullOrEmpty(token), "Token should not be null or empty.");

            refreshTokenRequest = new RefreshTokenRequest
            {
                RefreshToken = loginResponse.RefreshToken!
            };
        }
        
        // Act
        var refreshTokenResponse = await client.PostAsJsonAsync(endpoint, refreshTokenRequest);
        
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
        var client = await GetAuthenticatedClientAsync("admin@cagmc.com");
        
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