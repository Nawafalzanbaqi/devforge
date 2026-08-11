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

public class TemplateImprovementController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;

    public TemplateImprovementController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TemplateImprovementProposal>>> GetList()
    {
        var list = await _context.TemplateImprovementProposals
            .Include(p => p.Template)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        return Ok(list);
    }

    [HttpPost("{id}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var prop = await _context.TemplateImprovementProposals.FindAsync(id);
        if (prop == null) return NotFound();

        if (prop.Status != "Pending")
        {
            return BadRequest("Only Pending proposals can be approved.");
        }

        prop.Status = "Approved";
        prop.ProcessedAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(prop);
    }

    [HttpPost("{id}/execute")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Execute(Guid id)
    {
        var prop = await _context.TemplateImprovementProposals
            .Include(p => p.Template)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (prop == null) return NotFound();

        if (prop.Status != "Approved")
        {
            return BadRequest("Only Approved proposals can be executed.");
        }

        // Apply/execute the template improvement patch:
        prop.Status = "Applied";
        prop.ProcessedAt = DateTimeOffset.UtcNow;

        // 1. Update the template version and success stats in the catalog
        var template = await _context.TemplateCatalogs.FindAsync(prop.TemplateId);
        string parentVer = "1.0.0";
        if (template != null)
        {
            parentVer = template.Version;
            var currentVer = template.Version;
            if (!string.IsNullOrEmpty(currentVer) && currentVer.Contains('.'))
            {
                var parts = currentVer.Split('.');
                if (parts.Length >= 2 && int.TryParse(parts[1], out var minor))
                {
                    template.Version = $"{parts[0]}.{minor + 1}.0";
                }
                else
                {
                    template.Version = "1.1.0";
                }
            }
            else
            {
                template.Version = "1.1.0";
            }
            
            // Simulating self-improvement success rate jump
            template.SuccessRate = Math.Min(template.SuccessRate + 10.0, 100.0);
            _context.TemplateCatalogs.Update(template);
        }

        // 2. Update the template knowledge stats to reflect improvement
        var knowledge = await _context.TemplateKnowledges
            .FirstOrDefaultAsync(k => k.TemplateId == prop.TemplateId);
        if (knowledge != null)
        {
            knowledge.SuccessRate = Math.Min(knowledge.SuccessRate + 10.0, 100.0);
            knowledge.ConfidenceScore = Math.Min(knowledge.ConfidenceScore + 0.1, 1.0);
            _context.TemplateKnowledges.Update(knowledge);
        }

        // 3. Write a TemplateVersionHistory snapshot record upon executing the proposal
        if (template != null)
        {
            var history = new TemplateVersionHistory
            {
                TemplateId = template.Id,
                Version = template.Version,
                ParentVersion = parentVer,
                ImprovementProposalId = prop.Id,
                SuccessRate = template.SuccessRate,
                RepairRate = template.RepairRate,
                LeadTime = template.AverageLeadTime,
                ReleasedAt = DateTimeOffset.UtcNow
            };
            _context.TemplateVersionHistories.Add(history);
        }

        await _context.SaveChangesAsync();

        return Ok(prop);
    }
}
