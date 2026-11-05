using System.Text;
using KWinHub.BuildingBlocks.Abstractions;
using KWinHub.Hub.Api.Authorization;
using KWinHub.Hub.Api.Controllers;
using KWinHub.Hub.Api.Services;
using KWinHub.Hub.Application;
using KWinHub.Hub.Infrastructure;
using KWinHub.Hub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "KWinHub Hub API", Version = "v1" });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
builder.Services.AddSingleton<DevTokenIssuer>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(HubPolicyNames.OrgMember, policy => policy.RequireAuthenticatedUser());
    options.AddPolicy(HubPolicyNames.OrgAdmin, policy => policy
        .RequireAuthenticatedUser()
        .RequireClaim("org_role", "Owner", "Admin"));
    options.AddPolicy(HubPolicyNames.CanRead, policy =>
        policy.Requirements.Add(new PermissionRequirement(KWinHub.Hub.Organizations.OrganizationPermissions.Read)));
    options.AddPolicy(HubPolicyNames.CanWrite, policy =>
        policy.Requirements.Add(new PermissionRequirement(KWinHub.Hub.Organizations.OrganizationPermissions.Write)));
    options.AddPolicy(HubPolicyNames.CanExport, policy =>
        policy.Requirements.Add(new PermissionRequirement(KWinHub.Hub.Organizations.OrganizationPermissions.Export)));
    options.AddPolicy(HubPolicyNames.CanAccessRawAsset, policy =>
        policy.Requirements.Add(new PermissionRequirement(KWinHub.Hub.Organizations.OrganizationPermissions.RawAsset)));
    options.AddPolicy(HubPolicyNames.CanReport, policy =>
        policy.Requirements.Add(new PermissionRequirement(KWinHub.Hub.Organizations.OrganizationPermissions.Report)));
    options.AddPolicy(HubPolicyNames.CanPlayback, policy =>
        policy.Requirements.Add(new PermissionRequirement(KWinHub.Hub.Organizations.OrganizationPermissions.Playback)));
});

builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddProblemDetails();

builder.Services.Configure<MediaCallbackOptions>(builder.Configuration.GetSection("MediaCallbacks"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebReact", policy => policy
        .WithOrigins(
            "http://localhost:5174",
            "http://127.0.0.1:5174")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

ConfigureAuthentication(builder.Services, builder.Configuration);

builder.Services.AddHubApplication();
builder.Services.AddHubInfrastructure(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<HubDbContext>("HubDb");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
    if (db.Database.IsRelational())
    {
        await db.Database.MigrateAsync();
    }
    else
    {
        await db.Database.EnsureCreatedAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "KWinHub Hub API v1"));
}

app.UseHttpsRedirection();

app.UseCors("WebReact");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
{
    var key = configuration["Jwt:SigningKey"];
    if (string.IsNullOrWhiteSpace(key))
    {
        throw new InvalidOperationException("Jwt:SigningKey is not configured.");
    }

    if (!configuration.GetValue("Jwt:AllowInsecureSigningKey", false) && key.Length < 32)
    {
        throw new InvalidOperationException(
            "Jwt:SigningKey must be at least 32 characters long for production security. " +
            "Set Jwt:AllowInsecureSigningKey=true only in non-production environments.");
    }

    var issuer = configuration["Jwt:Issuer"] ?? "kwinhub";
    var audience = configuration["Jwt:HubAudience"] ?? "kwinhub-hub-api";

    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ClockSkew = TimeSpan.Zero,
            };
            options.Events = SecurityStampValidation.Events;
        });
}

namespace KWinHub.Hub.Api
{
    public partial class Program { }
}