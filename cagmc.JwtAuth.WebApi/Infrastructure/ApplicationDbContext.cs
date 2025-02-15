using cagmc.JwtAuth.WebApi.Domain;

using Microsoft.EntityFrameworkCore;

namespace cagmc.JwtAuth.WebApi.Infrastructure;

internal sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<MagicalObject> MagicalObjectSet { get; set; }
    public DbSet<RefreshTokenData> RefreshTokenSet { get; set; }
    public DbSet<User> UserSet { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}