using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DevForge.ControlPlane.Application.Common.Interfaces;
using DevForge.ControlPlane.Domain.Entities;

namespace DevForge.ControlPlane.WebApi.Controllers;

public class ClientsController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IClientMetricsService _metricsService;

    public ClientsController(IApplicationDbContext context, IClientMetricsService metricsService)
    {
        _context = context;
        _metricsService = metricsService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Client>>> GetList([FromQuery] Guid organizationId)
    {
        var clients = await _context.Clients
            .Where(c => c.OrganizationId == organizationId)
            .ToListAsync();
        return Ok(clients);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> Create(CreateClientRequest request)
    {
        var orgExists = await _context.Organizations.AnyAsync(o => o.Id == request.OrganizationId);
        if (!orgExists)
        {
            return BadRequest("Organization does not exist.");
        }

        var client = new Client
        {
            OrganizationId = request.OrganizationId,
            Name = request.Name,
            Description = request.Description,
            Revenue = request.Revenue
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetList), new { organizationId = client.OrganizationId }, client.Id);
    }

    [HttpGet("{id}/metrics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientMetrics>> GetMetrics(Guid id, [FromQuery] bool recalculate = false)
    {
        var clientExists = await _context.Clients.AnyAsync(c => c.Id == id);
        if (!clientExists)
        {
            return NotFound();
        }

        if (recalculate)
        {
            await _metricsService.RecalculateClientMetricsAsync(id);
        }

        var metrics = await _context.ClientMetrics.FirstOrDefaultAsync(m => m.ClientId == id);
        if (metrics == null)
        {
            await _metricsService.RecalculateClientMetricsAsync(id);
            metrics = await _context.ClientMetrics.FirstOrDefaultAsync(m => m.ClientId == id);
        }

        return Ok(metrics);
    }

    [HttpGet("{id}/usage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientUsageSummary>> GetUsage(Guid id)
    {
        var clientExists = await _context.Clients.AnyAsync(c => c.Id == id);
        if (!clientExists)
        {
            return NotFound();
        }

        var projectIds = await _context.Projects
            .Where(p => p.ClientId == id)
            .Select(p => p.Id)
            .ToListAsync();

        var usageRecords = await _context.UsageRecords
            .Where(u => projectIds.Contains(u.ProjectId))
            .ToListAsync();

        var summary = new ClientUsageSummary
        {
            TotalTokens = usageRecords.Sum(u => u.GeminiTokens),
            TotalGenerationTime = usageRecords.Sum(u => u.GenerationTime),
            TotalBuildTime = usageRecords.Sum(u => u.BuildTime),
            TotalStorageUsage = usageRecords.Sum(u => u.StorageUsage),
            TotalDeployments = usageRecords.Sum(u => u.DeploymentsCount),
            TotalRepairAttempts = usageRecords.Sum(u => u.RepairAttemptsCount),
            TotalEstimatedCost = usageRecords.Sum(u => u.EstimatedCost)
        };

        return Ok(summary);
    }
}

public class CreateClientRequest
{
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double Revenue { get; set; }
}

public class ClientUsageSummary
{
    public int TotalTokens { get; set; }
    public double TotalGenerationTime { get; set; }
    public double TotalBuildTime { get; set; }
    public double TotalStorageUsage { get; set; }
    public int TotalDeployments { get; set; }
    public int TotalRepairAttempts { get; set; }
    public double TotalEstimatedCost { get; set; }
}
