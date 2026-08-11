using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DevForge.ControlPlane.Application.Common.Interfaces;
using DevForge.ControlPlane.Domain.Entities;

namespace DevForge.ControlPlane.Infrastructure.Services;

public class AlertingService : IAlertingService
{
    private readonly IApplicationDbContext _db;
    private readonly ILogger<AlertingService> _logger;

    public AlertingService(IApplicationDbContext db, ILogger<AlertingService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task TriggerAlertAsync(string alertName, string message, string severity, Guid? projectId = null)
    {
        try
        {
            // Check if active alert already exists
            var existing = await _db.Alerts
                .FirstOrDefaultAsync(a => a.AlertName == alertName && a.ProjectId == projectId && a.Status == "Active");

            if (existing != null)
            {
                // Just log details if already active to prevent duplicates
                return;
            }

            _logger.LogWarning("ALERT TRIGGERED: [{Severity}] {Name} - {Message} (Project: {ProjectId})", 
                severity, alertName, message, projectId);

            // 1. Create database Alert record
            var alert = new Alert
            {
                AlertName = alertName,
                Message = message,
                Severity = severity,
                Status = "Active",
                TriggeredAt = DateTimeOffset.UtcNow,
                ProjectId = projectId
            };
            _db.Alerts.Add(alert);

            // 2. Add AuditLog entry
            var audit = new AuditLog
            {
                Action = "AlertTriggered",
                Details = $"Alert: {alertName} | Severity: {severity} | Msg: {message} | Project: {projectId}"
            };
            _db.AuditLogs.Add(audit);

            // 3. Create global User Notification (for the first user found or system scope)
            var user = await _db.Users.FirstOrDefaultAsync();
            if (user != null)
            {
                var notification = new Notification
                {
                    UserId = user.Id,
                    Title = $"System Alert: {alertName}",
                    Message = message,
                    IsRead = false
                };
                _db.Notifications.Add(notification);
            }

            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record active alert to database.");
        }
    }

    public async Task ResolveAlertAsync(string alertName, Guid? projectId = null)
    {
        try
        {
            var activeAlerts = await _db.Alerts
                .Where(a => a.AlertName == alertName && a.ProjectId == projectId && a.Status == "Active")
                .ToListAsync();

            if (activeAlerts.Count == 0) return;

            _logger.LogInformation("ALERT RESOLVED: {Name} (Project: {ProjectId})", alertName, projectId);

            foreach (var alert in activeAlerts)
            {
                alert.Status = "Resolved";
                alert.ResolvedAt = DateTimeOffset.UtcNow;
            }

            var audit = new AuditLog
            {
                Action = "AlertResolved",
                Details = $"Alert: {alertName} has been resolved. (Project: {projectId})"
            };
            _db.AuditLogs.Add(audit);

            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve active alert in database.");
        }
    }

    public async Task EvaluateMetricsAlertsAsync()
    {
        _logger.LogInformation("Evaluating metrics-based alerts thresholds...");

        // 1. Repair Success Rate < 80%
        try
        {
            var totalRepairs = await _db.RepairTickets.CountAsync();
            if (totalRepairs > 0)
            {
                var resolvedRepairs = await _db.RepairTickets.CountAsync(t => t.Status == "Resolved");
                var rate = (double)resolvedRepairs / totalRepairs;
                if (rate < 0.8)
                {
                    await TriggerAlertAsync(
                        "RepairSuccessRateLow",
                        $"Self-healing repair success rate is low: {rate:P1} ({resolvedRepairs}/{totalRepairs} resolved)",
                        "Warning");
                }
                else
                {
                    await ResolveAlertAsync("RepairSuccessRateLow");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking Repair success alerts.");
        }

        // 2. Lead Time > 15 Minutes
        try
        {
            var slowJobs = await _db.GenerationJobs
                .Where(j => j.Status == "Completed" && j.CompletedAt != null && j.StartedAt != null)
                .ToListAsync();

            foreach (var job in slowJobs)
            {
                var duration = ((job.CompletedAt ?? DateTimeOffset.UtcNow) - (job.StartedAt ?? job.CreatedAt)).TotalSeconds;
                if (duration > 900) // 15 Minutes SLA
                {
                    await TriggerAlertAsync(
                        "LeadTimeHigh",
                        $"Lead Time to Live for Job {job.Id} took {duration / 60:F1} mins, exceeding the 15-minute SLA limit.",
                        "Warning",
                        job.ProjectId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking Lead Time SLA alerts.");
        }

        // 3. Container Health = Failed
        try
        {
            var offlineDeployments = await _db.Deployments
                .Where(d => d.Status == "Failed" || d.HealthStatus == "Offline")
                .ToListAsync();

            foreach (var dep in offlineDeployments)
            {
                await TriggerAlertAsync(
                    "ContainerOffline",
                    $"Container {dep.ContainerId ?? "N/A"} for Project {dep.ProjectId} is offline or unhealthy.",
                    "Critical",
                    dep.ProjectId);
            }

            var healthyDeployments = await _db.Deployments
                .Where(d => d.Status == "Running" && d.HealthStatus == "Healthy")
                .ToListAsync();

            foreach (var dep in healthyDeployments)
            {
                await ResolveAlertAsync("ContainerOffline", dep.ProjectId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking Container Health status alerts.");
        }
    }
}
