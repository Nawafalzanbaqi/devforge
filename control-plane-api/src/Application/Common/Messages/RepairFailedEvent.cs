using System;

namespace DevForge.ControlPlane.Application.Common.Messages;

public record RepairFailedEvent
{
    public Guid JobId { get; init; }
    public Guid ProjectId { get; init; }
    public Guid TicketId { get; init; }
    public string ErrorSummary { get; init; } = string.Empty;
}
