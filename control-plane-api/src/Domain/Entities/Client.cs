using System;
using System.Collections.Generic;

namespace DevForge.ControlPlane.Domain.Entities;

public class Client : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double Revenue { get; set; } = 0.0;

    public ClientMetrics? Metrics { get; set; }
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
