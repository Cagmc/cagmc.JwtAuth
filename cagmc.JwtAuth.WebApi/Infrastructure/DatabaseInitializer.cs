using cagmc.JwtAuth.WebApi.Common.Constants;
using cagmc.JwtAuth.WebApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace cagmc.JwtAuth.WebApi.Infrastructure;

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
                Username = "admin",
                Password = "<PASSWORD>",
                Roles = [new() { Name = Roles.Admin}],
                Claims = []
            },
            new()
            {
                Username = "reader",
                Password = "<PASSWORD>",
                Roles = [],
                Claims = [new() { Type = Claims.Read, Value = "true" }]
            },
            new()
            {
                Username = "editor",
                Password = "<PASSWORD>",
                Roles = [],
                Claims = 
                [
                    new() { Type = Claims.Read, Value = "true" }, 
                    new() { Type = Claims.Write, Value = "true" }
                ]
            }
        ];
        
        dbContext.AddRange(users);
        
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}