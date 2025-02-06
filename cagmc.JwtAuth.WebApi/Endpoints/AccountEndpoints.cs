using cagmc.JwtAuth.WebApi.Service;

namespace Microsoft.AspNetCore.Builder;

public static class AccountEndpoints
{
    public static WebApplication MapAccountEndpoints(this WebApplication app)
    {
        app.MapGroup("/api/accounts")
            .ConfigureAccountRoutes()
            .WithTags("Accounts")
            .WithOpenApi();
        
        return app;
    }

    private static RouteGroupBuilder ConfigureAccountRoutes(this RouteGroupBuilder builder)
    {
        builder.MapPost("/login", async (LoginRequest model, IAccountService accountService, CancellationToken cancellationToken) =>
            {
                var token = await accountService.LoginAsync(model, cancellationToken);
                
                return Results.Ok(new { token });
            })
            .WithName("Login");
        
        builder.MapPost("/logout", async (IAccountService accountService, CancellationToken cancellationToken) =>
            {
                await accountService.LogoutAsync(cancellationToken);
                return Results.Ok();
            })
            .RequireAuthorization()
            .WithName("Logout");
        
        builder.MapPost("/refresh", async (RefreshTokenRequest model, IAccountService accountService, CancellationToken cancellationToken) =>
            {
                var response = await accountService.RefreshTokenAsync(model, cancellationToken);
                
                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithName("RefreshToken");
    
        return builder;
    }
}