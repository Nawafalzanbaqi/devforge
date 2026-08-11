using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DevForge.ControlPlane.Application.Common.Interfaces;
using DevForge.ControlPlane.Infrastructure.Persistence;
using DevForge.ControlPlane.Infrastructure.Services;
using StackExchange.Redis;

namespace DevForge.ControlPlane.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. PostgreSQL DB Context
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString,
                builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // 2. Redis distributed caching
        var redisConfig = configuration["Redis:Configuration"] ?? "localhost:6379";
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConfig;
        });

        // 3. Redis Connection Multiplexer for PubSub
        try
        {
            var options = ConfigurationOptions.Parse(redisConfig);
            options.AbortOnConnectFail = false; // Prevents crash if Redis is starting up
            var multiplexer = ConnectionMultiplexer.Connect(options);
            services.AddSingleton<IConnectionMultiplexer>(multiplexer);
        }
        catch
        {
            // Fail gracefully during local migration runs if Redis is not running
        }

        // 4. Cache & AI Client Services
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IJobStateManager, JobStateManager>();
        services.AddScoped<IWorkspaceManager, WorkspaceManager>();
        services.AddScoped<ICodeInjectionEngine, CodeInjectionEngine>();
        services.AddScoped<IGitService, GitService>();
        services.AddScoped<ISonarQubeService, SonarQubeService>();
        services.AddScoped<IPlaywrightService, PlaywrightService>();
        services.AddScoped<IQualityScoreEngine, QualityScoreEngine>();
        services.AddScoped<IContainerRuntime, DockerRuntime>();
        services.AddScoped<IDockerService, DockerService>();
        services.AddScoped<IDomainService, DomainService>();
        services.AddScoped<IDeploymentService, DeploymentService>();
        services.AddScoped<IRepairContextBuilder, RepairContextBuilder>();
        services.AddScoped<IRepairAgent, RepairAgent>();
        services.AddScoped<IPatchEngine, PatchEngine>();
        services.AddScoped<ITelemetryService, TelemetryService>();
        services.AddScoped<IContainerMetricsService, ContainerMetricsService>();
        services.AddScoped<IAlertingService, AlertingService>();
        services.AddScoped<ITemplateScoringService, TemplateScoringService>();
        services.AddScoped<IClientMetricsService, ClientMetricsService>();
        services.AddScoped<IBillingService, BillingService>();
        services.AddScoped<IQuotaService, QuotaService>();
        services.AddScoped<IMetricsPublisher, DatabaseMetricsPublisher>();
        services.AddScoped<IMetricsPublisher, OpenTelemetryMetricsPublisher>();

        services.AddHttpClient<IAiService, AiService>(client =>
        {
            client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
            client.Timeout = TimeSpan.FromMinutes(5); // Large code blocks require higher context timeouts
        });

        return services;
    }
}
