using System;
using System.Collections.Generic;

namespace DevForge.ControlPlane.Domain.Entities;

public class GenerationJob : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public Guid SchemaId { get; set; }
    public Schema Schema { get; set; } = null!;

    public Guid? TemplateId { get; set; }
    public TemplateCatalog? Template { get; set; }
    
    public string Status { get; set; } = "Pending";
    public Guid? InitiatedBy { get; set; }
    
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? ErrorSummary { get; set; }
    
    public ICollection<BuildResult> BuildResults { get; set; } = new List<BuildResult>();
    public ICollection<QaResult> QaResults { get; set; } = new List<QaResult>();
    public ICollection<AgentWorkflow> AgentWorkflows { get; set; } = new List<AgentWorkflow>();
}

public class BuildResult : BaseEntity
{
    public Guid GenerationJobId { get; set; }
    public GenerationJob GenerationJob { get; set; } = null!;
    
    public string Layer { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? LogOutput { get; set; }
    public string? CompilationErrors { get; set; }
    public int DurationMs { get; set; }
}

public class Deployment : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public Guid? BuildId { get; set; }
    public GenerationJob? GenerationJob { get; set; }
    
    public int Version { get; set; } = 1;
    public string Environment { get; set; } = "LocalDev";
    public string Domain { get; set; } = string.Empty;
    public string? ContainerId { get; set; }
    public string ImageTag { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Provisioning, Deploying, Running, Failed, Stopped, Archived
    
    public DateTimeOffset? CompletedAt { get; set; }
    public string HealthStatus { get; set; } = "Offline"; // Healthy, Warning, Degraded, Offline
    public int HealthScore { get; set; } = 0; // 0 - 100
}

public class QaResult : BaseEntity
{
    public Guid GenerationJobId { get; set; }
    public GenerationJob GenerationJob { get; set; } = null!;
    
    public decimal SonarScanScore { get; set; } = 100.00m;
    public string? SonarScanRaw { get; set; }
    
    public bool PlaywrightTestSuccess { get; set; }
    public string? PlaywrightTestRaw { get; set; }
}

public class AgentWorkflow
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GenerationJobId { get; set; }
    public GenerationJob GenerationJob { get; set; } = null!;
    
    public string AgentName { get; set; } = string.Empty;
    public string StepName { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    
    public string? InputPayload { get; set; }
    public string? OutputPayload { get; set; }
    
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }
}
