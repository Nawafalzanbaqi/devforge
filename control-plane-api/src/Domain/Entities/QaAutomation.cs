using System;
using System.Collections.Generic;

namespace DevForge.ControlPlane.Domain.Entities;

public class QaExecution : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid BuildId { get; set; } // Map to GenerationJobId
    public GenerationJob GenerationJob { get; set; } = null!;

    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }

    public string Status { get; set; } = "InProgress"; // InProgress, Passed, Failed

    public decimal CoveragePercent { get; set; }
    public int QualityScore { get; set; }
    public int SecurityScore { get; set; }
    public int BugCount { get; set; }
    public int WarningCount { get; set; }
    public bool Passed { get; set; }

    public ICollection<QaIssue> Issues { get; set; } = new List<QaIssue>();
}

public class QaIssue : BaseEntity
{
    public Guid QaExecutionId { get; set; }
    public QaExecution QaExecution { get; set; } = null!;

    public string Severity { get; set; } = "Info"; // Info, Minor, Major, Critical, Blocker
    public string Type { get; set; } = "Sonar"; // Sonar, Playwright, Compiler
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public int LineNumber { get; set; }
}
