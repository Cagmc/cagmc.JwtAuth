using System.Security.Claims;

namespace cagmc.JwtAuth.WebApi.Service;

public interface ICurrentUserService
{
    string Role { get; }
}

internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly HttpContext? _httpContext = httpContextAccessor.HttpContext;

    public string Role => _httpContext?.User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
}