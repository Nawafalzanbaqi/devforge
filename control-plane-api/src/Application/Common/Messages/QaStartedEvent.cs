using System;

namespace DevForge.ControlPlane.Application.Common.Messages;

public record QaStartedEvent
{
    public Guid JobId { get; init; }
    public Guid ProjectId { get; init; }
    public Guid QaExecutionId { get; init; }
}
