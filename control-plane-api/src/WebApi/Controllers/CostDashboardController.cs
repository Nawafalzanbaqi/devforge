using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DevForge.ControlPlane.Application.Common.Interfaces;

namespace DevForge.ControlPlane.WebApi.Controllers;

public class CostDashboardController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;

    public CostDashboardController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("cost")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CostDashboardSummary>> GetCostDashboardSummary([FromQuery] Guid organizationId)
    {
        var projectIds = await _context.Projects
            .Where(p => p.OrganizationId == organizationId)
            .Select(p => p.Id)
            .ToListAsync();

        var usageRecords = await _context.UsageRecords
            .Where(u => projectIds.Contains(u.ProjectId))
            .ToListAsync();

        var totalLlmCost = usageRecords.Sum(u => u.EstimatedLlmCost);
        var totalComputeCost = usageRecords.Sum(u => u.EstimatedComputeCost);
        var totalStorageCost = usageRecords.Sum(u => u.EstimatedStorageCost);
        var totalCost = usageRecords.Sum(u => u.EstimatedTotalCost > 0 ? u.EstimatedTotalCost : u.EstimatedCost);

        var summary = new CostDashboardSummary
        {
            TotalLlmCost = totalLlmCost,
            TotalComputeCost = totalComputeCost,
            TotalStorageCost = totalStorageCost,
            TotalCost = totalCost,
            TotalTokensConsumed = usageRecords.Sum(u => u.GeminiTokens),
            TotalBuildMinutes = usageRecords.Sum(u => u.BuildTime) / 60.0,
            TotalStorageUsageMb = usageRecords.Any() ? usageRecords.Max(u => u.StorageUsage) : 0.0
        };

        return Ok(summary);
    }
}

public class CostDashboardSummary
{
    public double TotalLlmCost { get; set; }
    public double TotalComputeCost { get; set; }
    public double TotalStorageCost { get; set; }
    public double TotalCost { get; set; }
    public int TotalTokensConsumed { get; set; }
    public double TotalBuildMinutes { get; set; }
    public double TotalStorageUsageMb { get; set; }
}
