using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DevForge.ControlPlane.Application.Common.Interfaces;
using DevForge.ControlPlane.Domain.Entities;

namespace DevForge.ControlPlane.OptimizationWorker;

public class OptimizationWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OptimizationWorker> _logger;

    public OptimizationWorker(IServiceProvider serviceProvider, ILogger<OptimizationWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OptimizationWorker background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

                await RunAgentsAsync(db);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during optimization analysis loop.");
            }

            // Sleep 10 seconds (optimized for acceptance tests loop cycles)
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private async Task<double> CalculateEvidenceBasedScoreAsync(IApplicationDbContext db, string category, double baseSeverity)
    {
        var knowledge = await db.OptimizationKnowledges
            .FirstOrDefaultAsync(k => k.RecommendationType == category);

        if (knowledge == null || knowledge.Executions == 0)
        {
            return baseSeverity;
        }

        return Math.Round(baseSeverity * knowledge.ConfidenceScore * (1.0 + knowledge.AverageImprovement / 100.0), 2);
    }

    public async Task RunAgentsAsync(IApplicationDbContext db)
    {
        _logger.LogInformation("Running optimization agents...");

        // 1. Performance Agent: CPU metrics / SLA durations
        var slowTelemetry = await db.ManufacturingTelemetries
            .Where(t => (t.MetricName == "Generation" || t.MetricName == "GenerationDuration") && t.MetricValue > 100.0)
            .ToListAsync();

        var highCpuTelemetry = await db.ManufacturingTelemetries
            .Where(t => t.MetricName == "devforge_container_cpu_usage" && t.MetricValue > 80.0)
            .ToListAsync();

        var performanceIssues = slowTelemetry.Concat(highCpuTelemetry).ToList();
        foreach (var issue in performanceIssues)
        {
            var title = "Database Query Cache Opportunity";
            var existing = await db.OptimizationRecommendations
                .FirstOrDefaultAsync(r => r.ProjectId == issue.ProjectId && r.Category == "Performance" && r.Title == title && (r.Status == "Pending" || r.Status == "Approved"));

            var existingLocal = db.OptimizationRecommendations.Local
                .Any(r => r.ProjectId == issue.ProjectId && r.Category == "Performance" && r.Title == title && (r.Status == "Pending" || r.Status == "Approved"));

            if (existing == null && !existingLocal)
            {
                var baseScore = issue.MetricName.Contains("cpu") ? issue.MetricValue * 1.5 : issue.MetricValue * 2.0;
                var finalScore = await CalculateEvidenceBasedScoreAsync(db, "Performance", baseScore);

                var rec = new OptimizationRecommendation
                {
                    ProjectId = issue.ProjectId,
                    Category = "Performance",
                    Title = title,
                    Description = $"Slow endpoint execution or high CPU utilization of {issue.MetricValue} detected during {issue.MetricName}.",
                    RecommendedAction = "Implement standard Caching on Patients query handlers using ICacheService to bypass database load.",
                    EstimatedSavingsValue = 15.0,
                    SavingsUnit = "Seconds",
                    Status = "Pending",
                    Score = finalScore,
                    RiskLevel = "Low"
                };

                var patch = new PatchProposal
                {
                    Recommendation = rec,
                    PatchType = "RedisCache",
                    Diff = "@@ -12,4 +12,12 @@\n+ // Inject ICacheService and wrap patient querying with cache bypass",
                    FilesChanged = "src/Application/Patients/Queries/GetPatientsQuery.cs",
                    RiskLevel = "Low"
                };
                rec.PatchProposal = patch;

                db.OptimizationRecommendations.Add(rec);
                _logger.LogInformation("Performance Agent generated recommendation for project {ProjectId}.", issue.ProjectId);
            }
        }

        // 2. Cost Agent: High token counts / Expense overruns
        var highCostRecords = await db.UsageRecords
            .Where(u => u.GeminiTokens > 10000 || u.EstimatedTotalCost > 0.50)
            .OrderByDescending(u => u.GeminiTokens)
            .ToListAsync();

        foreach (var record in highCostRecords)
        {
            var title = "Prompt Token Compression";
            var existing = await db.OptimizationRecommendations
                .FirstOrDefaultAsync(r => r.ProjectId == record.ProjectId && r.Category == "Cost" && r.Title == title && (r.Status == "Pending" || r.Status == "Approved"));

            var existingLocal = db.OptimizationRecommendations.Local
                .Any(r => r.ProjectId == record.ProjectId && r.Category == "Cost" && r.Title == title && (r.Status == "Pending" || r.Status == "Approved"));

            if (existing == null && !existingLocal)
            {
                var baseScore = record.GeminiTokens / 100.0;
                var finalScore = await CalculateEvidenceBasedScoreAsync(db, "Cost", baseScore);

                var rec = new OptimizationRecommendation
                {
                    ProjectId = record.ProjectId,
                    Category = "Cost",
                    Title = title,
                    Description = $"Generation job consumed excessive tokens ({record.GeminiTokens}) costing estimated ${record.EstimatedTotalCost}.",
                    RecommendedAction = "Prune redundant schema definition parameters and compress System Instructions context.",
                    EstimatedSavingsValue = 2500.0,
                    SavingsUnit = "Tokens",
                    Status = "Pending",
                    Score = finalScore,
                    RiskLevel = "Low"
                };

                var patch = new PatchProposal
                {
                    Recommendation = rec,
                    PatchType = "PromptCompression",
                    Diff = "@@ -25,4 +25,8 @@\n- // Verbose prompts\n+ // Compress templates and prune unused fields to reduce prompt tokens",
                    FilesChanged = "src/Infrastructure/Services/AiService.cs",
                    RiskLevel = "Low"
                };
                rec.PatchProposal = patch;

                db.OptimizationRecommendations.Add(rec);
                _logger.LogInformation("Cost Agent generated recommendation for project {ProjectId}.", record.ProjectId);
            }
        }

        // 3. Reliability Agent: Compile repair iterations / Container crashes
        var multiRepairTickets = await db.RepairTickets
            .Where(t => t.Attempts > 1 || t.Status == "Failed")
            .ToListAsync();

        var failedDeployments = await db.Deployments
            .Where(d => d.Status == "Failed" || d.HealthScore < 90.0)
            .ToListAsync();

        var reliabilityIssues = multiRepairTickets.Select(t => new { t.ProjectId, Source = "Repair Attempts" })
            .Concat(failedDeployments.Select(d => new { d.ProjectId, Source = "Deployment Outage" }))
            .Distinct()
            .ToList();

        foreach (var issue in reliabilityIssues)
        {
            var title = "Template Compilation Patches";
            var existing = await db.OptimizationRecommendations
                .FirstOrDefaultAsync(r => r.ProjectId == issue.ProjectId && r.Category == "Reliability" && r.Title == title && (r.Status == "Pending" || r.Status == "Approved"));

            var existingLocal = db.OptimizationRecommendations.Local
                .Any(r => r.ProjectId == issue.ProjectId && r.Category == "Reliability" && r.Title == title && (r.Status == "Pending" || r.Status == "Approved"));

            if (existing == null && !existingLocal)
            {
                var baseScore = issue.Source == "Repair Attempts" ? 50.0 : 100.0;
                var finalScore = await CalculateEvidenceBasedScoreAsync(db, "Reliability", baseScore);

                var rec = new OptimizationRecommendation
                {
                    ProjectId = issue.ProjectId,
                    Category = "Reliability",
                    Title = title,
                    Description = $"Application suffered compilation errors or deployment health outages (Trigger source: {issue.Source}).",
                    RecommendedAction = "Apply explicit namespace declarations inside generated templates to reduce CS0103 compiler alerts.",
                    EstimatedSavingsValue = 1.0,
                    SavingsUnit = "Repairs",
                    Status = "Pending",
                    Score = finalScore,
                    RiskLevel = "Medium"
                };

                var patch = new PatchProposal
                {
                    Recommendation = rec,
                    PatchType = "NamespaceFix",
                    Diff = "@@ -5,2 +5,5 @@\n- using Entities;\n+ using DevForge.ControlPlane.Domain.Entities;",
                    FilesChanged = "src/Domain/Entities/Patient.cs",
                    RiskLevel = "Medium"
                };
                rec.PatchProposal = patch;

                db.OptimizationRecommendations.Add(rec);
                _logger.LogInformation("Reliability Agent generated recommendation for project {ProjectId}.", issue.ProjectId);
            }
        }

        await db.SaveChangesAsync();
    }
}
