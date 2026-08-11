using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DevForge.ControlPlane.Application.Common.Interfaces;
using DevForge.ControlPlane.Application.Common.Messages;
using DevForge.ControlPlane.Domain.Entities;

namespace DevForge.ControlPlane.DeploymentWorker.Consumers;

public class QaPassedEventConsumer : IConsumer<QaPassedEvent>
{
    private readonly ILogger<QaPassedEventConsumer> _logger;
    private readonly IJobStateManager _stateManager;
    private readonly IWorkspaceManager _workspaceManager;
    private readonly IDeploymentService _deploymentService;
    private readonly IApplicationDbContext _db;

    public QaPassedEventConsumer(
        ILogger<QaPassedEventConsumer> logger,
        IJobStateManager stateManager,
        IWorkspaceManager workspaceManager,
        IDeploymentService deploymentService,
        IApplicationDbContext db)
    {
        _logger = logger;
        _stateManager = stateManager;
        _workspaceManager = workspaceManager;
        _deploymentService = deploymentService;
        _db = db;
    }

    public async Task Consume(ConsumeContext<QaPassedEvent> context)
    {
        var jobId = context.Message.JobId;
        var projectId = context.Message.ProjectId;

        _logger.LogInformation("DeploymentWorker received QaPassedEvent: Job {JobId}, Project {ProjectId}", jobId, projectId);

        // 1. Transition Job State to Deploying
        await _stateManager.TransitionToDeployingAsync(jobId);

        // 2. Load the project and schema details to determine version number
        var job = await _db.GenerationJobs
            .Include(j => j.Project)
            .Include(j => j.Schema)
            .FirstOrDefaultAsync(j => j.Id == jobId);

        var version = job?.Schema?.Version ?? 1;

        // 3. Create initial Deployment record
        var deployment = new Deployment
        {
            ProjectId = projectId,
            BuildId = jobId,
            Version = version,
            Environment = "Production",
            Status = "Deploying",
            HealthStatus = "Offline",
            HealthScore = 0,
            Domain = "pending.local"
        };
        _db.Deployments.Add(deployment);
        await _db.SaveChangesAsync();

        // 4. Publish DeploymentStartedEvent
        await context.Publish(new DeploymentStartedEvent
        {
            JobId = jobId,
            ProjectId = projectId,
            DeploymentId = deployment.Id
        });

        try
        {
            // 5. Orchestrate compilation of compose files and health diagnostics
            var workspacePath = _workspaceManager.GetWorkspacePath(jobId);
            var deployResult = await _deploymentService.DeployAsync(jobId, projectId, workspacePath);

            if (deployResult.Success)
            {
                // Update database deployment record
                deployment.Status = "Running";
                deployment.Domain = deployResult.AllocatedDomain ?? "unknown.local";
                deployment.ContainerId = deployResult.ContainerId;
                deployment.ImageTag = deployResult.ImageTag;
                deployment.HealthStatus = deployResult.HealthStatus;
                deployment.HealthScore = deployResult.HealthScore;
                deployment.CompletedAt = DateTimeOffset.UtcNow;
                
                await _db.SaveChangesAsync();

                // 6. Transition Job to Completed
                await _stateManager.TransitionToCompletedAsync(jobId);

                // 7. Publish DeploymentCompletedEvent
                await context.Publish(new DeploymentCompletedEvent
                {
                    JobId = jobId,
                    ProjectId = projectId,
                    DeploymentId = deployment.Id,
                    DomainUrl = $"http://{deployment.Domain}"
                });

                _logger.LogInformation("DeploymentWorker successfully processed deployment for Job {JobId}. Live at http://{Domain}", jobId, deployment.Domain);
            }
            else
            {
                throw new Exception(deployResult.ErrorMessage ?? "Deployment health checks failed.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deploy application for Job {JobId}", jobId);

            // Update database deployment record
            deployment.Status = "Failed";
            deployment.CompletedAt = DateTimeOffset.UtcNow;
            deployment.HealthStatus = "Offline";
            deployment.HealthScore = 0;
            
            await _db.SaveChangesAsync();

            // Transition Job to Failed
            await _stateManager.TransitionToFailedAsync(jobId, ex.Message);

            // Publish DeploymentFailedEvent
            await context.Publish(new DeploymentFailedEvent
            {
                JobId = jobId,
                ProjectId = projectId,
                DeploymentId = deployment.Id,
                ErrorSummary = ex.Message
            });
        }
    }
}
