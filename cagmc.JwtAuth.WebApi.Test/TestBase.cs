using System.Net.Http.Json;
using System.Reflection;
using cagmc.JwtAuth.WebApi.Service;
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