using System;

namespace DevForge.ControlPlane.Domain.Entities;

public class CandidateTemplateRelease : BaseEntity
{
    public Guid TemplateId { get; set; }
    public TemplateCatalog Template { get; set; } = null!;

    public string SourceVersion { get; set; } = string.Empty;
    public string CandidateVersion { get; set; } = string.Empty;
    public string GeneratedPatch { get; set; } = string.Empty;

    public string BuildStatus { get; set; } = "Pending"; // Pending, Passed, Failed
    public string QaStatus { get; set; } = "Pending"; // Pending, Passed, Failed
    public string DeploymentStatus { get; set; } = "Pending"; // Pending, Passed, Failed

    public double ImprovementScore { get; set; }
    public string ApprovalStatus { get; set; } = "Pending"; // Pending, Approved, Rejected, Applied
    public DateTimeOffset? ProcessedAt { get; set; }
}
