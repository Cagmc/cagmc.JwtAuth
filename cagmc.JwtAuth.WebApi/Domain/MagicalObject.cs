using cagmc.JwtAuth.WebApi.Common.Enums;

namespace cagmc.JwtAuth.WebApi.Domain;

public sealed class MagicalObject : EntityBase
{
    public required string Name { get; set; }
    public required string? Description { get; set; }
    public required ElementalType Elemental { get; set; }
    public required DateTime Discovered { get; set; }

    public required List<MagicalProperty> Properties { get; set; } = [];
}