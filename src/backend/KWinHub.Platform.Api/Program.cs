using System.Text;
using KWinHub.BuildingBlocks.Abstractions;
using KWinHub.Platform.Api.Authorization;
using KWinHub.Platform.Api.Services;
using KWinHub.Platform.Application;
using KWinHub.Platform.Application.Abstractions;
using KWinHub.Platform.Infrastructure;
using KWinHub.Platform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "KWinHub Platform API", Version = "v1" });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
builder.Services.AddSingleton<DevTokenIssuer>();
builder.Services.AddSingleton<IJwtTokenIssuer, JwtTokenIssuer>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PlatformAdmin", policy => policy.RequireClaim("platform_admin", "true"));
});

builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PlatformConsole", policy => policy
        .WithOrigins(
            "http://localhost:5173",
            "http://127.0.0.1:5173")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

ConfigureAuthentication(builder.Services, builder.Configuration);

builder.Services.AddPlatformApplication();
builder.Services.AddPlatformInfrastructure(builder.Configuration);

builder.Services.AddHostedService<OrganizationLifecycleEventRelay>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<PlatformDbContext>("PlatformDb");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
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
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "KWinHub Platform API v1"));
}

app.UseHttpsRedirection();

app.UseCors("PlatformConsole");

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
    var audience = configuration["Jwt:PlatformAudience"] ?? JwtTokenIssuer.PlatformAudience;

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

namespace KWinHub.Platform.Api
{
    public partial class Program { }
}