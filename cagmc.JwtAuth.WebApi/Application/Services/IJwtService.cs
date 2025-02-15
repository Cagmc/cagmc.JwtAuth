using System.Security.Claims;

namespace cagmc.JwtAuth.WebApi.Application.Services;

public interface IJwtService
{
    public string GenerateToken(DateTime expires, List<Claim>? additionalClaims);
    public string GenerateRefreshToken();
    public bool ValidateToken(string token);
}