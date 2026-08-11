using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DevForge.ControlPlane.Application.Common.Interfaces;
using DevForge.ControlPlane.Domain.Entities;

namespace DevForge.ControlPlane.Infrastructure.Services;

public class DatabaseMetricsPublisher : IMetricsPublisher
{
    private readonly IApplicationDbContext _db;
    private readonly ILogger<DatabaseMetricsPublisher> _logger;

    public DatabaseMetricsPublisher(IApplicationDbContext db, ILogger<DatabaseMetricsPublisher> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task PublishMetricAsync(Guid projectId, Guid jobId, string metricName, double value, string unit)
    {
        try
        {
            var telemetry = new ManufacturingTelemetry
            {
                ProjectId = projectId,
                JobId = jobId,
                MetricName = metricName,
                MetricValue = value,
                Unit = unit,
                RecordedAt = DateTimeOffset.UtcNow
            };

            _db.ManufacturingTelemetries.Add(telemetry);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DatabaseMetricsPublisher failed to persist metric {MetricName} to database.", metricName);
            // Graceful degradation: do not throw to block core execution pipeline
        }
    }
}
