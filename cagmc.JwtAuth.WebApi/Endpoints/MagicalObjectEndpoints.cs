namespace Microsoft.AspNetCore.Builder;

internal static class MagicalObjectEndpoints
{
    public static WebApplication MapMagicalObjectEndpoints(this WebApplication app)
    {
        app.MapGroup("/api/magical-objects")
            .ConfigureValuesRoutes()
            .WithTags("MagicalObjects")
            .RequireAuthorization()
            .WithOpenApi();

        return app;
    }

    private static RouteGroupBuilder ConfigureValuesRoutes(this RouteGroupBuilder builder)
    {
        return builder;
    }
}