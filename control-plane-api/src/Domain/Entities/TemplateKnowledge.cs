using System;

namespace DevForge.ControlPlane.Domain.Entities;

public class TemplateKnowledge : BaseEntity
{
    public Guid TemplateId { get; set; }
    public TemplateCatalog Template { get; set; } = null!;

    public int Executions { get; set; }
    public int Repairs { get; set; }
    public int Deployments { get; set; }
    public int Failures { get; set; }
    public double SuccessRate { get; set; }
    public double ConfidenceScore { get; set; }
}
