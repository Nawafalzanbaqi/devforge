using System;

namespace DevForge.ControlPlane.Domain.Entities;

public class PromptTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string TargetLayer { get; set; } = string.Empty;
    public string TemplateContent { get; set; } = string.Empty;
    public string? Variables { get; set; }
}

public class AiUsage : BaseEntity
{
    public Guid? ProjectId { get; set; }
    public Project? Project { get; set; }
    
    public Guid? GenerationJobId { get; set; }
    public GenerationJob? GenerationJob { get; set; }
    
    public string ModelName { get; set; } = string.Empty;
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public decimal Cost { get; set; }
}

public class Billing : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    
    public string SubscriptionPlan { get; set; } = "Free";
    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }
    
    public DateTimeOffset? BillingCycleStart { get; set; }
    public DateTimeOffset? BillingCycleEnd { get; set; }
    public string Status { get; set; } = "Active";
}

public class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
}

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
}

public class ApiKey : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public string KeyHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Scopes { get; set; } = string.Empty;
    
    public DateTimeOffset? ExpiresAt { get; set; }
}
