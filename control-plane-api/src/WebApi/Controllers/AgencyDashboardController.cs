using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DevForge.ControlPlane.Application.Common.Interfaces;

namespace DevForge.ControlPlane.WebApi.Controllers;

public class AgencyDashboardController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;

    public AgencyDashboardController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("agency")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<AgencyDashboardSummary>> GetAgencyDashboardSummary([FromQuery] Guid organizationId)
    {
        var clientsCount = await _context.Clients.CountAsync(c => c.OrganizationId == organizationId);
        
        var clientIds = await _context.Clients
            .Where(c => c.OrganizationId == organizationId)
            .Select(c => c.Id)
            .ToListAsync();

        var projects = await _context.Projects
            .Where(p => p.OrganizationId == organizationId || (p.ClientId != null && clientIds.Contains(p.ClientId.Value)))
            .ToListAsync();

        var projectIds = projects.Select(p => p.Id).ToList();

        var totalProjects = projects.Count;
        var totalDeployments = await _context.Deployments.CountAsync(d => projectIds.Contains(d.ProjectId));

        var jobs = await _context.GenerationJobs
            .Where(j => projectIds.Contains(j.ProjectId))
            .ToListAsync();

        var totalBuilds = jobs.Count;
        var successRate = totalBuilds == 0 ? 100.0 : ((double)jobs.Count(j => j.Status == "Completed") / totalBuilds) * 100.0;

        // MRR estimation: sum of invoice amounts in last 30 days
        var billingAccount = await _context.BillingAccounts
            .FirstOrDefaultAsync(b => b.OrganizationId == organizationId);
        
        double monthlyRecurringRevenue = 0.0;
        if (billingAccount != null)
        {
            var thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30);
            monthlyRecurringRevenue = await _context.Invoices
                .Where(i => i.BillingAccountId == billingAccount.Id && i.IssueDate >= thirtyDaysAgo && i.Status == "Paid")
                .SumAsync(i => i.Amount);
        }

        var summary = new AgencyDashboardSummary
        {
            TotalClients = clientsCount,
            TotalProjects = totalProjects,
            TotalDeployments = totalDeployments,
            TotalBuilds = totalBuilds,
            SuccessRate = successRate,
            MRR = monthlyRecurringRevenue
        };

        return Ok(summary);
    }
}

public class AgencyDashboardSummary
{
    public int TotalClients { get; set; }
    public int TotalProjects { get; set; }
    public int TotalDeployments { get; set; }
    public int TotalBuilds { get; set; }
    public double SuccessRate { get; set; }
    public double MRR { get; set; }
}
