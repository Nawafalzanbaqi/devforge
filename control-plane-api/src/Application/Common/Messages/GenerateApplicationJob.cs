using System;

namespace DevForge.ControlPlane.Application.Common.Messages;

public record GenerateApplicationJob
{
    public Guid JobId { get; init; }
    public Guid ProjectId { get; init; }
}
