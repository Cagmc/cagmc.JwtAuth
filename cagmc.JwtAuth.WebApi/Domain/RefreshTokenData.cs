namespace cagmc.JwtAuth.WebApi.Domain;

public sealed class RefreshTokenData : EntityBase
{
    public required string Username { get; init; }
    public required string RefreshToken { get; init; }
    public required DateTime Expires { get; init; }
}