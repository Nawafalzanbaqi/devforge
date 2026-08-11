using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DevForge.ControlPlane.Application.Common.Interfaces;
using DevForge.ControlPlane.Domain.Entities;

namespace DevForge.ControlPlane.Infrastructure.Services;

public class BillingService : IBillingService
{
    private readonly IApplicationDbContext _db;

    public BillingService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Invoice> GenerateInvoiceAsync(Guid billingAccountId, DateTimeOffset start, DateTimeOffset end)
    {
        var account = await _db.BillingAccounts
            .Include(b => b.Organization)
            .FirstOrDefaultAsync(b => b.Id == billingAccountId);

        if (account == null)
        {
            throw new ArgumentException($"Billing account {billingAccountId} not found.");
        }

        // Find all projects under this organization
        var projectIds = await _db.Projects
            .Where(p => p.OrganizationId == account.OrganizationId)
            .Select(p => p.Id)
            .ToListAsync();

        // Get usage records for this organization projects in the date range
        var usageRecords = await _db.UsageRecords
            .Where(u => projectIds.Contains(u.ProjectId) && u.RecordedAt >= start && u.RecordedAt <= end)
            .ToListAsync();

        // Aggregate
        var totalTokens = usageRecords.Sum(u => u.GeminiTokens);
        var totalStorage = usageRecords.Any() ? usageRecords.Max(u => u.StorageUsage) : 0.0; // max storage checked in this cycle
        var totalDeployments = usageRecords.Sum(u => u.DeploymentsCount);
        var totalBuildMinutes = usageRecords.Sum(u => u.BuildTime) / 60.0;
        var totalRepairAttempts = usageRecords.Sum(u => u.RepairAttemptsCount);

        // Sum costs
        var calculatedCost = usageRecords.Sum(u => u.EstimatedTotalCost > 0 ? u.EstimatedTotalCost : u.EstimatedCost);

        var invoice = new Invoice
        {
            BillingAccountId = billingAccountId,
            IssueDate = DateTimeOffset.UtcNow,
            DueDate = DateTimeOffset.UtcNow.AddDays(15),
            Amount = calculatedCost,
            Status = "Pending",
            UsedTokens = totalTokens,
            UsedStorage = totalStorage,
            UsedDeployments = totalDeployments,
            UsedBuildMinutes = totalBuildMinutes,
            UsedRepairAttempts = totalRepairAttempts
        };

        _db.Invoices.Add(invoice);
        
        // Update account balance
        account.CurrentBalance += calculatedCost;
        
        await _db.SaveChangesAsync();

        return invoice;
    }
}
