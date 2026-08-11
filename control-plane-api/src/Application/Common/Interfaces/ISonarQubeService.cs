using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevForge.ControlPlane.Application.Common.Interfaces;

public class QualityGateResult
{
    public bool Passed { get; set; }
    public string Status { get; set; } = "Passed";
    public List<string> Details { get; set; } = new();
}

public class SonarIssue
{
    public string Key { get; set; } = string.Empty;
    public string Rule { get; set; } = string.Empty;
    public string Severity { get; set; } = "Info"; // Info, Minor, Major, Critical, Blocker
    public string Message { get; set; } = string.Empty;
    public string Component { get; set; } = string.Empty; // File path
    public int Line { get; set; }
}

public class QualityMetrics
{
    public decimal Coverage { get; set; }
    public int CriticalVulnerabilities { get; set; }
    public int BlockerIssues { get; set; }
    public decimal DuplicationDensity { get; set; }
    public string SecurityRating { get; set; } = "A"; // A, B, C, D, E
}

public interface ISonarQubeService
{
    Task<QualityGateResult> AnalyzeAsync(string workspacePath, string projectKey, string projectName);
    Task<IReadOnlyList<SonarIssue>> GetIssuesAsync(string projectKey);
    Task<QualityMetrics> GetMetricsAsync(string projectKey);
}
