using cagmc.JwtAuth.WebApi.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace cagmc.JwtAuth.WebApi.Infrastructure.EntityConfigurations;

internal abstract class EntityConfigurationBase<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : EntityBase
{
    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(x => x.Id);

        ConfigureProperties(builder);
    }

    protected virtual void ConfigureProperties(EntityTypeBuilder<TEntity> builder)
    {
    }
}