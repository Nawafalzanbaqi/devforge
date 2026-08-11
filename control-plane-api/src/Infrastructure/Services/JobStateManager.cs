using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DevForge.ControlPlane.Application.Common.Interfaces;
using DevForge.ControlPlane.Domain.Entities;

namespace DevForge.ControlPlane.Infrastructure.Services;

public class JobStateManager : IJobStateManager
{
    private readonly IApplicationDbContext _context;

    public JobStateManager(IApplicationDbContext context)
    {
        _context = context;
    }

    private async Task UpdateJobStatusAsync(Guid jobId, string status, string? errorSummary = null)
    {
        var job = await _context.GenerationJobs
            .Include(j => j.Project)
            .FirstOrDefaultAsync(j => j.Id == jobId);
            
        if (job == null) return;

        job.Status = status;
        
        // Synchronize parent project status
        if (job.Project != null)
        {
            if (status == "Completed")
            {
                job.Project.Status = "Completed";
            }
            else if (status == "Failed")
            {
                job.Project.Status = "Failed";
            }
            else
            {
                job.Project.Status = status; // e.g., Compiling, Testing, Generating
            }
        }

        if (status == "Completed" || status == "Failed")
        {
            job.CompletedAt = DateTimeOffset.UtcNow;
            job.ErrorSummary = errorSummary;
        }

        // Add Workflow log entry
        var workflow = new AgentWorkflow
        {
            GenerationJobId = jobId,
            AgentName = "ControlPlaneOrchestrator",
            StepName = $"Transitioned to {status}",
            Status = "Succeeded",
            StartedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow
        };
        _context.AgentWorkflows.Add(workflow);

        await _context.SaveChangesAsync();
    }

    public Task TransitionToQueuedAsync(Guid jobId) => UpdateJobStatusAsync(jobId, "Queued");
    public Task TransitionToGeneratingAsync(Guid jobId) => UpdateJobStatusAsync(jobId, "Generating");
    public Task TransitionToCompilingAsync(Guid jobId) => UpdateJobStatusAsync(jobId, "Compiling");
    public Task TransitionToTestingAsync(Guid jobId) => UpdateJobStatusAsync(jobId, "Testing");
    public Task TransitionToDeployingAsync(Guid jobId) => UpdateJobStatusAsync(jobId, "Deploying");
    public Task TransitionToPassedQaAsync(Guid jobId) => UpdateJobStatusAsync(jobId, "PassedQA");
    public Task TransitionToFailedQaAsync(Guid jobId, string errorSummary) => UpdateJobStatusAsync(jobId, "FailedQA", errorSummary);
    public Task TransitionToCompletedAsync(Guid jobId) => UpdateJobStatusAsync(jobId, "Completed");
    public Task TransitionToFailedAsync(Guid jobId, string errorSummary) => UpdateJobStatusAsync(jobId, "Failed", errorSummary);
}
