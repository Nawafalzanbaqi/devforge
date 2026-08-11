using System;

namespace DevForge.ControlPlane.Domain.Entities;

public class Alert : BaseEntity
{
    public string AlertName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "Warning"; // Critical, Warning, Info
    public string Status { get; set; } = "Active"; // Active, Resolved
    public DateTimeOffset TriggeredAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ResolvedAt { get; set; }
    public Guid? ProjectId { get; set; }
    public Project? Project { get; set; }
}
