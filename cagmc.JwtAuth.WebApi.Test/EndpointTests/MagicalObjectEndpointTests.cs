using System.Globalization;
using System.Net;
using System.Net.Http.Json;

using cagmc.JwtAuth.WebApi.Application.Services;
using cagmc.JwtAuth.WebApi.Common.Enum;
using cagmc.JwtAuth.WebApi.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Xunit.Abstractions;

namespace cagmc.JwtAuth.WebApi.Test.EndpointTests;

public sealed class MagicalObjectEndpointTests(ITestOutputHelper testOutputHelper, WebApiFactory factory)
    : TestBase(testOutputHelper, factory)
{
    [Fact]
    public async Task GetMagicalObjects()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync("admin@cagmc.com");

        var query =
            $"nameFilter=fi" +
            $"&discoveredFrom={DateTime.UtcNow.AddDays(-10):o}" +
            $"&discoveredTo={DateTime.UtcNow:o}" +
            $"&elementalFilterSet=None" +
            $"&elementalFilterSet=Fire";

        // Act
        var response = await client.GetAsync($"/api/magical-objects?{query}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateMagicalObject()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync("admin@cagmc.com");

        var request = new CreateMagicalObjectRequest
        {
            Name = "Elder Wand",
            Discovered = DateTime.Parse("1824.04.16", new DateTimeFormatInfo()),
            Elemental = ElementalType.Earth,
            Description = "The most powerful wand in the world.",
            Properties =
            [
                new CreateMagicalPropertyRequest { Name = "Power", Value = "1000" }
            ]
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/magical-objects", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();

        var createdEntity = await dbContext.Set<MagicalObject>()
            .Where(x => x.Name == request.Name)
            .FirstOrDefaultAsync();

        Assert.NotNull(createdEntity);
        Assert.Equal(request.Name, createdEntity!.Name);
        Assert.Equal(request.Discovered, createdEntity.Discovered);
        Assert.Equal(request.Elemental, createdEntity.Elemental);
        Assert.Equal(request.Description, createdEntity.Description);
        Assert.Equal(request.Properties.Count, createdEntity.Properties.Count);
        Assert.Equal(request.Properties[0].Name, createdEntity.Properties[0].Name);
        Assert.Equal(request.Properties[0].Value, createdEntity.Properties[0].Value);
    }

    [Fact]
    public async Task DeleteMagicalObject()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync("admin@cagmc.com");

        var magicalObject = new MagicalObject
        {
            Name = "Test Magical Object",
            Elemental = ElementalType.Poison,
            Description = null,
            Discovered = DateTime.Today,
            Properties =
            [
                new MagicalProperty { Name = "Poison", Value = "100" }
            ]
        };

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();
            dbContext.Add(magicalObject);
            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await client.DeleteAsync($"/api/magical-objects/{magicalObject.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();
            var deletedEntity = await dbContext.Set<MagicalObject>()
                .Where(x => x.Id == magicalObject.Id)
                .FirstOrDefaultAsync();

            Assert.Null(deletedEntity);
        }
    }
}