using System;

namespace DevForge.ControlPlane.Domain.Entities;

public class Invoice : BaseEntity
{
    public Guid BillingAccountId { get; set; }
    public BillingAccount BillingAccount { get; set; } = null!;

    public DateTimeOffset IssueDate { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset DueDate { get; set; } = DateTimeOffset.UtcNow.AddDays(15);
    public double Amount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Paid, Overdue, Voided

    // Aggregated usage metrics for the invoice period
    public int UsedTokens { get; set; }
    public double UsedStorage { get; set; }
    public int UsedDeployments { get; set; }
    public double UsedBuildMinutes { get; set; }
    public int UsedRepairAttempts { get; set; }
}
