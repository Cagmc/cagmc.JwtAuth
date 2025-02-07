using System.Security.Claims;

namespace cagmc.JwtAuth.WebApi.Service;

public interface ICurrentUserService
{
    string UserName { get; }
    string Role { get; }
}

internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly HttpContext? _httpContext = httpContextAccessor.HttpContext;

    public string UserName => GetClaimValue(ClaimTypes.Name);
    public string Role => GetClaimValue(ClaimTypes.Role);
    
    private string GetClaimValue(string claimType) => _httpContext?.User.FindFirst(claimType)?.Value ?? string.Empty;
}