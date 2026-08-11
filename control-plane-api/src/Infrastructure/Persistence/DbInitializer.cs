using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DevForge.ControlPlane.Application.Common.Interfaces;
using DevForge.ControlPlane.Domain.Entities;

namespace DevForge.ControlPlane.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedSystemProjectAsync(IServiceProvider serviceProvider)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            var systemProjectId = new Guid("11111111-1111-1111-1111-111111111111");
            var systemJobId = new Guid("22222222-2222-2222-2222-222222222222");

            // Ensure database schema is created
            if (db is DbContext dbContext)
            {
                await dbContext.Database.EnsureCreatedAsync();
            }

            var project = await db.Projects.FindAsync(systemProjectId);
            if (project == null)
            {
                var org = await db.Organizations.FirstOrDefaultAsync();
                if (org == null)
                {
                    org = new Organization { Name = "DevForge System Organization" };
                    db.Organizations.Add(org);
                    await db.SaveChangesAsync();
                }

                var billingAccount = await db.BillingAccounts.FirstOrDefaultAsync(b => b.OrganizationId == org.Id);
                if (billingAccount == null)
                {
                    billingAccount = new BillingAccount
                    {
                        OrganizationId = org.Id,
                        PlanTier = "Agency",
                        CurrentBalance = 150.0,
                        Status = "Active"
                    };
                    db.BillingAccounts.Add(billingAccount);
                    await db.SaveChangesAsync();

                    var seedInvoice = new Invoice
                    {
                        BillingAccountId = billingAccount.Id,
                        IssueDate = DateTimeOffset.UtcNow.AddDays(-5),
                        DueDate = DateTimeOffset.UtcNow.AddDays(10),
                        Amount = 45.50,
                        Status = "Paid",
                        UsedTokens = 150000,
                        UsedStorage = 512.0,
                        UsedDeployments = 2,
                        UsedBuildMinutes = 20,
                        UsedRepairAttempts = 1
                    };
                    db.Invoices.Add(seedInvoice);
                    await db.SaveChangesAsync();

                    // Seed plan features
                    var planFeaturesCount = await db.PlanFeatures.CountAsync();
                    if (planFeaturesCount == 0)
                    {
                        db.PlanFeatures.AddRange(
                            new PlanFeature { PlanName = "Free", FeatureKey = "CanCreateProject", IsEnabled = true },
                            new PlanFeature { PlanName = "Free", FeatureKey = "CanGenerate", IsEnabled = true },
                            new PlanFeature { PlanName = "Free", FeatureKey = "CanDeploy", IsEnabled = true },
                            new PlanFeature { PlanName = "Free", FeatureKey = "CanRepair", IsEnabled = false },
                            new PlanFeature { PlanName = "Free", FeatureKey = "MultiClient", IsEnabled = false },

                            new PlanFeature { PlanName = "Agency", FeatureKey = "CanCreateProject", IsEnabled = true },
                            new PlanFeature { PlanName = "Agency", FeatureKey = "CanGenerate", IsEnabled = true },
                            new PlanFeature { PlanName = "Agency", FeatureKey = "CanDeploy", IsEnabled = true },
                            new PlanFeature { PlanName = "Agency", FeatureKey = "CanRepair", IsEnabled = true },
                            new PlanFeature { PlanName = "Agency", FeatureKey = "MultiClient", IsEnabled = true }
                        );
                        await db.SaveChangesAsync();
                    }

                    // Seed Subscription
                    var subscription = new Subscription
                    {
                        BillingAccountId = billingAccount.Id,
                        PlanName = "Agency",
                        Status = "Active",
                        StartedAt = DateTimeOffset.UtcNow,
                        EndsAt = DateTimeOffset.UtcNow.AddMonths(1)
                    };
                    db.Subscriptions.Add(subscription);

                    // Seed Usage Quotas
                    db.UsageQuotas.AddRange(
                        new UsageQuota { BillingAccountId = billingAccount.Id, QuotaType = "Tokens", LimitValue = 1000000.0, UsedValue = 150000.0 },
                        new UsageQuota { BillingAccountId = billingAccount.Id, QuotaType = "Storage", LimitValue = 5120.0, UsedValue = 512.0 },
                        new UsageQuota { BillingAccountId = billingAccount.Id, QuotaType = "Deployments", LimitValue = 50.0, UsedValue = 2.0 },
                        new UsageQuota { BillingAccountId = billingAccount.Id, QuotaType = "BuildMinutes", LimitValue = 500.0, UsedValue = 20.0 },
                        new UsageQuota { BillingAccountId = billingAccount.Id, QuotaType = "Repairs", LimitValue = 100.0, UsedValue = 1.0 }
                    );
                    await db.SaveChangesAsync();
                }

                var client = await db.Clients.FirstOrDefaultAsync();
                if (client == null)
                {
                    client = new Client
                    {
                        OrganizationId = org.Id,
                        Name = "Default System Client",
                        Description = "Default Client created for system projects"
                    };
                    db.Clients.Add(client);
                    await db.SaveChangesAsync();

                    var metrics = new ClientMetrics
                    {
                        ClientId = client.Id,
                        TotalProjects = 1,
                        TotalDeployments = 1,
                        AverageLeadTime = 120.0,
                        SuccessRate = 100.0,
                        RepairRate = 0.0,
                        MonthlyBuilds = 1
                    };
                    db.ClientMetrics.Add(metrics);
                    await db.SaveChangesAsync();
                }

                project = new Project
                {
                    Id = systemProjectId,
                    OrganizationId = org.Id,
                    ClientId = client.Id,
                    Name = "DevForge System",
                    Description = "DevForge Internal System Monitoring Project",
                    Status = "Completed"
                };
                db.Projects.Add(project);

                var webAppTemplate = new TemplateCatalog
                {
                    Name = "Enterprise Web App",
                    Version = "1.0.0",
                    Framework = ".NET 9 / Next.js 15",
                    Path = "/app/templates/enterprise-web",
                    Description = "Robust enterprise web portal starter template.",
                    Category = "Web Application",
                    Industry = "Fintech",
                    SuccessRate = 95.0,
                    RepairRate = 5.0,
                    AverageLeadTime = 420.0
                };
                db.TemplateCatalogs.Add(webAppTemplate);
                await db.SaveChangesAsync();

                var schema = new Schema
                {
                    ProjectId = systemProjectId,
                    Version = 1
                };
                db.Schemas.Add(schema);
                await db.SaveChangesAsync();

                var job = new GenerationJob
                {
                    Id = systemJobId,
                    ProjectId = systemProjectId,
                    SchemaId = schema.Id,
                    Status = "Completed",
                    StartedAt = DateTimeOffset.UtcNow,
                    CompletedAt = DateTimeOffset.UtcNow
                };
                db.GenerationJobs.Add(job);
                await db.SaveChangesAsync();

                var usage = new UsageRecord
                {
                    ProjectId = systemProjectId,
                    JobId = systemJobId,
                    GeminiTokens = 15000,
                    GenerationTime = 30.5,
                    BuildTime = 45.2,
                    StorageUsage = 250.0,
                    DeploymentsCount = 1,
                    RepairAttemptsCount = 0,
                    EstimatedCost = 0.45,
                    RecordedAt = DateTimeOffset.UtcNow
                };
                db.UsageRecords.Add(usage);
                await db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"System seeding failed: {ex}");
        }
    }
}
