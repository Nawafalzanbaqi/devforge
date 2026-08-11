using System.Threading.Tasks;

namespace DevForge.ControlPlane.Application.Common.Interfaces;

public interface IContainerMetricsService
{
    Task MonitorContainersAsync();
}
