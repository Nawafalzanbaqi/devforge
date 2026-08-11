using System;
using System.Threading.Tasks;

namespace DevForge.ControlPlane.Application.Common.Interfaces;

public interface IRepairContextBuilder
{
    Task<string> BuildContextAsync(Guid projectId, Guid buildId, string source, string errorMessage, string? filePath, int lineNumber, string workspacePath);
}
