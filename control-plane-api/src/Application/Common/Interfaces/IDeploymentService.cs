using System;
using System.Threading.Tasks;

namespace DevForge.ControlPlane.Application.Common.Interfaces;

public class DeploymentResult
{
    public bool Success { get; set; }
    public string? ContainerId { get; set; }
    public string? AllocatedDomain { get; set; }
    public string ImageTag { get; set; } = string.Empty;
    public string HealthStatus { get; set; } = "Offline";
    public int HealthScore { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface IDeploymentService
{
    Task<DeploymentResult> DeployAsync(Guid jobId, Guid projectId, string workspacePath);
}
