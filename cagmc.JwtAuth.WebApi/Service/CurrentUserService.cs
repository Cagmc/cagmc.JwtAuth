using System.Security.Claims;

using cagmc.JwtAuth.WebApi.Application.Services;
using cagmc.JwtAuth.WebApi.Common.Constants;

namespace cagmc.JwtAuth.WebApi.Service;

internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly HttpContext? _httpContext = httpContextAccessor.HttpContext;

    public string UserName => GetClaimValue(ClaimTypes.Name);
    public string Role => GetClaimValue(ClaimTypes.Role);
    public string RefreshToken => GetClaimValue(Claims.RefreshToken);

    private string GetClaimValue(string claimType)
    {
        return _httpContext?.User.FindFirst(claimType)?.Value ?? string.Empty;
    }
}