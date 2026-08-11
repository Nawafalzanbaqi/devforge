using System;

namespace DevForge.ControlPlane.Domain.Entities;

public class PatchProposal : BaseEntity
{
    public Guid RecommendationId { get; set; }
    public OptimizationRecommendation Recommendation { get; set; } = null!;

    public string PatchType { get; set; } = string.Empty;
    public string Diff { get; set; } = string.Empty;
    public string FilesChanged { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = "Low"; // Low, Medium, High
}
