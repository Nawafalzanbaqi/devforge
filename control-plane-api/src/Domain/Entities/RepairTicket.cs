using System;

namespace DevForge.ControlPlane.Domain.Entities;

public class RepairTicket : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid? BuildId { get; set; } // Map to GenerationJobId
    public GenerationJob? GenerationJob { get; set; }

    public string Source { get; set; } = string.Empty; // Compiler, Playwright, Sonar
    public string Severity { get; set; } = "Major"; // Info, Minor, Major, Critical, Blocker
    public string Status { get; set; } = "Open"; // Open, Analyzing, Repairing, Validating, Resolved, Failed
    public string FailureType { get; set; } = string.Empty; // CS0103, ElementNotFound, etc.
    public string ErrorMessage { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public int LineNumber { get; set; }
    public int Attempts { get; set; } = 0;

    public DateTimeOffset? ResolvedAt { get; set; }
}
