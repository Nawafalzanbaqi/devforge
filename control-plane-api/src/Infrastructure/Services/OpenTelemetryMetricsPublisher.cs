using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DevForge.ControlPlane.Application.Common.Interfaces;
using DevForge.ControlPlane.Application.Common.Metrics;

namespace DevForge.ControlPlane.Infrastructure.Services;

public class OpenTelemetryMetricsPublisher : IMetricsPublisher
{
    private readonly ILogger<OpenTelemetryMetricsPublisher> _logger;

    public OpenTelemetryMetricsPublisher(ILogger<OpenTelemetryMetricsPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishMetricAsync(Guid projectId, Guid jobId, string metricName, double value, string unit)
    {
        try
        {
            var tags = new[]
            {
                new KeyValuePair<string, object?>("ProjectId", projectId.ToString()),
                new KeyValuePair<string, object?>("JobId", jobId.ToString())
            };

            switch (metricName)
            {
                case "GenerationDuration":
                    DevForgeMetrics.GenerationDuration.Record(value, tags);
                    break;
                case "QaDuration":
                    DevForgeMetrics.QaDuration.Record(value, tags);
                    break;
                case "DeploymentDuration":
                    DevForgeMetrics.DeploymentDuration.Record(value, tags);
                    break;
                case "RepairDuration":
                    DevForgeMetrics.RepairDuration.Record(value, tags);
                    break;
                case "LeadTimeToLive":
                    DevForgeMetrics.LeadTimeToLive.Record(value, tags);
                    break;
                case "devforge_container_cpu_usage":
                    DevForgeMetrics.ContainerCpuUsage.Record(value, tags);
                    break;
                case "devforge_container_memory_usage_bytes":
                    DevForgeMetrics.ContainerMemoryUsage.Record(value, tags);
                    break;
                case "devforge_container_restart_count":
                    DevForgeMetrics.ContainerRestartCount.Add((long)value, tags);
                    break;
                case "devforge_container_uptime_seconds":
                    DevForgeMetrics.ContainerUptime.Record(value, tags);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenTelemetryMetricsPublisher failed to record metric {MetricName}.", metricName);
        }

        return Task.CompletedTask;
    }
}
