using cagmc.JwtAuth.WebApi.Application.Services;
using cagmc.JwtAuth.WebApi.Common.Constants;
using cagmc.JwtAuth.WebApi.Common.Enums;

namespace cagmc.JwtAuth.WebApi.Endpoints;

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
        builder.MapPost("/login",
                async (LoginRequest model, IAccountService accountService, CancellationToken cancellationToken) =>
                {
                    var response = await accountService.LoginAsync(model, cancellationToken);

                    if (response.Code == 401) return Results.Unauthorized();

                    if (model.AuthenticationMode == AuthenticationMode.Jwt) return Results.Ok(response.Data);

                    return Results.Ok();
                })
            .WithName("Login");

        builder.MapPost("/logout", async (IAccountService accountService, CancellationToken cancellationToken) =>
            {
                await accountService.LogoutAsync(cancellationToken);
                return Results.Ok();
            })
            .RequireAuthorization(Policies.MultiAuthPolicy)
            .WithName("Logout");

        builder.MapPost("/refresh-token", async (
                RefreshTokenRequest model,
                IAccountService accountService,
                CancellationToken cancellationToken) =>
            {
                var response = await accountService.RefreshTokenAsync(model, cancellationToken);

                if (response.Code == 400) return Results.BadRequest(response.Message);

                if (response.Code == 401) return Results.Unauthorized();

                return Results.Ok(response.Data);
            })
            .WithName("RefreshToken");

        builder.MapPost("/refresh-token-cookie", async (
                ICurrentUserService currentUserService,
                IAccountService accountService,
                CancellationToken cancellationToken) =>
            {
                var refreshToken = currentUserService.RefreshToken;

                if (string.IsNullOrEmpty(refreshToken)) return Results.Unauthorized();

                var model = new RefreshTokenRequest
                {
                    RefreshToken = refreshToken
                };

                var response = await accountService.RefreshTokenAsync(model, cancellationToken);

                if (response.Code == 400) return Results.BadRequest(response.Message);

                if (response.Code == 401) return Results.Unauthorized();

                return Results.Ok(response.Data);
            })
            .RequireAuthorization(Policies.CookiePolicy)
            .WithName("RefreshTokenCookie");

        builder.MapGet("/me", async (IAccountService accountService, CancellationToken cancellationToken) =>
            {
                var meViewModel = await accountService.MeAsync(cancellationToken);

                return meViewModel;
            })
            .RequireAuthorization(Policies.MultiAuthPolicy)
            .WithName("Me");

        return builder;
    }
}