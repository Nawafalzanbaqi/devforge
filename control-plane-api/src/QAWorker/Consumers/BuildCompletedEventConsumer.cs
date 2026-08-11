using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DevForge.ControlPlane.Application.Common.Interfaces;
using DevForge.ControlPlane.Application.Common.Messages;
using DevForge.ControlPlane.Domain.Entities;

namespace DevForge.ControlPlane.QAWorker.Consumers;

public class BuildCompletedEventConsumer : IConsumer<BuildCompletedEvent>
{
    private readonly ILogger<BuildCompletedEventConsumer> _logger;
    private readonly IJobStateManager _stateManager;
    private readonly ISonarQubeService _sonarService;
    private readonly IPlaywrightService _playwrightService;
    private readonly IQualityScoreEngine _scoreEngine;
    private readonly IApplicationDbContext _db;

    public BuildCompletedEventConsumer(
        ILogger<BuildCompletedEventConsumer> logger,
        IJobStateManager stateManager,
        ISonarQubeService sonarService,
        IPlaywrightService playwrightService,
        IQualityScoreEngine scoreEngine,
        IApplicationDbContext db)
    {
        _logger = logger;
        _stateManager = stateManager;
        _sonarService = sonarService;
        _playwrightService = playwrightService;
        _scoreEngine = scoreEngine;
        _db = db;
    }

    public async Task Consume(ConsumeContext<BuildCompletedEvent> context)
    {
        var jobId = context.Message.JobId;
        var projectId = context.Message.ProjectId;
        var workspacePath = context.Message.WorkspacePath;

        _logger.LogInformation("QAWorker started processing build validation for Job {JobId}, Project {ProjectId}", jobId, projectId);

        // 1. Transition Job State to Testing
        await _stateManager.TransitionToTestingAsync(jobId);

        // 2. Publish QaStartedEvent
        var qaExecution = new QaExecution
        {
            ProjectId = projectId,
            BuildId = jobId,
            StartedAt = DateTimeOffset.UtcNow,
            Status = "InProgress",
            Passed = false
        };
        _db.QaExecutions.Add(qaExecution);
        await _db.SaveChangesAsync();

        await context.Publish(new QaStartedEvent
        {
            JobId = jobId,
            ProjectId = projectId,
            QaExecutionId = qaExecution.Id
        });

        try
        {
            // 3. Trigger SonarQube Analysis
            _logger.LogInformation("Triggering SonarQube analysis for Project {ProjectId}", projectId);
            var sonarGate = await _sonarService.AnalyzeAsync(workspacePath, $"project-{projectId}", $"Project_{projectId}");
            var sonarMetrics = await _sonarService.GetMetricsAsync($"project-{projectId}");
            var sonarIssues = await _sonarService.GetIssuesAsync($"project-{projectId}");

            // 4. Trigger Playwright Smoke Tests
            // Assuming local dev server or simulation runs at standard URL
            var appUrl = "http://localhost:3000"; 
            _logger.LogInformation("Triggering Playwright E2E smoke tests targeting {AppUrl}", appUrl);
            var playwrightResult = await _playwrightService.RunSmokeTestsAsync(projectId, jobId, appUrl, workspacePath);

            // 5. Calculate Score using Quality Score Engine
            var buildSuccess = true; // Build completed means compilation succeeded
            int securityScore;
            var qualityScore = _scoreEngine.CalculateScore(sonarMetrics, playwrightResult, buildSuccess, out securityScore);
            _logger.LogInformation("QA execution completed. Score: {Score}/100, Security: {SecurityScore}/100", qualityScore, securityScore);

            // 6. Gather QA Issues (Deliverable 10 - Capture failures into structured records)
            var issuesList = new List<QaIssue>();

            // Map Sonar Issues
            foreach (var sIssue in sonarIssues)
            {
                issuesList.Add(new QaIssue
                {
                    QaExecutionId = qaExecution.Id,
                    Severity = sIssue.Severity,
                    Type = "Sonar",
                    Title = sIssue.Rule,
                    Description = sIssue.Message,
                    FilePath = sIssue.Component,
                    LineNumber = sIssue.Line
                });
            }

            // Map Playwright Failures if any
            foreach (var detail in playwrightResult.Details)
            {
                if (!detail.Passed)
                {
                    issuesList.Add(new QaIssue
                    {
                        QaExecutionId = qaExecution.Id,
                        Severity = "Critical",
                        Type = "Playwright",
                        Title = $"{detail.Suite}: {detail.TestName}",
                        Description = detail.ErrorMessage ?? "Assertion failed",
                        FilePath = "tests/playwright-smoke.spec.js",
                        LineNumber = 0
                    });
                }
            }

            // Save issues to database
            if (issuesList.Count > 0)
            {
                _db.QaIssues.AddRange(issuesList);
            }

            // 7. Evaluate Quality Gate Rules:
            // Coverage >= 70%
            // Critical Vulnerabilities = 0
            // Blocker Issues = 0
            // Security Rating <= B
            // Playwright must pass
            bool passedQualityGate = sonarMetrics.Coverage >= 70m &&
                                    sonarMetrics.CriticalVulnerabilities == 0 &&
                                    sonarMetrics.BlockerIssues == 0 &&
                                    (sonarMetrics.SecurityRating == "A" || sonarMetrics.SecurityRating == "B") &&
                                    playwrightResult.Success;

            // Update QaExecution record
            qaExecution.CompletedAt = DateTimeOffset.UtcNow;
            qaExecution.Status = passedQualityGate ? "Passed" : "Failed";
            qaExecution.CoveragePercent = sonarMetrics.Coverage;
            qaExecution.QualityScore = qualityScore;
            qaExecution.SecurityScore = securityScore;
            qaExecution.BugCount = sonarIssues.Count(i => i.Severity == "Major" || i.Severity == "Critical" || i.Severity == "Blocker");
            qaExecution.WarningCount = sonarIssues.Count(i => i.Severity == "Minor" || i.Severity == "Info");
            qaExecution.Passed = passedQualityGate;

            await _db.SaveChangesAsync();

            // 8. Publish Result Events & Transition Job State
            if (passedQualityGate)
            {
                await _stateManager.TransitionToPassedQaAsync(jobId);
                await context.Publish(new QaPassedEvent
                {
                    JobId = jobId,
                    ProjectId = projectId,
                    QaExecutionId = qaExecution.Id,
                    QualityScore = qualityScore
                });
            }
            else
            {
                var errorSummary = $"Quality gate checks failed. Score: {qualityScore}/100. Blocker issues found or Playwright tests failed.";
                await _stateManager.TransitionToFailedQaAsync(jobId, errorSummary);
                await context.Publish(new QaFailedEvent
                {
                    JobId = jobId,
                    ProjectId = projectId,
                    QaExecutionId = qaExecution.Id,
                    QualityScore = qualityScore,
                    ErrorSummary = errorSummary
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception caught during QA execution for Job {JobId}", jobId);
            
            // Save exception state
            qaExecution.CompletedAt = DateTimeOffset.UtcNow;
            qaExecution.Status = "Failed";
            qaExecution.Passed = false;
            
            _db.QaIssues.Add(new QaIssue
            {
                QaExecutionId = qaExecution.Id,
                Severity = "Blocker",
                Type = "Compiler",
                Title = "QA Runner Exception",
                Description = ex.Message,
                FilePath = "QAWorker",
                LineNumber = 0
            });
            await _db.SaveChangesAsync();

            await _stateManager.TransitionToFailedQaAsync(jobId, ex.Message);
            await context.Publish(new QaFailedEvent
            {
                JobId = jobId,
                ProjectId = projectId,
                QaExecutionId = qaExecution.Id,
                QualityScore = 0,
                ErrorSummary = ex.Message
            });
        }
    }
}
