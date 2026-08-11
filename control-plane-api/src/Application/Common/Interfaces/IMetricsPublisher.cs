using System;
using System.Threading.Tasks;

namespace DevForge.ControlPlane.Application.Common.Interfaces;

public interface IMetricsPublisher
{
    Task PublishMetricAsync(Guid projectId, Guid jobId, string metricName, double value, string unit);
}
