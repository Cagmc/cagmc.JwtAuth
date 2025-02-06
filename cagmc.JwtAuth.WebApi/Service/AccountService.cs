using System.Security.Claims;
using cagmc.JwtAuth.WebApi.Constants;
using cagmc.JwtAuth.WebApi.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace cagmc.JwtAuth.WebApi.Service;

public interface IAccountService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync(CancellationToken cancellationToken = default);
}

internal sealed class AccountService(
    DbContext dbContext,
    IJwtService jwtService) : IAccountService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var tokenExpires = DateTime.Now.AddMinutes(30);
        var refreshTokenExpires = DateTime.Now.AddDays(7);
        
        List<Claim> claims = [];

        if (request.Username == "admin")
        {
            claims.Add(new Claim(ClaimTypes.Role, Roles.Admin));
        }

        if (request.Username == "reader")
        {
            claims.Add(new Claim(Claims.Read, "true"));
        }

        if (request.Username == "editor")
        {
            claims.Add(new Claim(Claims.Write, "true"));
            claims.Add(new Claim(Claims.Read, "true"));
        }
        
        var token = jwtService.GenerateToken(request.Username, tokenExpires, claims);
        var refreshToken = jwtService.GenerateRefreshToken();

        var refreshTokenData = new RefreshTokenData
        {
            Username = request.Username, RefreshToken = refreshToken, Expires = refreshTokenExpires
        };

        var response = new LoginResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            Expires = tokenExpires,
            RefreshExpires = refreshTokenExpires
        };
        
        await ManageRefreshTokenDataAsync(request, refreshTokenData, cancellationToken);

        return response;
    }

    private async Task ManageRefreshTokenDataAsync(LoginRequest request,
        RefreshTokenData refreshTokenData, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        
        await dbContext.Set<RefreshTokenData>()
            .Where(t => t.Username == request.Username)
            .ExecuteDeleteAsync(cancellationToken);

        dbContext.Add(refreshTokenData);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var existingToken = await dbContext.Set<RefreshTokenData>()
            .AsTracking()
            .FirstOrDefaultAsync(t => t.RefreshToken == request.RefreshToken, cancellationToken);
        
        if (existingToken is null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        if (existingToken.Expires < DateTime.Now)
        {
            dbContext.Remove(existingToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            
            throw new UnauthorizedAccessException("Refresh token expired.");
        }
    
        var tokenExpires = DateTime.Now.AddMinutes(30);
        
        List<Claim> claims = [];

        if (existingToken.Username == "admin")
        {
            claims.Add(new Claim(ClaimTypes.Role, "admin"));
        }
    
        var newToken = jwtService.GenerateToken(existingToken.Username, tokenExpires, claims);
    
        var response = new RefreshTokenResponse
        {
            Token = newToken,
            Expires = tokenExpires
        };
    
        return response;
    }

    public Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

public sealed record LoginRequest
{
    public required string Username { get; init; }
    public required string Password { get; init; }
}

public sealed record LoginResponse
{
    public required string Token { get; init; }
    public required string RefreshToken { get; init; }
    public required DateTime Expires { get; init; }
    public required DateTime RefreshExpires { get; init; }
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