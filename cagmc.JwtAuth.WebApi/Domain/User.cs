namespace cagmc.JwtAuth.WebApi.Domain;

public sealed class User : EntityBase
{
    public required string Username { get; init; }
    
    /// <summary>
    /// Never store password as plain text in production!
    /// This is just a small example application
    /// </summary>
    public required string Password { get; init; }
    
    public required List<UserRole> Roles { get; init; }
    public required List<UserClaim> Claims { get; init; }
}

public sealed class UserRole
{
    public required string Name { get; init; }
}

public sealed class UserClaim
{
    public required string Type { get; init; }
    public required string Value { get; init; }
}