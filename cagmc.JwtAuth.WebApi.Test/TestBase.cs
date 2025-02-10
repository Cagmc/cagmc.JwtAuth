using System.Net.Http.Json;
using System.Reflection;
using cagmc.JwtAuth.WebApi.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace cagmc.JwtAuth.WebApi.Test;

public abstract class TestBase : IClassFixture<WebApiFactory>
{
    protected WebApiFactory Factory { get; }

    protected TestBase(ITestOutputHelper testOutputHelper, WebApiFactory factory)
    {
        Factory = factory;
        Factory.TestId = GetTestName(testOutputHelper);
    }

    private static string GetTestName(ITestOutputHelper testOutputHelper)
    {
        var type = testOutputHelper.GetType();
        var testMember = type.GetField("test", BindingFlags.Instance | BindingFlags.NonPublic);
        var test = (ITest)testMember!.GetValue(testOutputHelper)!;
        var testName = test.TestCase.DisplayName;
        
        testOutputHelper.WriteLine($"Running test: {testName}");
        
        return testName;
    }
    
    protected async Task<HttpClient> GetAuthenticatedClientAsync(string username, string password = "<PASSWORD>")
    {
        var client = Factory.CreateClient();

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

public sealed class WebApiFactory : WebApplicationFactory<Program>
{
    public string TestId { get; set; } = string.Empty;
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            var configurationRoot = configurationBuilder.Build();
            var connectionString =configurationRoot.GetValue<string>("ConnectionStrings:DefaultConnection");
            connectionString = connectionString!.Replace("{TestId}", TestId);
            
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString
            }!);
        });
    }
}