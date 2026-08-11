using System;

namespace DevForge.ControlPlane.Application.Common.Messages;

public record QaFailedEvent
{
    public Guid JobId { get; init; }
    public Guid ProjectId { get; init; }
    public Guid QaExecutionId { get; init; }
    public int QualityScore { get; init; }
    public string ErrorSummary { get; init; } = string.Empty;
}
