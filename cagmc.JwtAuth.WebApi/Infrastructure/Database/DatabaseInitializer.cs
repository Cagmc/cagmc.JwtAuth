using System.Globalization;

using cagmc.JwtAuth.WebApi.Common.Constants;
using cagmc.JwtAuth.WebApi.Common.Enums;
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
            },
            new()
            {
                Name = "Staff of Merlin",
                Description = "The legendary staff of the wizard Merlin",
                Elemental = ElementalType.Water,
                Discovered = DateTime.Parse("1200.03.10", new DateTimeFormatInfo()),
                Properties =
                [
                    new MagicalProperty { Name = "Magic Power", Value = "100" }
                ]
            },
            new()
            {
                Name = "Crystal of Eternity",
                Description = "An ancient crystal with eternal energy",
                Elemental = ElementalType.Earth,
                Discovered = DateTime.Parse("5000.01.01", new DateTimeFormatInfo()),
                Properties =
                [
                    new MagicalProperty { Name = "Eternal Energy", Value = "Infinity" }
                ]
            },
            new()
            {
                Name = "Phoenix Feather",
                Description = "A feather from the mythical phoenix bird",
                Elemental = ElementalType.Fire,
                Discovered = DateTime.Parse("1500.08.20", new DateTimeFormatInfo()),
                Properties =
                [
                    new MagicalProperty { Name = "Rebirth Energy", Value = "85" }
                ]
            },
            new()
            {
                Name = "Orb of Lumina",
                Description = "A glowing orb said to illuminate the darkest corners",
                Elemental = ElementalType.Light,
                Discovered = DateTime.Parse("300.12.01", new DateTimeFormatInfo()),
                Properties =
                [
                    new MagicalProperty { Name = "Light Emission", Value = "200" },
                    new MagicalProperty { Name = "Power Reserve", Value = "Unlimited" }
                ]
            },
            new()
            {
                Name = "Shadow Cloak",
                Description = "A cloak that renders the wearer invisible in shadows",
                Elemental = ElementalType.Darkness,
                Discovered = DateTime.Parse("1100.05.15", new DateTimeFormatInfo()),
                Properties =
                [
                    new MagicalProperty { Name = "Invisibility Power", Value = "100" }
                ]
            },
            new()
            {
                Name = "Elixir of Ages",
                Description = "A potion that grants extended longevity",
                Elemental = ElementalType.Water,
                Discovered = DateTime.Parse("1400.07.30", new DateTimeFormatInfo()),
                Properties =
                [
                    new MagicalProperty { Name = "Longevity Boost", Value = "1000" }
                ]
            },
            new()
            {
                Name = "Gem of Resonance",
                Description = "A mystical gem that amplifies magical spells",
                Elemental = ElementalType.Earth,
                Discovered = DateTime.Parse("700.11.22", new DateTimeFormatInfo()),
                Properties =
                [
                    new MagicalProperty { Name = "Spell Amplification", Value = "3x" }
                ]
            }
        ];

        dbContext.AddRange(magicalObjects);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}