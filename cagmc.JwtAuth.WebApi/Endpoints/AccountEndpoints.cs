using cagmc.JwtAuth.WebApi.Constants;
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
                var response = await accountService.LoginAsync(model, cancellationToken);

                if (response.Code == 401)
                {
                    return Results.Unauthorized(); 
                }
                
                return Results.Ok(response.Data);
            })
            .WithName("Login");
        
        builder.MapPost("/logout", async (IAccountService accountService, CancellationToken cancellationToken) =>
            {
                await accountService.LogoutAsync(cancellationToken);
                return Results.Ok();
            })
            .RequireAuthorization(Policies.MultitenantPolicy)
            .WithName("Logout");
        
        builder.MapPost("/refresh", async (RefreshTokenRequest model, IAccountService accountService, CancellationToken cancellationToken) =>
            {
                var response = await accountService.RefreshTokenAsync(model, cancellationToken);

                if (response.Code == 400)
                {
                    return Results.BadRequest(response.Message);
                }
                
                if (response.Code == 401)
                {
                    return Results.Unauthorized();
                }
                
                return Results.Ok(response.Data);
            })
            .WithName("RefreshToken");

        builder.MapGet("/me", async (IAccountService accountService, CancellationToken cancellationToken) =>
            {
                var meViewModel = await accountService.MeAsync(cancellationToken);

                return meViewModel;
            })
            .RequireAuthorization(Policies.MultitenantPolicy)
            .WithName("Me");
    
        return builder;
    }
}