using System.Net.Http.Json;
using cagmc.JwtAuth.WebApi.Service;
using Microsoft.AspNetCore.Mvc.Testing;

namespace cagmc.JwtAuth.WebApi.Test;

public abstract class TestBase(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    protected async Task<HttpClient> GetAuthenticatedClientAsync(string username, string password = "<PASSWORD>")
    {
        var client = factory.CreateClient();

        var loginRequest = new LoginRequest
        {
            Username = username,
            Password = password
        };
        
        var httpResponseMessage = await client.PostAsJsonAsync("/api/accounts/login", loginRequest);
        
        httpResponseMessage.EnsureSuccessStatusCode();
    
        var loginResponse = await httpResponseMessage.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResponse);
        
        var token = loginResponse.Token;
        Assert.False(string.IsNullOrEmpty(token), "Token should not be null or empty.");
    
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        return client;
    }
}