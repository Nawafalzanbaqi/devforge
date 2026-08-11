using System;

namespace DevForge.ControlPlane.Domain.Entities;

public class OptimizationRecommendation : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string Category { get; set; } = string.Empty; // Performance, Cost, Reliability
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RecommendedAction { get; set; } = string.Empty;
    public double EstimatedSavingsValue { get; set; }
    public string SavingsUnit { get; set; } = string.Empty; // "Tokens", "Seconds", "USD"
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    public double Score { get; set; }
    public string RiskLevel { get; set; } = "Low"; // Low, Medium, High
    public PatchProposal? PatchProposal { get; set; }
    public OptimizationOutcome? Outcome { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedAt { get; set; }
}
