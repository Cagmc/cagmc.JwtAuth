using System.Globalization;

using cagmc.JwtAuth.WebApi.Common.Constants;
using cagmc.JwtAuth.WebApi.Common.Enum;
using cagmc.JwtAuth.WebApi.Domain;

using Microsoft.EntityFrameworkCore;

namespace cagmc.JwtAuth.WebApi.Infrastructure.Database;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}

internal sealed class DatabaseInitializer(DbContext dbContext) : IDatabaseInitializer
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        List<User> users =
        [
            new()
            {
                Username = "admin@cagmc.com",
                Password = "password",
                Roles = [new UserRole { Name = Roles.Admin }],
                Claims = []
            },
            new()
            {
                Username = "reader@cagmc.com",
                Password = "password",
                Roles = [],
                Claims = [new UserClaim { Type = Claims.Read, Value = "true" }]
            },
            new()
            {
                Username = "editor@cagmc.com",
                Password = "password",
                Roles = [],
                Claims =
                [
                    new UserClaim { Type = Claims.Read, Value = "true" },
                    new UserClaim { Type = Claims.Write, Value = "true" }
                ]
            }
        ];

        dbContext.AddRange(users);

        List<MagicalObject> magicalObjects =
        [
            new()
            {
                Name = "Artifact X",
                Description = "A mysterious artifact",
                Elemental = ElementalType.Radiation,
                Discovered = DateTime.Parse("1978.06.19", new DateTimeFormatInfo()),
                Properties =
                [
                    new MagicalProperty { Name = "Radiation", Value = "50" }
                ]
            },
            new()
            {
                Name = "Sword of Attila",
                Description = "Sword of the Hun leader",
                Elemental = ElementalType.Fire,
                Discovered = DateTime.Parse("452.07.25", new DateTimeFormatInfo()),
                Properties =
                [
                    new MagicalProperty { Name = "Flame", Value = "13" }
                ]
            }
        ];

        dbContext.AddRange(magicalObjects);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}