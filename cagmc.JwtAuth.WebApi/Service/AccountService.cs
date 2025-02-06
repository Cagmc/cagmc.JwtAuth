namespace cagmc.JwtAuth.WebApi.Service;

public interface IAccountService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync(CancellationToken cancellationToken = default);
}

internal sealed class AccountService(IJwtService jwtService) : IAccountService
{
    private static readonly List<RefreshTokenData> RefreshTokens = [];
    
    public Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var tokenExpires = DateTime.Now.AddMinutes(30);
        var refreshTokenExpires = DateTime.Now.AddDays(7);
        
        var token = jwtService.GenerateToken(request.Username, tokenExpires);
        var refreshToken = jwtService.GenerateRefreshToken();

        var response = new LoginResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            Expires = tokenExpires,
            RefreshExpires = refreshTokenExpires
        };
        
        RefreshTokens.RemoveAll(t => t.Username == request.Username);
        RefreshTokens.Add(new RefreshTokenData
        {
            Username = request.Username, RefreshToken = refreshToken, Expires = refreshTokenExpires
        });
        
        return Task.FromResult(response);
    }

    public Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var existingToken = RefreshTokens.FirstOrDefault(t => t.RefreshToken == request.RefreshToken);
        
        if (existingToken is null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        if (existingToken.Expires < DateTime.Now)
        {
            RefreshTokens.Remove(existingToken);
            
            throw new UnauthorizedAccessException("Refresh token expired.");
        }
    
        var tokenExpires = DateTime.Now.AddMinutes(30);
    
        var newToken = jwtService.GenerateToken(existingToken.Username, tokenExpires);
    
        var response = new RefreshTokenResponse
        {
            Token = newToken,
            Expires = tokenExpires
        };
    
        return Task.FromResult(response);
    }

    public Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

public sealed record RefreshTokenData
{
    public required string Username { get; init; }
    public required string RefreshToken { get; init; }
    public required DateTime Expires { get; init; }
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