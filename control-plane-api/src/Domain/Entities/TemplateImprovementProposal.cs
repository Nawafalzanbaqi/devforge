using System;

namespace DevForge.ControlPlane.Domain.Entities;

public class TemplateImprovementProposal : BaseEntity
{
    public Guid TemplateId { get; set; }
    public TemplateCatalog Template { get; set; } = null!;

    public string Reason { get; set; } = string.Empty;
    public string Evidence { get; set; } = string.Empty;
    public string PatchProposal { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = "Low"; // Low, Medium, High
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Applied
    public DateTimeOffset? ProcessedAt { get; set; }
}
