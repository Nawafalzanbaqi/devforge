using System;

namespace DevForge.ControlPlane.Domain.Entities;

public class ManufacturingTelemetry : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid JobId { get; set; }
    public GenerationJob GenerationJob { get; set; } = null!;

    public string MetricName { get; set; } = string.Empty;
    public double MetricValue { get; set; }
    public string Unit { get; set; } = string.Empty;

    public DateTimeOffset RecordedAt { get; set; } = DateTimeOffset.UtcNow;
}
