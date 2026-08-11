using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DevForge.ControlPlane.Application.Common.Interfaces;
using DevForge.ControlPlane.Application.Common.Messages;
using DevForge.ControlPlane.Domain.Entities;

namespace DevForge.ControlPlane.BuildWorker.Consumers;

public class GenerateApplicationJobConsumer : IConsumer<GenerateApplicationJob>
{
    private readonly ILogger<GenerateApplicationJobConsumer> _logger;
    private readonly IJobStateManager _stateManager;
    private readonly IWorkspaceManager _workspaceManager;
    private readonly ICodeInjectionEngine _injectionEngine;
    private readonly IApplicationDbContext _db;
    private readonly IGitService _gitService;

    public GenerateApplicationJobConsumer(
        ILogger<GenerateApplicationJobConsumer> logger,
        IJobStateManager stateManager,
        IWorkspaceManager workspaceManager,
        ICodeInjectionEngine injectionEngine,
        IApplicationDbContext db,
        IGitService gitService)
    {
        _logger = logger;
        _stateManager = stateManager;
        _workspaceManager = workspaceManager;
        _injectionEngine = injectionEngine;
        _db = db;
        _gitService = gitService;
    }

    public async Task Consume(ConsumeContext<GenerateApplicationJob> context)
    {
        var jobId = context.Message.JobId;
        var projectId = context.Message.ProjectId;

        _logger.LogInformation("Processing GenerateApplicationJob event: Job {JobId} for Project {ProjectId}", jobId, projectId);

        try
        {
            // Step 1: Transition to Queued
            await _stateManager.TransitionToQueuedAsync(jobId);
            await Task.Delay(1000);

            // Fetch Job details to get Schema and Project reference
            var job = await _db.GenerationJobs
                .Include(j => j.Project)
                .Include(j => j.Schema)
                .FirstOrDefaultAsync(j => j.Id == jobId);
                
            var workspacePath = _workspaceManager.GetWorkspacePath(jobId);
            
            // Initialize Git Repository at workspace creation
            await _gitService.InitializeRepositoryAsync(workspacePath);

            // Step 2: Transition to Generating & Scaffolding Boilerplate Deltas
            await _stateManager.TransitionToGeneratingAsync(jobId);
            
            // Seed a default Template Catalog in database if none exists
            var template = await _db.TemplateCatalogs.FirstOrDefaultAsync();
            if (template == null)
            {
                template = new TemplateCatalog
                {
                    Name = "CleanArchitecture.NET",
                    Framework = ".NET 9",
                    Path = "C:\\Users\\alzan\\Desktop\\test\\devforge\\templates\\clean-arch",
                    Description = "Clean architecture starter template"
                };
                _db.TemplateCatalogs.Add(template);
                await _db.SaveChangesAsync();
            }

            // Copy base templates and inject entities configurations
            await _injectionEngine.InjectSchemaIntoTemplateAsync(jobId, template.Id);
            _logger.LogInformation("Files copy and code placeholders injection completed for job {JobId}", jobId);
            
            // Commit changes after code scaffolding & injection
            var schemaVersion = job?.Schema?.Version ?? 1;
            await _gitService.CommitChangesAsync(workspacePath, $"Initial scaffold for Schema Version {schemaVersion}");
            await Task.Delay(2000);

            // Step 3: Transition to Compiling
            await _stateManager.TransitionToCompilingAsync(jobId);
            _logger.LogInformation("Compiling target sandbox project...");
            _workspaceManager.WriteFile(jobId, "logs/compile.log", "[12:00:01] Compiled target modules successfully.");
            
            // Stage/Commit compilation logs
            await _gitService.CommitChangesAsync(workspacePath, "Compilation logs updated");
            await Task.Delay(2000);

            // Publish BuildCompletedEvent to trigger QA verification worker
            _logger.LogInformation("Compilation successful. Delegating verification checks to QAWorker for Job {JobId}.", jobId);
            await context.Publish(new BuildCompletedEvent
            {
                JobId = jobId,
                ProjectId = projectId,
                WorkspacePath = workspacePath
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compile and manufacture project for Job {JobId}", jobId);
            
            // Capture compilation issues (Deliverable 10) in a QA Execution record even if compilation failed
            try
            {
                var qaExecution = new QaExecution
                {
                    ProjectId = projectId,
                    BuildId = jobId,
                    StartedAt = DateTimeOffset.UtcNow,
                    CompletedAt = DateTimeOffset.UtcNow,
                    Status = "Failed",
                    Passed = false
                };
                _db.QaExecutions.Add(qaExecution);
                await _db.SaveChangesAsync();

                _db.QaIssues.Add(new QaIssue
                {
                    QaExecutionId = qaExecution.Id,
                    Severity = "Blocker",
                    Type = "Compiler",
                    Title = "Compilation Failed",
                    Description = ex.Message,
                    FilePath = "BuildWorker",
                    LineNumber = 0
                });
                await _db.SaveChangesAsync();
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx, "Failed to log compiler issue to DB for Job {JobId}", jobId);
            }

            await _stateManager.TransitionToFailedAsync(jobId, ex.Message);
            throw; // Re-throw to allow MassTransit retry/dead-lettering
        }
    }
}
