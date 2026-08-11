using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DevForge.ControlPlane.Application.Common.Interfaces;

namespace DevForge.ControlPlane.MonitoringWorker;

public class ContainerMetricsWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ContainerMetricsWorker> _logger;

    public ContainerMetricsWorker(IServiceProvider serviceProvider, ILogger<ContainerMetricsWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Container Metrics Background Worker running...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var monitor = scope.ServiceProvider.GetRequiredService<IContainerMetricsService>();
                await monitor.MonitorContainersAsync();

                var alerting = scope.ServiceProvider.GetRequiredService<IAlertingService>();
                await alerting.EvaluateMetricsAlertsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during periodic container metrics collection loop.");
            }

            // Wait 15 seconds before the next poll
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
    }
}
