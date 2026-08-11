using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DevForge.ControlPlane.Application.Common.Interfaces;
using DevForge.ControlPlane.Domain.Entities;

namespace DevForge.ControlPlane.Infrastructure.Services;

public class ClientMetricsService : IClientMetricsService
{
    private readonly IApplicationDbContext _db;
    private readonly ILogger<ClientMetricsService> _logger;

    public ClientMetricsService(IApplicationDbContext db, ILogger<ClientMetricsService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task RecalculateClientMetricsAsync(Guid clientId)
    {
        try
        {
            var client = await _db.Clients
                .Include(c => c.Metrics)
                .FirstOrDefaultAsync(c => c.Id == clientId);

            if (client == null)
            {
                _logger.LogWarning("Client with ID {ClientId} not found for metrics recalculation.", clientId);
                return;
            }

            var projectIds = await _db.Projects
                .Where(p => p.ClientId == clientId)
                .Select(p => p.Id)
                .ToListAsync();

            var totalProjects = projectIds.Count;
            var totalDeployments = await _db.Deployments.CountAsync(d => projectIds.Contains(d.ProjectId));

            var jobs = await _db.GenerationJobs
                .Where(j => projectIds.Contains(j.ProjectId))
                .ToListAsync();

            var totalJobs = jobs.Count;
            var successRate = totalJobs == 0 ? 100.0 : ((double)jobs.Count(j => j.Status == "Completed") / totalJobs) * 100.0;

            // Monthly Builds (last 30 days)
            var thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30);
            var monthlyBuilds = jobs.Count(j => j.CreatedAt >= thirtyDaysAgo);

            // Repair rate
            var jobsWithRepair = totalJobs == 0 ? 0 : await _db.GenerationJobs.CountAsync(j =>
                projectIds.Contains(j.ProjectId) &&
                (_db.RepairTickets.Any(rt => rt.BuildId == j.Id) ||
                 _db.ManufacturingTelemetries.Any(t => t.JobId == j.Id && (t.MetricName == "RepairAttempts" && t.MetricValue > 0 || t.MetricName == "RepairDuration"))));
            var repairRate = totalJobs == 0 ? 0.0 : ((double)jobsWithRepair / totalJobs) * 100.0;

            // Average Lead Time
            var leadTimes = await _db.ManufacturingTelemetries
                .Where(t => projectIds.Contains(t.ProjectId) && t.MetricName == "LeadTimeToLive")
                .Select(t => t.MetricValue)
                .ToListAsync();

            double averageLeadTime = 0.0;
            if (leadTimes.Any())
            {
                averageLeadTime = leadTimes.Average();
            }
            else
            {
                var jobDurations = jobs.Where(j => j.Status == "Completed" && j.StartedAt != null && j.CompletedAt != null)
                    .Select(j => (j.CompletedAt.Value - j.StartedAt.Value).TotalSeconds)
                    .ToList();
                averageLeadTime = jobDurations.Any() ? jobDurations.Average() : 0.0;
            }

            var usageRecords = await _db.UsageRecords
                .Where(u => projectIds.Contains(u.ProjectId))
                .ToListAsync();

            var totalLlmCost = usageRecords.Sum(u => u.EstimatedLlmCost);
            var totalComputeCost = usageRecords.Sum(u => u.EstimatedComputeCost);
            var totalStorageCost = usageRecords.Sum(u => u.EstimatedStorageCost);
            var totalCosts = totalLlmCost + totalComputeCost + totalStorageCost;

            var metrics = client.Metrics;
            if (metrics == null)
            {
                metrics = new ClientMetrics { ClientId = clientId };
                _db.ClientMetrics.Add(metrics);
            }

            metrics.TotalProjects = totalProjects;
            metrics.TotalDeployments = totalDeployments;
            metrics.AverageLeadTime = averageLeadTime;
            metrics.SuccessRate = successRate;
            metrics.RepairRate = repairRate;
            metrics.MonthlyBuilds = monthlyBuilds;
            metrics.Revenue = client.Revenue;
            metrics.GrossMargin = client.Revenue - totalCosts;
            metrics.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();

            _logger.LogInformation("Recalculated metrics for Client {ClientName} ({ClientId}): Projects={TotalProjects}, Deployments={TotalDeployments}, SuccessRate={SuccessRate}%, RepairRate={RepairRate}%, AvgLeadTime={AvgLeadTime}s, MonthlyBuilds={MonthlyBuilds}",
                client.Name, client.Id, totalProjects, totalDeployments, successRate, repairRate, averageLeadTime, monthlyBuilds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recalculating client metrics for ID {ClientId}", clientId);
        }
    }

    public async Task RecalculateAllClientsMetricsAsync()
    {
        try
        {
            var clientIds = await _db.Clients.Select(c => c.Id).ToListAsync();
            foreach (var clientId in clientIds)
            {
                await RecalculateClientMetricsAsync(clientId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recalculating metrics for all clients.");
        }
    }
}
