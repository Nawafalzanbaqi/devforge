using System.Threading.Tasks;

namespace DevForge.ControlPlane.Application.Common.Interfaces;

public interface IContainerRuntime
{
    Task<string> BuildImageAsync(string contextPath, string imageTag);
    Task<string> StartContainerAsync(string imageTag, string containerName, string portMapping);
    Task StopContainerAsync(string containerId);
    Task<bool> HealthCheckAsync(string containerId);
    Task<string> RunComposeUpAsync(string composeFilePath);
    Task RunComposeDownAsync(string composeFilePath);
}
