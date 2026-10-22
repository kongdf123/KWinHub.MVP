using KWinHub.Application.Protocols;
using KWinHub.Application.Results;
using KWinHub.Application.Sessions;
using KWinHub.Application.Subjects;
using KWinHub.Infrastructure.Persistence;
using KWinHub.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KWinHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("KWinHubDatabase")
            ?? throw new InvalidOperationException("Connection string 'KWinHubDatabase' not found.");

        services.AddDbContext<KWinHubDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<IProtocolRepository, ProtocolRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IResultRepository, ResultRepository>();

        return services;
    }
}
