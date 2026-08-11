using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DevForge.ControlPlane.Domain.Entities;

namespace DevForge.ControlPlane.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Organization> Organizations { get; }
    DbSet<OrganizationMember> OrganizationMembers { get; }
    DbSet<Project> Projects { get; }
    DbSet<Schema> Schemas { get; }
    DbSet<Entity> Entities { get; }
    DbSet<DevForge.ControlPlane.Domain.Entities.Attribute> Attributes { get; }
    DbSet<GenerationJob> GenerationJobs { get; }
    DbSet<PromptTemplate> PromptTemplates { get; }
    DbSet<BuildResult> BuildResults { get; }
    DbSet<Deployment> Deployments { get; }
    DbSet<QaResult> QaResults { get; }
    DbSet<AiUsage> AiUsages { get; }
    DbSet<Billing> Billings { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<ApiKey> ApiKeys { get; }
    DbSet<Client> Clients { get; }
    DbSet<BillingAccount> BillingAccounts { get; }
    DbSet<ClientMetrics> ClientMetrics { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<PlanFeature> PlanFeatures { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<TemplateCatalog> TemplateCatalogs { get; }
    DbSet<UsageQuota> UsageQuotas { get; }
    DbSet<UsageRecord> UsageRecords { get; }
    DbSet<AgentWorkflow> AgentWorkflows { get; }
    DbSet<QaExecution> QaExecutions { get; }
    DbSet<QaIssue> QaIssues { get; }
    DbSet<RepairTicket> RepairTickets { get; }
    DbSet<ManufacturingTelemetry> ManufacturingTelemetries { get; }
    DbSet<Alert> Alerts { get; }
    DbSet<OptimizationRecommendation> OptimizationRecommendations { get; }
    DbSet<OptimizationOutcome> OptimizationOutcomes { get; }
    DbSet<OptimizationKnowledge> OptimizationKnowledges { get; }
    DbSet<PatchProposal> PatchProposals { get; }
    DbSet<TemplateKnowledge> TemplateKnowledges { get; }
    DbSet<TemplateImprovementProposal> TemplateImprovementProposals { get; }
    DbSet<TemplateVersionHistory> TemplateVersionHistories { get; }
    DbSet<CandidateTemplateRelease> CandidateTemplateReleases { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
