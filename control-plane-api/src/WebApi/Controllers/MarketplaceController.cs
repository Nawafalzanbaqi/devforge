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

public class MarketplaceController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;

    public MarketplaceController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("leaderboards")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TemplateCatalog>>> GetLeaderboard([FromQuery] string sortBy = "popularity")
    {
        IQueryable<TemplateCatalog> query = _context.TemplateCatalogs;

        query = sortBy.ToLower() switch
        {
            "success" => query.OrderByDescending(t => t.SuccessRate),
            "leadtime" => query.OrderBy(t => t.AverageLeadTime),
            "repair" => query.OrderBy(t => t.RepairRate),
            "popularity" => query.OrderByDescending(t => t.TemplateUsageCount),
            _ => query.OrderByDescending(t => t.TemplateUsageCount)
        };

        var results = await query.ToListAsync();
        return Ok(results);
    }
}
