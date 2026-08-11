using System;

namespace DevForge.ControlPlane.Domain.Entities;

public class TemplateCatalog : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public string Framework { get; set; } = ".NET 9 / Next.js 15";
    public string Path { get; set; } = string.Empty; // Absolute path on the host system to template assets
    public string Description { get; set; } = string.Empty;
    
    public string Category { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    
    public double SuccessRate { get; set; } = 100.0;
    public double RepairRate { get; set; } = 0.0;
    public double AverageLeadTime { get; set; } = 0.0;
    public int TemplateUsageCount { get; set; } = 0;
}
