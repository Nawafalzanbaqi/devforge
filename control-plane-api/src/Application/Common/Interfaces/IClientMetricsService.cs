using System;
using System.Threading.Tasks;

namespace DevForge.ControlPlane.Application.Common.Interfaces;

public interface IClientMetricsService
{
    Task RecalculateClientMetricsAsync(Guid clientId);
    Task RecalculateAllClientsMetricsAsync();
}
