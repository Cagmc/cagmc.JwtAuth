using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using cagmc.JwtAuth.WebApi.Application.Services;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace cagmc.JwtAuth.WebApi.Services;

internal sealed class JwtService(IOptions<JwtOptions> options) : IJwtService
{
    public string GenerateToken(DateTime expires, List<Claim>? additionalClaims)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (additionalClaims is not null) claims = claims.Concat(additionalClaims).ToArray();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            options.Value.Issuer,
            options.Value.Audience,
            claims,
            expires: expires,
            signingCredentials: credentials);

        var result = new JwtSecurityTokenHandler().WriteToken(token);

        return result;
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[options.Value.RefreshTokenSize];

        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return Convert.ToBase64String(randomBytes);
    }

    public bool ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(options.Value.Secret);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = options.Value.Issuer,
                ValidAudience = options.Value.Audience,
                ValidateLifetime = true
            }, out _);

            return true;
        }
        catch
        {
            return false;
        }
    }
}

public sealed record JwtOptions
{
    public required string Secret { get; init; }
    public required int RefreshTokenSize { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
}