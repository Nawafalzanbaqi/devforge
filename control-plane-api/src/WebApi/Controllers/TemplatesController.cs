using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DevForge.ControlPlane.Application.Common.Interfaces;
using DevForge.ControlPlane.Domain.Entities;

namespace DevForge.ControlPlane.WebApi.Controllers;

public class TemplatesController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly ITemplateScoringService _scoringService;

    public TemplatesController(IApplicationDbContext context, ITemplateScoringService scoringService)
    {
        _context = context;
        _scoringService = scoringService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TemplateCatalog>>> GetList([FromQuery] bool recalculate = false)
    {
        if (recalculate)
        {
            await _scoringService.RecalculateAllTemplatesStatsAsync();
        }

        var templates = await _context.TemplateCatalogs.ToListAsync();
        return Ok(templates);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TemplateCatalog>> GetById(Guid id, [FromQuery] bool recalculate = false)
    {
        if (recalculate)
        {
            await _scoringService.RecalculateTemplateStatsAsync(id);
        }

        var template = await _context.TemplateCatalogs.FindAsync(id);
        if (template == null)
        {
            return NotFound();
        }

        return Ok(template);
    }

    [HttpPost("recalculate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RecalculateAll()
    {
        await _scoringService.RecalculateAllTemplatesStatsAsync();
        return NoContent();
    }
}
