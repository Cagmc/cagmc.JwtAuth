using cagmc.JwtAuth.WebApi.Application.Services;
using cagmc.JwtAuth.WebApi.Infrastructure.Database;
using cagmc.JwtAuth.WebApi.Services;

namespace cagmc.JwtAuth.WebApi;

internal static class WebLayerConfiguration
{
    public static IServiceCollection AddWebLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IJwtService, JwtService>();

        return services;
    }

    public static async Task ConfigureDatabaseAsync(this WebApplication app,
        CancellationToken cancellationToken = default)
    {
        await using var scope = app.Services.CreateAsyncScope();

        var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
        await initializer.InitializeAsync(cancellationToken);
    }
}