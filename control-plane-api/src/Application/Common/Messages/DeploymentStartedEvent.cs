using System;

namespace DevForge.ControlPlane.Application.Common.Messages;

public record DeploymentStartedEvent
{
    public Guid JobId { get; init; }
    public Guid ProjectId { get; init; }
    public Guid DeploymentId { get; init; }
}
