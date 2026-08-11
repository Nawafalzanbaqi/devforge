using System;

namespace DevForge.ControlPlane.Domain.Entities;

public class ClientMetrics : BaseEntity
{
    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;

    public int TotalProjects { get; set; }
    public int TotalDeployments { get; set; }
    public double AverageLeadTime { get; set; }
    public double SuccessRate { get; set; } = 100.0;
    public double RepairRate { get; set; } = 0.0;
    public int MonthlyBuilds { get; set; }
    public double Revenue { get; set; }
    public double GrossMargin { get; set; }
}
