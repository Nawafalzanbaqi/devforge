using System;

namespace DevForge.ControlPlane.Application.Common.Messages;

public record RepairSucceededEvent
{
    public Guid JobId { get; init; }
    public Guid ProjectId { get; init; }
    public Guid TicketId { get; init; }
}
