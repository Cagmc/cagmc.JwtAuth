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
        builder.MapGet("/anonimous", () => "Hello World!")
            .WithName("GetAnonimous");
        
        builder.MapGet("/authenticated", () => "Hello World!")
            .RequireAuthorization()
            .WithName("GetAuthenticated");
        
        builder.MapGet("/admin", () => "Hello World!")
            .RequireAuthorization(Policies.AdminPolicy)
            .WithName("GetAdmin");
        
        return builder;
    }
}