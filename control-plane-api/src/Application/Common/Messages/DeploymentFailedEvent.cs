using System;

namespace DevForge.ControlPlane.Application.Common.Messages;

public record DeploymentFailedEvent
{
    public Guid JobId { get; init; }
    public Guid ProjectId { get; init; }
    public Guid DeploymentId { get; init; }
    public string ErrorSummary { get; init; } = string.Empty;
}
