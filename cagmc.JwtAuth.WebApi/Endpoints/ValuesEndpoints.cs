using cagmc.JwtAuth.WebApi.Common.Constants;

namespace cagmc.JwtAuth.WebApi.Endpoints;

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
            .RequireAuthorization(Policies.MultiAuthPolicy)
            .WithName("GetAuthenticated");
        
        builder.MapGet("/admin", () => "Hello Admin!")
            .RequireAuthorization(Policies.AdminPolicy, Policies.MultiAuthPolicy)
            .WithName("GetAdmin");
        
        builder.MapGet("/read", () => "Hello Reader!")
            .RequireAuthorization(Policies.ReadOnlyPolicy, Policies.MultiAuthPolicy)
            .WithName("GetRead");
        
        builder.MapGet("/edit", () => "Hello Editor!")
            .RequireAuthorization(Policies.EditorPolicy, Policies.MultiAuthPolicy)
            .WithName("GetEdit");
        
        return builder;
    }
}