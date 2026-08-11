using System;

namespace DevForge.ControlPlane.Domain.Entities;

public class Subscription : BaseEntity
{
    public Guid BillingAccountId { get; set; }
    public BillingAccount BillingAccount { get; set; } = null!;

    public string PlanName { get; set; } = "Free"; // Free, Pro, Agency, Enterprise
    public string Status { get; set; } = "Active"; // Active, PastDue, Canceled
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset EndsAt { get; set; } = DateTimeOffset.UtcNow.AddMonths(1);
}
