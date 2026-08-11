using System;

namespace DevForge.ControlPlane.Domain.Entities;

public class OptimizationKnowledge : BaseEntity
{
    public string RecommendationType { get; set; } = string.Empty;
    public int Executions { get; set; }
    public int Successes { get; set; }
    public int Failures { get; set; }
    public double AverageImprovement { get; set; }
    public double ConfidenceScore { get; set; }
}
