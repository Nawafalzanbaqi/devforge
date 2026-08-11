using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DevForge.ControlPlane.Application.Common.Interfaces;
using DevForge.ControlPlane.Domain.Entities;

namespace DevForge.ControlPlane.Infrastructure.Services;

public class TemplateScoringService : ITemplateScoringService
{
    private readonly IApplicationDbContext _db;
    private readonly ILogger<TemplateScoringService> _logger;

    public TemplateScoringService(IApplicationDbContext db, ILogger<TemplateScoringService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task RecalculateTemplateStatsAsync(Guid templateId)
    {
        try
        {
            var template = await _db.TemplateCatalogs.FindAsync(templateId);
            if (template == null)
            {
                _logger.LogWarning("Template with ID {TemplateId} not found for stats recalculation.", templateId);
                return;
            }

            var totalJobs = await _db.GenerationJobs.CountAsync(j => j.TemplateId == templateId);
            if (totalJobs == 0)
            {
                // Default stats if no jobs run yet
                template.SuccessRate = 100.0;
                template.RepairRate = 0.0;
                template.AverageLeadTime = 0.0;
                template.TemplateUsageCount = 0;
            }
            else
            {
                template.TemplateUsageCount = totalJobs;
                // 1. Success Rate
                var successfulJobs = await _db.GenerationJobs.CountAsync(j => j.TemplateId == templateId && j.Status == "Completed");
                template.SuccessRate = ((double)successfulJobs / totalJobs) * 100.0;

                // 2. Repair Rate
                var jobsWithRepair = await _db.GenerationJobs.CountAsync(j => 
                    j.TemplateId == templateId && 
                    (_db.RepairTickets.Any(rt => rt.BuildId == j.Id) || 
                     _db.ManufacturingTelemetries.Any(t => t.JobId == j.Id && (t.MetricName == "RepairAttempts" && t.MetricValue > 0 || t.MetricName == "RepairDuration"))));
                template.RepairRate = ((double)jobsWithRepair / totalJobs) * 100.0;

                // 3. Average Lead Time
                var leadTimes = await _db.ManufacturingTelemetries
                    .Where(t => t.GenerationJob.TemplateId == templateId && t.MetricName == "LeadTimeToLive")
                    .Select(t => t.MetricValue)
                    .ToListAsync();

                if (leadTimes.Any())
                {
                    template.AverageLeadTime = leadTimes.Average();
                }
                else
                {
                    // Fallback to job completion duration
                    var jobDurations = await _db.GenerationJobs
                        .Where(j => j.TemplateId == templateId && j.Status == "Completed" && j.StartedAt != null && j.CompletedAt != null)
                        .Select(j => (j.CompletedAt.Value - j.StartedAt.Value).TotalSeconds)
                        .ToListAsync();
                    
                    template.AverageLeadTime = jobDurations.Any() ? jobDurations.Average() : 0.0;
                }
            }

            template.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Recalculated stats for Template {TemplateName} ({TemplateId}): SuccessRate={SuccessRate}%, RepairRate={RepairRate}%, AvgLeadTime={AvgLeadTime}s, UsageCount={UsageCount}",
                template.Name, template.Id, template.SuccessRate, template.RepairRate, template.AverageLeadTime, template.TemplateUsageCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recalculating template stats for ID {TemplateId}", templateId);
        }
    }

    public async Task RecalculateAllTemplatesStatsAsync()
    {
        try
        {
            var templateIds = await _db.TemplateCatalogs.Select(t => t.Id).ToListAsync();
            foreach (var templateId in templateIds)
            {
                await RecalculateTemplateStatsAsync(templateId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recalculating stats for all templates.");
        }
    }
}
