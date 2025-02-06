using Microsoft.EntityFrameworkCore;

namespace cagmc.JwtAuth.WebApi.Infrastructure;

internal sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<RefreshTokenData> RefreshTokenSet { get; set; } = null!;
}

public sealed record RefreshTokenData
{
    public int Id { get; init; }
    
    public required string Username { get; init; }
    public required string RefreshToken { get; init; }
    public required DateTime Expires { get; init; }
}