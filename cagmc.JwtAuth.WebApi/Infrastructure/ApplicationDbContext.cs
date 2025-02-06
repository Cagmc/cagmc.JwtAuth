using cagmc.JwtAuth.WebApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace cagmc.JwtAuth.WebApi.Infrastructure;

internal sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<RefreshTokenData> RefreshTokenSet { get; set; }
    public DbSet<User> UserSet { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(builder =>
        {
            builder.OwnsMany(x => x.Roles);
            builder.OwnsMany(x => x.Claims);
        });
    }
}
