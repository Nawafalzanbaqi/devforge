using System;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DevForge.ControlPlane.Application.Common.Interfaces;
using DevForge.ControlPlane.Application.Common.Messages;
using DevForge.ControlPlane.Domain.Entities;

namespace DevForge.ControlPlane.MonitoringWorker.Consumers;

public class PipelineTelemetryConsumer : 
    IConsumer<BuildCompletedEvent>,
    IConsumer<QaPassedEvent>,
    IConsumer<QaFailedEvent>,
    IConsumer<DeploymentCompletedEvent>,
    IConsumer<DeploymentFailedEvent>,
    IConsumer<RepairSucceededEvent>
{
    private readonly ILogger<PipelineTelemetryConsumer> _logger;
    private readonly ITelemetryService _telemetry;
    private readonly IApplicationDbContext _db;
    private readonly IAlertingService _alerting;

    public PipelineTelemetryConsumer(
        ILogger<PipelineTelemetryConsumer> logger,
        ITelemetryService telemetry,
        IApplicationDbContext db,
        IAlertingService alerting)
    {
        _logger = logger;
        _telemetry = telemetry;
        _db = db;
        _alerting = alerting;
    }

    public async Task Consume(ConsumeContext<BuildCompletedEvent> context)
    {
        var jobId = context.Message.JobId;
        var projectId = context.Message.ProjectId;

        _logger.LogInformation("Telemetry: BuildCompletedEvent received for Job {JobId}", jobId);

        var job = await _db.GenerationJobs.FindAsync(jobId);
        if (job != null)
        {
            var start = job.StartedAt ?? job.CreatedAt;
            var duration = (DateTimeOffset.UtcNow - start).TotalSeconds;

            await _telemetry.RecordDurationAsync(projectId, jobId, "Generation", duration);
            await _telemetry.RecordCounterAsync(projectId, jobId, "BuildSuccessRate", 1);
        }
    }

    public async Task Consume(ConsumeContext<QaPassedEvent> context)
    {
        var jobId = context.Message.JobId;
        var projectId = context.Message.ProjectId;

        _logger.LogInformation("Telemetry: QaPassedEvent received for Job {JobId}", jobId);

        var qa = await _db.QaExecutions
            .FirstOrDefaultAsync(q => q.BuildId == jobId);
        
        if (qa != null)
        {
            var duration = (DateTimeOffset.UtcNow - qa.StartedAt).TotalSeconds;
            await _telemetry.RecordDurationAsync(projectId, jobId, "QA", duration);
            await _telemetry.RecordCounterAsync(projectId, jobId, "BuildSuccessRate", 1);
        }

        await _telemetry.RecordCounterAsync(projectId, jobId, "devforge_jobs_completed_total", 1);
    }

    public async Task Consume(ConsumeContext<QaFailedEvent> context)
    {
        var jobId = context.Message.JobId;
        var projectId = context.Message.ProjectId;

        _logger.LogInformation("Telemetry: QaFailedEvent received for Job {JobId}", jobId);

        var qa = await _db.QaExecutions
            .FirstOrDefaultAsync(q => q.BuildId == context.Message.QaExecutionId || q.BuildId == jobId);

        if (qa != null)
        {
            var duration = (DateTimeOffset.UtcNow - qa.StartedAt).TotalSeconds;
            await _telemetry.RecordDurationAsync(projectId, jobId, "QA", duration);
        }

        await _telemetry.RecordCounterAsync(projectId, jobId, "devforge_jobs_failed_total", 1);
    }

    public async Task Consume(ConsumeContext<DeploymentCompletedEvent> context)
    {
        var jobId = context.Message.JobId;
        var projectId = context.Message.ProjectId;

        _logger.LogInformation("Telemetry: DeploymentCompletedEvent received for Job {JobId}", jobId);

        var deployment = await _db.Deployments
            .FirstOrDefaultAsync(d => d.BuildId == jobId);

        if (deployment != null)
        {
            var duration = (DateTimeOffset.UtcNow - deployment.CreatedAt).TotalSeconds;
            await _telemetry.RecordDurationAsync(projectId, jobId, "Deployment", duration);
            await _telemetry.RecordCounterAsync(projectId, jobId, "DeploymentSuccessRate", 1);
            await _alerting.ResolveAlertAsync("DeploymentFailed", projectId);
        }

        var job = await _db.GenerationJobs.FindAsync(jobId);
        if (job != null)
        {
            var leadTime = (DateTimeOffset.UtcNow - job.CreatedAt).TotalSeconds;
            await _telemetry.RecordDurationAsync(projectId, jobId, "LeadTimeToLive", leadTime);
        }
    }

    public async Task Consume(ConsumeContext<DeploymentFailedEvent> context)
    {
        var jobId = context.Message.JobId;
        var projectId = context.Message.ProjectId;

        _logger.LogInformation("Telemetry: DeploymentFailedEvent received for Job {JobId}", jobId);

        await _telemetry.RecordCounterAsync(projectId, jobId, "devforge_jobs_failed_total", 1);
        await _alerting.TriggerAlertAsync("DeploymentFailed", $"Deployment failed for Job {jobId} on Project {projectId}.", "Critical", projectId);
    }

    public async Task Consume(ConsumeContext<RepairSucceededEvent> context)
    {
        var jobId = context.Message.JobId;
        var projectId = context.Message.ProjectId;

        _logger.LogInformation("Telemetry: RepairSucceededEvent received for Job {JobId}", jobId);

        var ticket = await _db.RepairTickets
            .FirstOrDefaultAsync(t => t.BuildId == jobId && t.Status == "Resolved");

        if (ticket != null)
        {
            var duration = (DateTimeOffset.UtcNow - ticket.CreatedAt).TotalSeconds;
            await _telemetry.RecordDurationAsync(projectId, jobId, "Repair", duration);
            await _telemetry.RecordMetricAsync(projectId, jobId, "RepairAttempts", ticket.Attempts, "Count");
            await _telemetry.RecordCounterAsync(projectId, jobId, "RepairSuccessRate", 1);
        }
    }
}
