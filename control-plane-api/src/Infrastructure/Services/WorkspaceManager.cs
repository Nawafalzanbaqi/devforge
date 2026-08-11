using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using DevForge.ControlPlane.Application.Common.Interfaces;

namespace DevForge.ControlPlane.Infrastructure.Services;

public class WorkspaceManager : IWorkspaceManager
{
    private readonly string _rootPath;

    public WorkspaceManager(IConfiguration configuration)
    {
        // Target absolute workspace parent folder
        _rootPath = configuration["Workspaces:RootPath"] 
                    ?? Path.Combine("C:\\Users\\alzan\\Desktop\\test\\devforge", "workspaces");
    }

    public string GetWorkspacePath(Guid jobId)
    {
        return Path.Combine(_rootPath, $"job-{jobId}");
    }

    public string CreateWorkspace(Guid jobId)
    {
        var path = GetWorkspacePath(jobId);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        return path;
    }

    public void DeleteWorkspace(Guid jobId)
    {
        var path = GetWorkspacePath(jobId);
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }

    public void WriteFile(Guid jobId, string relativePath, string content)
    {
        var workspacePath = GetWorkspacePath(jobId);
        var filePath = Path.Combine(workspacePath, relativePath);
        
        // Ensure directories containing file exist
        var directory = Path.GetDirectoryName(filePath);
        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(filePath, content);
    }

    public string ReadFile(Guid jobId, string relativePath)
    {
        var workspacePath = GetWorkspacePath(jobId);
        var filePath = Path.Combine(workspacePath, relativePath);
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Target file was not found inside workspace: {relativePath}");
        }

        return File.ReadAllText(filePath);
    }

    public bool FileExists(Guid jobId, string relativePath)
    {
        var workspacePath = GetWorkspacePath(jobId);
        var filePath = Path.Combine(workspacePath, relativePath);
        return File.Exists(filePath);
    }
}
