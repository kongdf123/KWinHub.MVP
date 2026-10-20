using KWinHub.Application.Subjects;
using Microsoft.Extensions.DependencyInjection;

namespace KWinHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ISubjectService, SubjectService>();

        return services;
    }
}