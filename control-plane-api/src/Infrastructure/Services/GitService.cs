using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DevForge.ControlPlane.Application.Common.Interfaces;

namespace DevForge.ControlPlane.Infrastructure.Services;

public class GitService : IGitService
{
    private readonly ILogger<GitService> _logger;

    public GitService(ILogger<GitService> logger)
    {
        _logger = logger;
    }

    public async Task InitializeRepositoryAsync(string path)
    {
        _logger.LogInformation("Initializing Git repository at {Path}", path);
        try
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            await RunCommandAsync(path, "init");
            await SetLocalConfigAsync(path);
            await WriteDefaultGitignoreAsync(path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize git repository at {Path}", path);
            throw;
        }
    }

    public async Task CommitChangesAsync(string path, string message)
    {
        _logger.LogInformation("Committing changes at {Path} with message: '{Message}'", path, message);
        try
        {
            await RunCommandAsync(path, "add .");
            
            // Check if there are changes to commit to avoid failing on nothing to commit
            var statusOutput = await RunCommandAsync(path, "status --porcelain");
            if (string.IsNullOrWhiteSpace(statusOutput))
            {
                _logger.LogInformation("No changes to commit in {Path}.", path);
                return;
            }

            // Perform commit. Escape quotes safely.
            var escapedMessage = message.Replace("\"", "\\\"");
            await RunCommandAsync(path, $"commit -m \"{escapedMessage}\"");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to commit changes in {Path}. It might be due to an empty commit state.", path);
        }
    }

    public async Task PushChangesAsync(string path, string remoteUrl)
    {
        if (string.IsNullOrWhiteSpace(remoteUrl))
        {
            _logger.LogInformation("No repository remote URL provided. Skipping git push.");
            return;
        }

        _logger.LogInformation("Pushing changes to remote: {RemoteUrl}", remoteUrl);
        try
        {
            // Configure remote
            var remotes = await RunCommandAsync(path, "remote");
            if (remotes.Contains("origin"))
            {
                await RunCommandAsync(path, $"remote set-url origin {remoteUrl}");
            }
            else
            {
                await RunCommandAsync(path, $"remote add origin {remoteUrl}");
            }

            // Determine branch name
            var branchOutput = await RunCommandAsync(path, "rev-parse --abbrev-ref HEAD");
            var branch = branchOutput.Trim();
            if (string.IsNullOrEmpty(branch) || branch == "HEAD")
            {
                branch = "main";
                await RunCommandAsync(path, "branch -M main");
            }

            // Push changes (ignore exit failure in push if credentials are not set on this machine)
            await RunCommandAsync(path, $"push -u origin {branch}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to push to remote repository: {RemoteUrl}. Local commit history is intact.", remoteUrl);
        }
    }

    public async Task CreateBranchAsync(string path, string branchName)
    {
        _logger.LogInformation("Creating Git branch {Branch} at {Path}", branchName, path);
        try
        {
            await RunCommandAsync(path, $"branch {branchName}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create branch {Branch}. It might already exist.", branchName);
        }
    }

    public async Task CheckoutBranchAsync(string path, string branchName)
    {
        _logger.LogInformation("Checking out Git branch {Branch} at {Path}", branchName, path);
        try
        {
            await RunCommandAsync(path, $"checkout {branchName}");
        }
        catch (Exception ex)
        {
            try
            {
                await RunCommandAsync(path, $"checkout -b {branchName}");
            }
            catch (Exception innerEx)
            {
                _logger.LogError(innerEx, "Failed to checkout branch {Branch}.", branchName);
                throw;
            }
        }
    }

    public async Task MergeBranchAsync(string path, string sourceBranch, string targetBranch)
    {
        _logger.LogInformation("Merging branch {Source} into {Target} at {Path}", sourceBranch, targetBranch, path);
        try
        {
            await RunCommandAsync(path, $"checkout {targetBranch}");
            await RunCommandAsync(path, $"merge {sourceBranch}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Merge failed between {Source} and {Target} due to merge conflicts or git issues.", sourceBranch, targetBranch);
            throw;
        }
    }

    private async Task SetLocalConfigAsync(string path)
    {
        try
        {
            await RunCommandAsync(path, "config user.name \"DevForge Agent\"");
            await RunCommandAsync(path, "config user.email \"agent@devforge.ai\"");
            await RunCommandAsync(path, "config init.defaultBranch main");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to configure local git credentials in {Path}.", path);
        }
    }

    private async Task WriteDefaultGitignoreAsync(string path)
    {
        try
        {
            var gitignorePath = Path.Combine(path, ".gitignore");
            if (!File.Exists(gitignorePath))
            {
                var gitignoreContent = """
                # .NET template ignore files
                bin/
                obj/
                *.user
                *.suo
                *.userosscache
                *.sln.docstates
                
                # Node / Frontend ignores
                node_modules/
                .next/
                out/
                build/
                dist/
                .env*.local
                npm-debug.log*
                yarn-debug.log*
                yarn-error.log*
                .pnpm-debug.log*
                
                # OS files
                .DS_Store
                Thumbs.db
                """;
                await File.WriteAllTextAsync(gitignorePath, gitignoreContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write .gitignore in {Path}.", path);
        }
    }

    private async Task<string> RunCommandAsync(string workingDirectory, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        var output = await outputTask;
        var error = await errorTask;

        if (process.ExitCode != 0)
        {
            throw new Exception($"Git command failed (git {arguments}) with exit code {process.ExitCode}. Error: {error.Trim()}");
        }

        return output;
    }
}
