using System.Threading.Tasks;

namespace DevForge.ControlPlane.Application.Common.Interfaces;

public interface IDockerService
{
    Task<string> BuildImageAsync(string contextPath, string imageTag);
    Task<string> StartContainerAsync(string imageTag, string containerName, string portMapping);
    Task StopContainerAsync(string containerId);
    Task<bool> HealthCheckAsync(string containerId);
}
