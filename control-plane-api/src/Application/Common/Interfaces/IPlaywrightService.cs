using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevForge.ControlPlane.Application.Common.Interfaces;

public class PlaywrightResult
{
    public bool Success { get; set; }
    public List<TestResultDetail> Details { get; set; } = new();
    public string LogOutput { get; set; } = string.Empty;
}

public class TestResultDetail
{
    public string Suite { get; set; } = string.Empty;
    public string TestName { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public string? ErrorMessage { get; set; }
    public int DurationMs { get; set; }
}

public interface IPlaywrightService
{
    Task<PlaywrightResult> RunSmokeTestsAsync(Guid projectId, Guid buildId, string appUrl, string workspacePath);
}
