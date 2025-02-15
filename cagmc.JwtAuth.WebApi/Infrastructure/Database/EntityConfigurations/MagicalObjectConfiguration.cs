using cagmc.JwtAuth.WebApi.Domain;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace cagmc.JwtAuth.WebApi.Infrastructure.Database.EntityConfigurations;

internal sealed class MagicalObjectConfiguration : EntityConfigurationBase<MagicalObject>
{
    protected override void ConfigureProperties(EntityTypeBuilder<MagicalObject> builder)
    {
        builder.Property(x => x.Name).IsRequired();

        builder.OwnsMany(x => x.Properties);
    }
}