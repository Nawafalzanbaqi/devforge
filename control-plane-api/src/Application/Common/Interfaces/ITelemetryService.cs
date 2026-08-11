using System;
using System.Threading.Tasks;

namespace DevForge.ControlPlane.Application.Common.Interfaces;

public interface ITelemetryService
{
    Task RecordMetricAsync(Guid projectId, Guid jobId, string metricName, double value, string unit);
    Task RecordDurationAsync(Guid projectId, Guid jobId, string stage, double durationSeconds);
    Task RecordCounterAsync(Guid projectId, Guid jobId, string metricName, double value = 1);
}
