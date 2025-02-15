namespace cagmc.JwtAuth.WebApi.Application.Services;

public interface ICurrentUserService
{
    string UserName { get; }
    string Role { get; }
    string RefreshToken { get; }
}