﻿using cagmc.JwtAuth.WebApi.Application.Services;

namespace cagmc.JwtAuth.WebApi.Application;

public static class ApplicationLayerConfiguration
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AccountOptions>(configuration.GetSection("Account"));

        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IMagicalObjectService, MagicalObjectService>();

        return services;
    }
}