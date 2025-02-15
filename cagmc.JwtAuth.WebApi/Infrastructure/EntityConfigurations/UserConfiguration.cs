using cagmc.JwtAuth.WebApi.Domain;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace cagmc.JwtAuth.WebApi.Infrastructure.EntityConfigurations;

internal sealed class UserConfiguration : EntityConfigurationBase<User>
{
    protected override void ConfigureProperties(EntityTypeBuilder<User> builder)
    {
        builder.OwnsMany(x => x.Roles);
        builder.OwnsMany(x => x.Claims);
    }
}