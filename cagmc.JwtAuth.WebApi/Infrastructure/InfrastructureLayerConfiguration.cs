using cagmc.JwtAuth.WebApi.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;

namespace cagmc.JwtAuth.WebApi.Infrastructure;

public static class InfrastructureLayerConfiguration
{
    public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services,
        IConfiguration configuration)
    {
        return services
            .AddDatabase(configuration);
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();

        services.AddDbContext<DbContext, ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(configuration.GetConnectionString("DefaultConnection")!));

        return services;
    }
}