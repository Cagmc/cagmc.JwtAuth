﻿using cagmc.JwtAuth.WebApi.Application.Services;
using cagmc.JwtAuth.WebApi.Common.Constants;
using cagmc.JwtAuth.WebApi.Common.Enum;

using Microsoft.AspNetCore.Mvc;

namespace cagmc.JwtAuth.WebApi.Endpoints;

internal static class MagicalObjectEndpoints
{
    public static WebApplication MapMagicalObjectEndpoints(this WebApplication app)
    {
        app.MapGroup("/api/magical-objects")
            .ConfigureValuesRoutes()
            .WithTags("MagicalObjects")
            .RequireAuthorization(Policies.MultiAuthPolicy)
            .WithOpenApi();

        return app;
    }

    private static RouteGroupBuilder ConfigureValuesRoutes(this RouteGroupBuilder builder)
    {
        builder.MapGet("",
                async ([FromQuery] string? nameFilter, [FromQuery] DateTime? discoveredFrom,
                    [FromQuery] DateTime? discoveredTo, [FromQuery] ElementalType[]? elementalFilterSet,
                    IMagicalObjectService service,
                    CancellationToken cancellationToken) =>
                {
                    var filter = new MagicalObjectFilter
                    {
                        NameFilter = nameFilter,
                        DiscoveredTo = discoveredTo,
                        DiscoveredFrom = discoveredFrom,
                        ElementalFilterSet = elementalFilterSet?.ToList()
                    };

                    var response = await service.GetMagicalObjectsAsync(filter, cancellationToken);

                    return response;
                })
            .WithName("GetMagicalObjects");

        return builder;
    }
}