using System.Security.Claims;

using cagmc.JwtAuth.WebApi.Common.Constants;
using cagmc.JwtAuth.WebApi.Common.Enum;
using cagmc.JwtAuth.WebApi.Domain;
using cagmc.Response.Core;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace cagmc.JwtAuth.WebApi.Application.Services;

public interface IAccountService
{
    Task<Response<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<Response<RefreshTokenResponse>> RefreshTokenAsync(RefreshTokenRequest request,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(CancellationToken cancellationToken = default);
    Task<MeViewModel> MeAsync(CancellationToken cancellationToken = default);
}

internal sealed class AccountService(
    DbContext dbContext,
    IHttpContextAccessor httpContextAccessor,
    ICurrentUserService currentUserService,
    IJwtService jwtService) : IAccountService
{
    public async Task<Response<LoginResponse>> LoginAsync(LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Set<User>()
            .AsNoTracking()
            .Where(x => x.Username == request.Username)
            .Where(x => x.Password == request.Password)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null) return Response<LoginResponse>.Unauthorized;

        var claims = GetClaimsForUser(user);

        if (request.AuthenticationMode == AuthenticationMode.Cookie)
        {
            await SignInWithCookieAsync(claims, request.IsPersistent);

            return Response<LoginResponse>.Success;
        }

        var tokenExpires = DateTime.Now.AddMinutes(30);
        var token = jwtService.GenerateToken(tokenExpires, claims);

        var refreshTokenExpires = DateTime.Now.AddDays(7);
        var refreshToken = jwtService.GenerateRefreshToken();

        var refreshTokenData = new RefreshTokenData
        {
            Username = request.Username, RefreshToken = refreshToken, Expires = refreshTokenExpires
        };

        await ManageRefreshTokenDataAsync(request, refreshTokenData, cancellationToken);

        if (request.AuthenticationMode == AuthenticationMode.JwtWithCookie)
        {
            claims.Add(new Claim(Claims.RefreshToken, refreshToken));
            await SignInWithCookieAsync(claims, request.IsPersistent);
        }

        var response = new LoginResponse
        {
            Token = token,
            Expires = tokenExpires,

            RefreshToken = request.AuthenticationMode == AuthenticationMode.Jwt ? refreshToken : null,
            RefreshExpires = request.AuthenticationMode == AuthenticationMode.Jwt ? refreshTokenExpires : null
        };

        return new Response<LoginResponse> { Data = response, IsSuccess = true };
    }

    public async Task<Response<RefreshTokenResponse>> RefreshTokenAsync(RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var existingToken = await dbContext.Set<RefreshTokenData>()
            .AsTracking()
            .FirstOrDefaultAsync(t => t.RefreshToken == request.RefreshToken, cancellationToken);

        if (existingToken is null)
            return new Response<RefreshTokenResponse> { Code = 400, IsSuccess = false, Message = "Invalid token." };

        if (existingToken.Expires < DateTime.Now)
        {
            dbContext.Remove(existingToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new Response<RefreshTokenResponse> { Code = 400, IsSuccess = false, Message = "Token expired." };
        }

        var user = await dbContext.Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == existingToken.Username, cancellationToken);

        if (user is null) return Response<RefreshTokenResponse>.Unauthorized;

        var claims = GetClaimsForUser(user);
        var tokenExpires = DateTime.Now.AddMinutes(30);
        var newToken = jwtService.GenerateToken(tokenExpires, claims);

        var response = new RefreshTokenResponse
        {
            Token = newToken,
            Expires = tokenExpires
        };

        return new Response<RefreshTokenResponse> { Data = response, IsSuccess = true };
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        await httpContextAccessor.HttpContext!
            .SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public Task<MeViewModel> MeAsync(CancellationToken cancellationToken = default)
    {
        var userName = currentUserService.UserName;
        var role = currentUserService.Role;

        return Task.FromResult(new MeViewModel
        {
            Username = userName,
            Role = role
        });
    }

    private static List<Claim> GetClaimsForUser(User user)
    {
        List<Claim> claims =
        [
            new(ClaimTypes.Name, user.Username)
        ];
        user.Roles.ForEach(role => claims.Add(new Claim(ClaimTypes.Role, role.Name)));
        user.Claims.ForEach(claim => claims.Add(new Claim(claim.Type, claim.Value)));

        return claims;
    }

    private async Task SignInWithCookieAsync(List<Claim> claims, bool isPersistent)
    {
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        await httpContextAccessor.HttpContext!.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            claimsPrincipal, new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                ExpiresUtc = DateTime.UtcNow.AddDays(7)
            });
    }

    private async Task ManageRefreshTokenDataAsync(LoginRequest request,
        RefreshTokenData refreshTokenData, CancellationToken cancellationToken)
    {
        var existingRefreshToken = await dbContext.Set<RefreshTokenData>()
            .AsTracking()
            .Where(t => t.Username == request.Username)
            .SingleOrDefaultAsync(cancellationToken);

        if (existingRefreshToken is not null) dbContext.Remove(existingRefreshToken);

        dbContext.Add(refreshTokenData);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

public sealed record LoginRequest
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public bool IsPersistent { get; init; } = true;
    public AuthenticationMode AuthenticationMode { get; init; } = AuthenticationMode.Jwt;
}

public sealed record LoginResponse
{
    public required string Token { get; init; }
    public required DateTime Expires { get; init; }

    public required string? RefreshToken { get; init; }
    public required DateTime? RefreshExpires { get; init; }
}

public sealed record RefreshTokenRequest
{
    public required string RefreshToken { get; init; }
}

public sealed record RefreshTokenResponse
{
    public required string Token { get; init; }
    public required DateTime Expires { get; init; }
}

public sealed record MeViewModel
{
    public required string Username { get; init; }
    public required string Role { get; init; }
}