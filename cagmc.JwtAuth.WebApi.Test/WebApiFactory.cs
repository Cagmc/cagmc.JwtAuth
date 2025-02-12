using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace cagmc.JwtAuth.WebApi.Test;

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