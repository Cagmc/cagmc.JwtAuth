using cagmc.JwtAuth.WebApi.Common.Enum;
using cagmc.JwtAuth.WebApi.Domain;
using cagmc.Response.Core;

using Microsoft.EntityFrameworkCore;

namespace cagmc.JwtAuth.WebApi.Application.Services;

public interface IMagicalObjectService
{
    Task<ListResponse<MagicalObjectItemViewModel>> GetMagicalObjectsAsync(MagicalObjectFilter filter,
        CancellationToken cancellationToken = default);

    Task<MagicalObjectViewModel?> GetMagicalObjectAsync(int id, CancellationToken cancellationToken = default);

    Task<Response.Core.Response> CreateAsync(CreateMagicalObjectRequest request,
        CancellationToken cancellationToken = default);

    Task<Response.Core.Response> UpdateAsync(int id, UpdateMagicalObjectRequest request,
        CancellationToken cancellationToken = default);

    Task<Response.Core.Response> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

internal sealed class MagicalObjectService(DbContext dbContext) : IMagicalObjectService
{
    public async Task<ListResponse<MagicalObjectItemViewModel>> GetMagicalObjectsAsync(MagicalObjectFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Set<MagicalObject>()
            .AsQueryable();

        if (!string.IsNullOrEmpty(filter.NameFilter)) query = query.Where(x => x.Name.Contains(filter.NameFilter));

        if (filter.ElementalFilterSet?.Any() == true)
            query = query.Where(x => filter.ElementalFilterSet!.Contains(x.Elemental));

        if (filter.DiscoveredFrom.HasValue) query = query.Where(x => x.Discovered >= filter.DiscoveredFrom.Value);

        if (filter.DiscoveredTo.HasValue) query = query.Where(x => x.Discovered <= filter.DiscoveredTo.Value);

        var items = await query
            .Select(x => new MagicalObjectItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Elemental = x.Elemental,
                Discovered = x.Discovered
            })
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return new ListResponse<MagicalObjectItemViewModel>(items);
    }

    public async Task<MagicalObjectViewModel?> GetMagicalObjectAsync(int id,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Set<MagicalObject>()
            .AsQueryable()
            .Where(x => x.Id == id);

        var viewModel = await query
            .Select(x => new MagicalObjectViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Elemental = x.Elemental,
                Discovered = x.Discovered,
                Properties = x.Properties.Select(y => new MagicalPropertyViewModel
                {
                    Name = y.Name,
                    Value = y.Value
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return viewModel;
    }

    public async Task<Response.Core.Response> CreateAsync(CreateMagicalObjectRequest request,
        CancellationToken cancellationToken = default)
    {
        var isNameTaken = await dbContext.Set<MagicalObject>()
            .Where(x => x.Name == request.Name)
            .AnyAsync(cancellationToken);

        if (isNameTaken) return Response.Core.Response.Conflict;

        var entity = new MagicalObject
        {
            Name = request.Name,
            Description = request.Description,
            Elemental = request.Elemental,
            Discovered = request.Discovered,

            Properties = request.Properties.Select(x => new MagicalProperty
            {
                Name = x.Name,
                Value = x.Value
            }).ToList()
        };

        dbContext.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Response.Core.Response.Success;
    }

    public async Task<Response.Core.Response> UpdateAsync(int id, UpdateMagicalObjectRequest request,
        CancellationToken cancellationToken = default)
    {
        var entityToUpdate = await dbContext.Set<MagicalObject>()
            .AsTracking()
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        if (entityToUpdate is null) return Response.Core.Response.NotFound;

        entityToUpdate.Name = request.Name;
        entityToUpdate.Description = request.Description;
        entityToUpdate.Elemental = request.Elemental;
        entityToUpdate.Discovered = request.Discovered;

        entityToUpdate.Properties.Clear();
        entityToUpdate.Properties.AddRange(request.Properties.Select(x => new MagicalProperty
        {
            Name = x.Name,
            Value = x.Value
        }));

        await dbContext.SaveChangesAsync(cancellationToken);

        return Response.Core.Response.Success;
    }

    public async Task<Response.Core.Response> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entityToDelete = await dbContext.Set<MagicalObject>()
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        if (entityToDelete is null) return Response.Core.Response.NotFound;

        dbContext.Remove(entityToDelete);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Response.Core.Response.Success;
    }
}

public sealed record MagicalObjectFilter
{
    public string? NameFilter { get; init; }
    public List<ElementalType>? ElementalFilterSet { get; init; }
    public DateTime? DiscoveredFrom { get; init; }
    public DateTime? DiscoveredTo { get; init; }
}

public sealed record MagicalObjectItemViewModel
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required ElementalType Elemental { get; init; }
    public required DateTime Discovered { get; init; }
}

public sealed record MagicalObjectViewModel
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string? Description { get; init; }
    public required ElementalType Elemental { get; init; }
    public required DateTime Discovered { get; init; }
    public required List<MagicalPropertyViewModel> Properties { get; init; }
}

public sealed record MagicalPropertyViewModel
{
    public required string Name { get; init; }
    public required string Value { get; init; }
}

public sealed record CreateMagicalObjectRequest
{
    public required string Name { get; init; }
    public required string? Description { get; init; }
    public required ElementalType Elemental { get; init; }
    public required DateTime Discovered { get; init; }

    public List<CreateMagicalPropertyRequest> Properties { get; init; } = [];
}

public sealed record CreateMagicalPropertyRequest
{
    public required string Name { get; init; }
    public required string Value { get; init; }
}

public sealed record UpdateMagicalObjectRequest
{
    public required string Name { get; init; }
    public required string? Description { get; init; }
    public required ElementalType Elemental { get; init; }
    public required DateTime Discovered { get; init; }
    public List<UpdateMagicalPropertyRequest> Properties { get; init; } = [];
}

public sealed record UpdateMagicalPropertyRequest
{
    public required string Name { get; init; }
    public required string Value { get; init; }
}