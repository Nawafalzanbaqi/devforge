using System;
using System.Threading.Tasks;

namespace DevForge.ControlPlane.Application.Common.Interfaces;

public interface IAlertingService
{
    Task TriggerAlertAsync(string alertName, string message, string severity, Guid? projectId = null);
    Task ResolveAlertAsync(string alertName, Guid? projectId = null);
    Task EvaluateMetricsAlertsAsync();
}
