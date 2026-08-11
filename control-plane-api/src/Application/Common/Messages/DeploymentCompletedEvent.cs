using System;

namespace DevForge.ControlPlane.Application.Common.Messages;

public record DeploymentCompletedEvent
{
    public Guid JobId { get; init; }
    public Guid ProjectId { get; init; }
    public Guid DeploymentId { get; init; }
    public string DomainUrl { get; init; } = string.Empty;
}
