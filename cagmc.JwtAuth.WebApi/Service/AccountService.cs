using System.Security.Claims;
using cagmc.JwtAuth.WebApi.Domain;
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
        var user = await dbContext.Set<User>()
            .AsNoTracking()
            .Where(x => x.Username == request.Username )
            .Where(x => x.Password == request.Password)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }
        
        var tokenExpires = DateTime.Now.AddMinutes(30);
        var token = GetRefreshTokenForUser(user, tokenExpires);
        
        var refreshTokenExpires = DateTime.Now.AddDays(7);
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
        var existingRefreshToken = await dbContext.Set<RefreshTokenData>()
            .AsTracking()
            .Where(t => t.Username == request.Username)
            .SingleOrDefaultAsync(cancellationToken);

        if (existingRefreshToken is not null)
        {
            dbContext.Remove(existingRefreshToken);
        }

        dbContext.Add(refreshTokenData);
        await dbContext.SaveChangesAsync(cancellationToken);
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
        
        var user = await dbContext.Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == existingToken.Username, cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid user.");
        }
    
        var tokenExpires = DateTime.Now.AddMinutes(30);
        var newToken = GetRefreshTokenForUser(user, tokenExpires);
    
        var response = new RefreshTokenResponse
        {
            Token = newToken,
            Expires = tokenExpires
        };
    
        return response;
    }

    private string GetRefreshTokenForUser(User user, DateTime tokenExpires)
    {
        List<Claim> claims = [];
        user.Claims.ForEach(claim => claims.Add(new Claim(claim.Type, claim.Value)));
        user.Roles.ForEach(role => claims.Add(new Claim(ClaimTypes.Role, role.Name)));
    
        var newToken = jwtService.GenerateToken(user.Username, tokenExpires, claims);
        
        return newToken;
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