using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DevForge.ControlPlane.Application.Common.Interfaces;
using DevForge.ControlPlane.Domain.Entities;

namespace DevForge.ControlPlane.Infrastructure.Services;

public class QuotaService : IQuotaService
{
    private readonly IApplicationDbContext _db;

    public QuotaService(IApplicationDbContext db)
    {
        _db = db;
    }

    private async Task<(Subscription Subscription, BillingAccount Account)> GetBillingDetailsAsync(Guid organizationId)
    {
        var account = await _db.BillingAccounts
            .Include(b => b.Invoices)
            .FirstOrDefaultAsync(b => b.OrganizationId == organizationId);

        if (account == null)
        {
            // Auto-provision a free billing account if none exists
            account = new BillingAccount
            {
                OrganizationId = organizationId,
                PlanTier = "Free",
                CurrentBalance = 0.0,
                Status = "Active"
            };
            _db.BillingAccounts.Add(account);
            await _db.SaveChangesAsync();
        }

        var sub = await _db.Subscriptions.FirstOrDefaultAsync(s => s.BillingAccountId == account.Id);
        if (sub == null)
        {
            sub = new Subscription
            {
                BillingAccountId = account.Id,
                PlanName = account.PlanTier,
                Status = "Active"
            };
            _db.Subscriptions.Add(sub);
            await _db.SaveChangesAsync();
        }

        return (sub, account);
    }

    private async Task<bool> CheckFeatureAsync(string planName, string featureKey)
    {
        var feature = await _db.PlanFeatures
            .FirstOrDefaultAsync(f => f.PlanName == planName && f.FeatureKey == featureKey);
        
        return feature?.IsEnabled ?? false;
    }

    private async Task<bool> CheckQuotaAsync(Guid billingAccountId, string quotaType, double additionalRequired = 0)
    {
        var quota = await _db.UsageQuotas
            .FirstOrDefaultAsync(q => q.BillingAccountId == billingAccountId && q.QuotaType == quotaType);

        if (quota == null)
        {
            // If quota doesn't exist, create it with a high value or look up defaults.
            // Let's create it with plan defaults.
            quota = new UsageQuota
            {
                BillingAccountId = billingAccountId,
                QuotaType = quotaType,
                LimitValue = quotaType switch
                {
                    "Tokens" => 500000.0,
                    "Storage" => 1024.0,
                    "Deployments" => 5.0,
                    "BuildMinutes" => 60.0,
                    "Repairs" => 10.0,
                    _ => 10.0
                },
                UsedValue = 0.0
            };
            _db.UsageQuotas.Add(quota);
            await _db.SaveChangesAsync();
        }

        return (quota.UsedValue + additionalRequired) <= quota.LimitValue;
    }

    public async Task<bool> CanGenerateAsync(Guid organizationId)
    {
        var (sub, account) = await GetBillingDetailsAsync(organizationId);

        // 1. Check feature permission
        var hasFeature = await CheckFeatureAsync(sub.PlanName, "CanGenerate");
        if (!hasFeature) return false;

        // 2. Check Tokens Quota
        return await CheckQuotaAsync(account.Id, "Tokens");
    }

    public async Task<bool> CanDeployAsync(Guid organizationId)
    {
        var (sub, account) = await GetBillingDetailsAsync(organizationId);

        // 1. Check feature permission
        var hasFeature = await CheckFeatureAsync(sub.PlanName, "CanDeploy");
        if (!hasFeature) return false;

        // 2. Check Deployments Quota
        return await CheckQuotaAsync(account.Id, "Deployments");
    }

    public async Task<bool> CanRepairAsync(Guid organizationId)
    {
        var (sub, account) = await GetBillingDetailsAsync(organizationId);

        // 1. Check feature permission
        var hasFeature = await CheckFeatureAsync(sub.PlanName, "CanRepair");
        if (!hasFeature) return false;

        // 2. Check Repairs Quota
        return await CheckQuotaAsync(account.Id, "Repairs");
    }

    public async Task<bool> CanCreateProjectAsync(Guid organizationId)
    {
        var (sub, account) = await GetBillingDetailsAsync(organizationId);

        // 1. Check feature permission
        var hasFeature = await CheckFeatureAsync(sub.PlanName, "CanCreateProject");
        if (!hasFeature) return false;

        // 2. If Free Plan, limit is strictly 5 projects
        if (sub.PlanName == "Free")
        {
            var projectCount = await _db.Projects.CountAsync(p => p.OrganizationId == organizationId);
            if (projectCount >= 5) return false;
        }

        return true;
    }

    public async Task RecordUsageAsync(Guid organizationId, string quotaType, double amount)
    {
        var (_, account) = await GetBillingDetailsAsync(organizationId);
        
        var quota = await _db.UsageQuotas
            .FirstOrDefaultAsync(q => q.BillingAccountId == account.Id && q.QuotaType == quotaType);

        if (quota != null)
        {
            quota.UsedValue += amount;
            quota.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}
