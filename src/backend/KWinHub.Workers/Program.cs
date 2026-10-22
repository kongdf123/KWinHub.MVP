using KWinHub.Infrastructure.Persistence;
using KWinHub.Workers;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<KWinHubDbContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("KWinHubDatabase")
        ?? "Host=localhost;Port=5432;Database=kwinhub;Username=postgres;Password=postgres";
    options.UseNpgsql(cs);
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
