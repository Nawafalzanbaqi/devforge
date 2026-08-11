using System;

namespace DevForge.ControlPlane.Application.Common.Messages;

public record BuildCompletedEvent
{
    public Guid JobId { get; init; }
    public Guid ProjectId { get; init; }
    public string WorkspacePath { get; init; } = string.Empty;
}
