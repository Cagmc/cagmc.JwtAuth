using System.Text;
using cagmc.JwtAuth.WebApi.Constants;
using cagmc.JwtAuth.WebApi.Infrastructure;
using cagmc.JwtAuth.WebApi.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
builder.Services.AddDbContext<DbContext, ApplicationDbContext>(options => options.UseInMemoryDatabase("JwtAuthDb"));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAccountService, AccountService>();

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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.AdminPolicy, policy =>
        policy.RequireRole(Roles.Admin));
    
    options.AddPolicy(Policies.ReadOnlyPolicy, policy =>
        policy.RequireClaim(Claims.Read, "true"));
    
    options.AddPolicy(Policies.EditorPolicy, policy =>
    {
        policy.RequireClaim(Claims.Write, "true");
        policy.RequireClaim(Claims.Read, "true");
    });
    
    options.AddPolicy(Policies.MultitenantPolicy, policy =>
    {
        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
        policy.AuthenticationSchemes.Add(CookieAuthenticationDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
    });
});

builder.Services.AddOpenApi();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
    await initializer.InitializeAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCookiePolicy();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app
    .MapAccountEndpoints()
    .MapValuesEndpoints();

await app.RunAsync();

public partial class Program { protected Program() { } }