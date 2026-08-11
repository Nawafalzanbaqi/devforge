using System;

namespace DevForge.ControlPlane.Application.Common.Interfaces;

public interface IWorkspaceManager
{
    string CreateWorkspace(Guid jobId);
    void DeleteWorkspace(Guid jobId);
    void WriteFile(Guid jobId, string relativePath, string content);
    string ReadFile(Guid jobId, string relativePath);
    bool FileExists(Guid jobId, string relativePath);
    string GetWorkspacePath(Guid jobId);
}
