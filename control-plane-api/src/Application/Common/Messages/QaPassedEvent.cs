using System;

namespace DevForge.ControlPlane.Application.Common.Messages;

public record QaPassedEvent
{
    public Guid JobId { get; init; }
    public Guid ProjectId { get; init; }
    public Guid QaExecutionId { get; init; }
    public int QualityScore { get; init; }
}
