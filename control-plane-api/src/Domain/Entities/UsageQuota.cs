using System;

namespace DevForge.ControlPlane.Domain.Entities;

public class UsageQuota : BaseEntity
{
    public Guid BillingAccountId { get; set; }
    public BillingAccount BillingAccount { get; set; } = null!;

    public string QuotaType { get; set; } = string.Empty; // Tokens, Storage, Deployments, BuildMinutes, Repairs
    public double LimitValue { get; set; }
    public double UsedValue { get; set; }
    public DateTimeOffset PeriodStart { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset PeriodEnd { get; set; } = DateTimeOffset.UtcNow.AddMonths(1);
}
