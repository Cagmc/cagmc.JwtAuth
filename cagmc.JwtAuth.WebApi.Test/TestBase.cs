using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

using cagmc.JwtAuth.WebApi.Application.Services;
using cagmc.JwtAuth.WebApi.Common.Enum;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Xunit.Abstractions;

namespace cagmc.JwtAuth.WebApi.Test;

public abstract class TestBase : IClassFixture<WebApiFactory>
{
    protected TestBase(ITestOutputHelper testOutputHelper, WebApiFactory factory)
    {
        Factory = factory;
        Factory.TestId = GetTestName(testOutputHelper);
        Configuration = Factory.Services.GetRequiredService<IConfiguration>();

        JsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };
    }

    protected JsonSerializerOptions JsonSerializerOptions { get; }
    protected WebApiFactory Factory { get; }
    protected IConfiguration Configuration { get; }

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
        var authenticationMode = Configuration.GetValue<AuthenticationMode>("AuthenticationMode");

        var loginRequest = new LoginRequest
        {
            Username = username,
            Password = password,
            AuthenticationMode = authenticationMode
        };

        var httpResponseMessage = await client.PostAsJsonAsync("/api/accounts/login", loginRequest);
        httpResponseMessage.EnsureSuccessStatusCode();

        if (authenticationMode == AuthenticationMode.Jwt)
        {
            var loginResponse = await httpResponseMessage.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.NotNull(loginResponse);

            var token = loginResponse.Token;
            Assert.False(string.IsNullOrEmpty(token), "Token should not be null or empty.");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            var cookie = httpResponseMessage.Headers.GetValues("Set-Cookie").FirstOrDefault();
            client.DefaultRequestHeaders.Add("Cookie", cookie!);
        }

        return client;
    }
}