using System;

namespace DevForge.ControlPlane.Domain.Entities;

public class UsageRecord : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid? JobId { get; set; }
    public GenerationJob? GenerationJob { get; set; }

    public int GeminiTokens { get; set; }
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }

    public double GenerationTime { get; set; } // in seconds
    public double BuildTime { get; set; } // in seconds
    public double StorageUsage { get; set; } // in MB
    
    public int DeploymentsCount { get; set; }
    public int RepairAttemptsCount { get; set; }
    
    public double EstimatedLlmCost { get; set; }
    public double EstimatedComputeCost { get; set; }
    public double EstimatedStorageCost { get; set; }
    public double EstimatedCost { get; set; } // For backwards compatibility
    public double EstimatedTotalCost { get; set; }
    
    public DateTimeOffset RecordedAt { get; set; } = DateTimeOffset.UtcNow;
}
