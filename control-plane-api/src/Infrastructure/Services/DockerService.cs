using System.Threading.Tasks;
using DevForge.ControlPlane.Application.Common.Interfaces;

namespace DevForge.ControlPlane.Infrastructure.Services;

public class DockerService : IDockerService
{
    private readonly IContainerRuntime _runtime;

    public DockerService(IContainerRuntime runtime)
    {
        _runtime = runtime;
    }

    public Task<string> BuildImageAsync(string contextPath, string imageTag)
    {
        return _runtime.BuildImageAsync(contextPath, imageTag);
    }

    public Task<string> StartContainerAsync(string imageTag, string containerName, string portMapping)
    {
        return _runtime.StartContainerAsync(imageTag, containerName, portMapping);
    }

    public Task StopContainerAsync(string containerId)
    {
        return _runtime.StopContainerAsync(containerId);
    }

    public Task<bool> HealthCheckAsync(string containerId)
    {
        return _runtime.HealthCheckAsync(containerId);
    }
}
