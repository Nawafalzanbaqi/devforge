using System;

namespace DevForge.ControlPlane.Domain.Entities;

public class TemplateVersionHistory : BaseEntity
{
    public Guid TemplateId { get; set; }
    public TemplateCatalog Template { get; set; } = null!;

    public string Version { get; set; } = string.Empty;
    public string ParentVersion { get; set; } = string.Empty;

    public Guid? ImprovementProposalId { get; set; }
    public TemplateImprovementProposal? ImprovementProposal { get; set; }

    public double SuccessRate { get; set; }
    public double RepairRate { get; set; }
    public double LeadTime { get; set; }
    public DateTimeOffset ReleasedAt { get; set; } = DateTimeOffset.UtcNow;
}
