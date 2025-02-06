using cagmc.JwtAuth.WebApi.Constants;

namespace Microsoft.AspNetCore.Builder;

public static class ValuesEndpoints
{
    public static WebApplication MapValuesEndpoints(this WebApplication app)
    {
        app.MapGroup("/api/values")
            .ConfigureValuesRoutes()
            .WithTags("Values")
            .WithOpenApi();
        
        return app;
    }
    
    private static RouteGroupBuilder ConfigureValuesRoutes(this RouteGroupBuilder builder)
    {
        builder.MapGet("/anonymous", () => "Hello Anonymous!")
            .WithName("GetAnonymous");
        
        builder.MapGet("/authenticated", () => "Hello Authenticated!")
            .RequireAuthorization()
            .WithName("GetAuthenticated");
        
        builder.MapGet("/admin", () => "Hello Admin!")
            .RequireAuthorization(Policies.AdminPolicy)
            .WithName("GetAdmin");
        
        builder.MapGet("/read", () => "Hello Reader!")
            .RequireAuthorization(Policies.ReadOnlyPolicy)
            .WithName("GetRead");
        
        builder.MapGet("/edit", () => "Hello Editor!")
            .RequireAuthorization(Policies.EditorPolicy)
            .WithName("GetEdit");
        
        return builder;
    }
}