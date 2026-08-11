using System;
using System.Collections.Generic;

namespace DevForge.ControlPlane.Domain.Entities;

public class BillingAccount : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public string PlanTier { get; set; } = "Free"; // Free, Pro, Agency, Enterprise
    public double CurrentBalance { get; set; }
    public string Status { get; set; } = "Active"; // Active, PastDue, Suspended
    
    // Limits
    public int MonthlyTokenLimit { get; set; } = 500000;
    public double MonthlyStorageLimit { get; set; } = 1024.0; // in MB
    public int MonthlyDeploymentLimit { get; set; } = 5;
    public int MonthlyBuildMinutesLimit { get; set; } = 60;
    public int MonthlyRepairLimit { get; set; } = 10;

    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
