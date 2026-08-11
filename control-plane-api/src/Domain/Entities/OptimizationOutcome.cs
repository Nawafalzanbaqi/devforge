using System;

namespace DevForge.ControlPlane.Domain.Entities;

public class OptimizationOutcome : BaseEntity
{
    public Guid RecommendationId { get; set; }
    public OptimizationRecommendation Recommendation { get; set; } = null!;

    public double BaselineMetric { get; set; }
    public double NewMetric { get; set; }
    public double ImprovementPercent { get; set; }
    public DateTimeOffset AppliedAt { get; set; }
    public DateTimeOffset VerifiedAt { get; set; }
}
