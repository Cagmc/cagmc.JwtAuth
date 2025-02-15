using System.Text;
using System.Text.Json.Serialization;

using cagmc.JwtAuth.WebApi;
using cagmc.JwtAuth.WebApi.Application;
using cagmc.JwtAuth.WebApi.Common.Constants;
using cagmc.JwtAuth.WebApi.Endpoints;
using cagmc.JwtAuth.WebApi.Infrastructure;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddInfrastructureLayer(builder.Configuration)
    .AddApplicationLayer(builder.Configuration)
    .AddWebLayer(builder.Configuration);

builder.Services.AddAuthentication()
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/api/accounts/login";
        options.LogoutPath = "/api/accounts/logout";
        options.AccessDeniedPath = "/api/accounts/access-denied";

        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy(Policies.CorsPolicy, policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors").Get<string[]>()!)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Policies.AdminPolicy, policy =>
        policy.RequireRole(Roles.Admin))
    .AddPolicy(Policies.ReadOnlyPolicy, policy =>
        policy.RequireClaim(Claims.Read, "true"))
    .AddPolicy(Policies.EditorPolicy, policy =>
    {
        policy.RequireClaim(Claims.Write, "true");
        policy.RequireClaim(Claims.Read, "true");
    })
    .AddPolicy(Policies.CookiePolicy, policy =>
    {
        policy.AuthenticationSchemes.Add(CookieAuthenticationDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
    })
    .AddPolicy(Policies.JwtPolicy, policy =>
    {
        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
    })
    .AddPolicy(Policies.MultiAuthPolicy, policy =>
    {
        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
        policy.AuthenticationSchemes.Add(CookieAuthenticationDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
    });

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddOpenApi();

var app = builder.Build();

await app.ConfigureDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCookiePolicy();
app.UseCors(Policies.CorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app
    .MapAccountEndpoints()
    .MapMagicalObjectEndpoints()
    .MapValuesEndpoints();

await app.RunAsync();

public partial class Program
{
    protected Program()
    {
    }
}