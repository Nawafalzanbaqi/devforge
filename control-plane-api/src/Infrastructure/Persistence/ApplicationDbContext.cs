using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DevForge.ControlPlane.Application.Common.Interfaces;
using DevForge.ControlPlane.Domain.Entities;

namespace DevForge.ControlPlane.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationMember> OrganizationMembers => Set<OrganizationMember>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Schema> Schemas => Set<Schema>();
    public DbSet<Entity> Entities => Set<Entity>();
    public DbSet<DevForge.ControlPlane.Domain.Entities.Attribute> Attributes => Set<DevForge.ControlPlane.Domain.Entities.Attribute>();
    public DbSet<GenerationJob> GenerationJobs => Set<GenerationJob>();
    public DbSet<PromptTemplate> PromptTemplates => Set<PromptTemplate>();
    public DbSet<BuildResult> BuildResults => Set<BuildResult>();
    public DbSet<Deployment> Deployments => Set<Deployment>();
    public DbSet<QaResult> QaResults => Set<QaResult>();
    public DbSet<AiUsage> AiUsages => Set<AiUsage>();
    public DbSet<Billing> Billings => Set<Billing>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<BillingAccount> BillingAccounts => Set<BillingAccount>();
    public DbSet<ClientMetrics> ClientMetrics => Set<ClientMetrics>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<PlanFeature> PlanFeatures => Set<PlanFeature>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<TemplateCatalog> TemplateCatalogs => Set<TemplateCatalog>();
    public DbSet<UsageQuota> UsageQuotas => Set<UsageQuota>();
    public DbSet<UsageRecord> UsageRecords => Set<UsageRecord>();
    public DbSet<AgentWorkflow> AgentWorkflows => Set<AgentWorkflow>();
    public DbSet<QaExecution> QaExecutions => Set<QaExecution>();
    public DbSet<QaIssue> QaIssues => Set<QaIssue>();
    public DbSet<RepairTicket> RepairTickets => Set<RepairTicket>();
    public DbSet<ManufacturingTelemetry> ManufacturingTelemetries => Set<ManufacturingTelemetry>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<OptimizationRecommendation> OptimizationRecommendations => Set<OptimizationRecommendation>();
    public DbSet<OptimizationOutcome> OptimizationOutcomes => Set<OptimizationOutcome>();
    public DbSet<OptimizationKnowledge> OptimizationKnowledges => Set<OptimizationKnowledge>();
    public DbSet<PatchProposal> PatchProposals => Set<PatchProposal>();
    public DbSet<TemplateKnowledge> TemplateKnowledges => Set<TemplateKnowledge>();
    public DbSet<TemplateImprovementProposal> TemplateImprovementProposals => Set<TemplateImprovementProposal>();
    public DbSet<TemplateVersionHistory> TemplateVersionHistories => Set<TemplateVersionHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Composite Key for OrganizationMember
        modelBuilder.Entity<OrganizationMember>()
            .HasKey(om => new { om.OrganizationId, om.UserId });

        modelBuilder.Entity<Organization>()
            .HasMany(o => o.Clients)
            .WithOne(c => c.Organization)
            .HasForeignKey(c => c.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Organization>()
            .HasOne<BillingAccount>()
            .WithOne(b => b.Organization)
            .HasForeignKey<BillingAccount>(b => b.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BillingAccount>()
            .HasMany(b => b.Invoices)
            .WithOne(i => i.BillingAccount)
            .HasForeignKey(i => i.BillingAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BillingAccount>()
            .HasOne<Subscription>()
            .WithOne(s => s.BillingAccount)
            .HasForeignKey<Subscription>(s => s.BillingAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BillingAccount>()
            .HasMany<UsageQuota>()
            .WithOne(q => q.BillingAccount)
            .HasForeignKey(q => q.BillingAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Client>()
            .HasMany(c => c.Projects)
            .WithOne(p => p.Client)
            .HasForeignKey(p => p.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Client>()
            .HasOne(c => c.Metrics)
            .WithOne(m => m.Client)
            .HasForeignKey<ClientMetrics>(m => m.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Project>()
            .HasMany<UsageRecord>()
            .WithOne(u => u.Project)
            .HasForeignKey(u => u.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GenerationJob>()
            .HasMany<UsageRecord>()
            .WithOne(u => u.GenerationJob)
            .HasForeignKey(u => u.JobId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Organization>()
            .HasMany(o => o.Projects)
            .WithOne(p => p.Organization)
            .HasForeignKey(p => p.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GenerationJob>()
            .HasOne(j => j.Template)
            .WithMany()
            .HasForeignKey(j => j.TemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Project>()
            .HasMany(p => p.Schemas)
            .WithOne(s => s.Project)
            .HasForeignKey(s => s.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Schema>()
            .HasMany(s => s.Entities)
            .WithOne(e => e.Schema)
            .HasForeignKey(e => e.SchemaId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Entity>()
            .HasMany(e => e.Attributes)
            .WithOne(a => a.Entity)
            .HasForeignKey(a => a.EntityId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GenerationJob>()
            .HasMany(j => j.BuildResults)
            .WithOne(r => r.GenerationJob)
            .HasForeignKey(r => r.GenerationJobId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GenerationJob>()
            .HasMany(j => j.QaResults)
            .WithOne(q => q.GenerationJob)
            .HasForeignKey(q => q.GenerationJobId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GenerationJob>()
            .HasMany(j => j.AgentWorkflows)
            .WithOne(w => w.GenerationJob)
            .HasForeignKey(w => w.GenerationJobId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<QaExecution>()
            .HasMany(e => e.Issues)
            .WithOne(i => i.QaExecution)
            .HasForeignKey(i => i.QaExecutionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Project>()
            .HasMany(p => p.RepairTickets)
            .WithOne(t => t.Project)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ManufacturingTelemetry>()
            .HasOne(t => t.Project)
            .WithMany()
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ManufacturingTelemetry>()
            .HasOne(t => t.GenerationJob)
            .WithMany()
            .HasForeignKey(t => t.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Alert>()
            .HasOne(a => a.Project)
            .WithMany()
            .HasForeignKey(a => a.ProjectId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OptimizationRecommendation>()
            .HasOne(r => r.Project)
            .WithMany()
            .HasForeignKey(r => r.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OptimizationRecommendation>()
            .HasOne(r => r.Outcome)
            .WithOne(o => o.Recommendation)
            .HasForeignKey<OptimizationOutcome>(o => o.RecommendationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OptimizationRecommendation>()
            .HasOne(r => r.PatchProposal)
            .WithOne(p => p.Recommendation)
            .HasForeignKey<PatchProposal>(p => p.RecommendationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TemplateKnowledge>()
            .HasOne(tk => tk.Template)
            .WithOne()
            .HasForeignKey<TemplateKnowledge>(tk => tk.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TemplateImprovementProposal>()
            .HasOne(tip => tip.Template)
            .WithMany()
            .HasForeignKey(tip => tip.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TemplateVersionHistory>()
            .HasOne(tvh => tvh.Template)
            .WithMany()
            .HasForeignKey(tvh => tvh.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TemplateVersionHistory>()
            .HasOne(tvh => tvh.ImprovementProposal)
            .WithMany()
            .HasForeignKey(tvh => tvh.ImprovementProposalId)
            .OnDelete(DeleteBehavior.SetNull);
            
        // Explicitly load configuration from assembly scans if needed
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
