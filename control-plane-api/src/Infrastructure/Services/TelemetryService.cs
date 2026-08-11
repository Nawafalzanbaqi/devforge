using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DevForge.ControlPlane.Application.Common.Interfaces;

namespace DevForge.ControlPlane.Infrastructure.Services;

public class TelemetryService : ITelemetryService
{
    private readonly IEnumerable<IMetricsPublisher> _publishers;
    private readonly ILogger<TelemetryService> _logger;

    public TelemetryService(IEnumerable<IMetricsPublisher> publishers, ILogger<TelemetryService> logger)
    {
        _publishers = publishers;
        _logger = logger;
    }

    public async Task RecordMetricAsync(Guid projectId, Guid jobId, string metricName, double value, string unit)
    {
        _logger.LogInformation("Recording metric via publisher pipeline: {MetricName} = {Value} {Unit} for Project {ProjectId}, Job {JobId}", 
            metricName, value, unit, projectId, jobId);

        foreach (var publisher in _publishers)
        {
            try
            {
                await publisher.PublishMetricAsync(projectId, jobId, metricName, value, unit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing metric to publisher {PublisherName}", publisher.GetType().Name);
            }
        }
    }

    public Task RecordDurationAsync(Guid projectId, Guid jobId, string stage, double durationSeconds)
    {
        string metricName = stage switch
        {
            "Generation" => "GenerationDuration",
            "QA" => "QaDuration",
            "Deployment" => "DeploymentDuration",
            "Repair" => "RepairDuration",
            "LeadTimeToLive" => "LeadTimeToLive",
            _ => $"{stage}Duration"
        };

        return RecordMetricAsync(projectId, jobId, metricName, durationSeconds, "Seconds");
    }

    public Task RecordCounterAsync(Guid projectId, Guid jobId, string metricName, double value = 1)
    {
        return RecordMetricAsync(projectId, jobId, metricName, value, "Count");
    }
}
