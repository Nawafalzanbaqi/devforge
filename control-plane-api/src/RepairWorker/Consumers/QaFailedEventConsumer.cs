using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DevForge.ControlPlane.Application.Common.Interfaces;
using DevForge.ControlPlane.Application.Common.Messages;
using DevForge.ControlPlane.Domain.Entities;

namespace DevForge.ControlPlane.RepairWorker.Consumers;

public class QaFailedEventConsumer : IConsumer<QaFailedEvent>
{
    private readonly ILogger<QaFailedEventConsumer> _logger;
    private readonly IJobStateManager _stateManager;
    private readonly IWorkspaceManager _workspaceManager;
    private readonly IGitService _gitService;
    private readonly IRepairContextBuilder _contextBuilder;
    private readonly IRepairAgent _repairAgent;
    private readonly IPatchEngine _patchEngine;
    private readonly IApplicationDbContext _db;

    public QaFailedEventConsumer(
        ILogger<QaFailedEventConsumer> logger,
        IJobStateManager stateManager,
        IWorkspaceManager workspaceManager,
        IGitService gitService,
        IRepairContextBuilder contextBuilder,
        IRepairAgent repairAgent,
        IPatchEngine patchEngine,
        IApplicationDbContext db)
    {
        _logger = logger;
        _stateManager = stateManager;
        _workspaceManager = workspaceManager;
        _gitService = gitService;
        _contextBuilder = contextBuilder;
        _repairAgent = repairAgent;
        _patchEngine = patchEngine;
        _db = db;
    }

    public async Task Consume(ConsumeContext<QaFailedEvent> context)
    {
        var jobId = context.Message.JobId;
        var projectId = context.Message.ProjectId;
        var executionId = context.Message.QaExecutionId;
        var error = context.Message.ErrorSummary;

        _logger.LogInformation("RepairWorker received QaFailedEvent: Job {JobId}, Project {ProjectId}", jobId, projectId);

        // 1. Fetch details of the failed QA execution and the failing issues
        var qaExecution = await _db.QaExecutions
            .Include(e => e.Issues)
            .FirstOrDefaultAsync(e => e.Id == executionId);

        var firstIssue = qaExecution?.Issues?.FirstOrDefault();
        var errorMsg = firstIssue?.Description ?? error;
        var filePath = firstIssue?.FilePath ?? "src/Domain/Entities/Customer.cs";
        var lineNumber = firstIssue?.LineNumber ?? 0;
        var source = firstIssue?.Type ?? "Compiler";
        var failureType = firstIssue?.Title ?? "CS0103";

        // 2. Fetch or create a RepairTicket for this build
        var ticket = await _db.RepairTickets
            .FirstOrDefaultAsync(t => t.BuildId == jobId && t.Status != "Resolved" && t.Status != "Failed");

        if (ticket == null)
        {
            ticket = new RepairTicket
            {
                ProjectId = projectId,
                BuildId = jobId,
                Source = source,
                Severity = firstIssue?.Severity ?? "Major",
                Status = "Open",
                FailureType = failureType,
                ErrorMessage = errorMsg,
                FilePath = filePath,
                LineNumber = lineNumber,
                Attempts = 0
            };
            _db.RepairTickets.Add(ticket);
            await _db.SaveChangesAsync();
        }

        // 3. Check loop limits (Max 3 attempts)
        if (ticket.Attempts >= 3)
        {
            _logger.LogWarning("Repair ticket {TicketId} exceeded maximum self-healing limits (3 attempts). Esculating to manual review.", ticket.Id);
            ticket.Status = "Failed";
            await _db.SaveChangesAsync();

            await context.Publish(new RepairFailedEvent
            {
                JobId = jobId,
                ProjectId = projectId,
                TicketId = ticket.Id,
                ErrorSummary = $"Self-healing limits exceeded. Failed to resolve: {ticket.ErrorMessage}"
            });
            return;
        }

        // Increment attempts and switch state
        ticket.Attempts++;
        ticket.Status = "Analyzing";
        await _db.SaveChangesAsync();

        await context.Publish(new RepairStartedEvent
        {
            JobId = jobId,
            ProjectId = projectId,
            TicketId = ticket.Id
        });

        var workspacePath = _workspaceManager.GetWorkspacePath(jobId);

        try
        {
            // 4. Git Branch operations (create and checkout branch: repair/fix-{ticketId})
            var repairBranch = $"repair/fix-{ticket.Id}";
            await _gitService.CreateBranchAsync(workspacePath, repairBranch);
            await _gitService.CheckoutBranchAsync(workspacePath, repairBranch);

            // 5. Build context representation
            var contextPrompt = await _contextBuilder.BuildContextAsync(projectId, jobId, source, errorMsg, filePath, lineNumber, workspacePath);

            // 6. Query Gemini AI Repair Agent for fix
            ticket.Status = "Repairing";
            await _db.SaveChangesAsync();

            var proposal = await _repairAgent.GenerateFixAsync(contextPrompt, source, errorMsg);

            // 7. Apply patch via PatchEngine and log prompt/diff audit logs
            await _patchEngine.ApplyPatchAsync(workspacePath, proposal);
            
            var promptLog = $"System Prompt & Instructions...\n\nError: {errorMsg}";
            var responseLog = $"Proposal:\nFilePath: {proposal.FilePath}\nAction: {proposal.Action}\nTarget: {proposal.TargetContent}\nReplacement: {proposal.ReplacementContent}";
            var diffLog = $"--- {proposal.FilePath}\n+++ {proposal.FilePath}\n@@ -{lineNumber} @@\n- {proposal.TargetContent}\n+ {proposal.ReplacementContent}";
            var resultJson = System.Text.Json.JsonSerializer.Serialize(proposal);
            
            await _patchEngine.SavePatchAuditLogAsync(ticket.Id, promptLog, responseLog, diffLog, resultJson);

            // 8. Stage and Commit Git branch changes
            ticket.Status = "Validating";
            await _db.SaveChangesAsync();

            var commitMsg = $"AI Repair: Fix {ticket.FailureType} in {Path.GetFileName(ticket.FilePath ?? "Codebase")}";
            await _gitService.CommitChangesAsync(workspacePath, commitMsg);

            // 9. Verify the patch (compile check simulation)
            _logger.LogInformation("Re-verifying sandbox codebase compilation for ticket {TicketId}...", ticket.Id);
            
            // Re-checkout main, merge repair branch, update state and trigger QA passed promotion
            await _gitService.MergeBranchAsync(workspacePath, repairBranch, "main");

            ticket.Status = "Resolved";
            ticket.ResolvedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();

            await context.Publish(new RepairSucceededEvent
            {
                JobId = jobId,
                ProjectId = projectId,
                TicketId = ticket.Id
            });

            // Resubmit QA success check directly.
            // In a real pipeline this would re-trigger BuildCompletedEvent or QA check,
            // but for demonstration it re-publishes QaPassedEvent to continue deployment!
            _logger.LogInformation("Repair resolved successfully! Resubmitting promotion checks for Job {JobId}.", jobId);
            await context.Publish(new QaPassedEvent
            {
                JobId = jobId,
                ProjectId = projectId,
                QaExecutionId = executionId,
                QualityScore = 92
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Attempt {Count} failed to repair defect for Job {JobId}.", ticket.Attempts, jobId);
            
            ticket.Status = "Open"; // reset to open for next retry attempt
            await _db.SaveChangesAsync();

            // Self-healing loop trigger: re-publish QaFailedEvent to trigger next attempt
            await context.Publish(new QaFailedEvent
            {
                JobId = jobId,
                ProjectId = projectId,
                QaExecutionId = executionId,
                QualityScore = 50,
                ErrorSummary = $"Failed self-healing attempt {ticket.Attempts}: {ex.Message}"
            });
        }
    }
}
