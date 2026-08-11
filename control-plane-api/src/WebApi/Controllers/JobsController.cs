using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DevForge.ControlPlane.Application.Features.Jobs.Commands;
using DevForge.ControlPlane.Application.Common.Interfaces;

namespace DevForge.ControlPlane.WebApi.Controllers;

public class JobsController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IQuotaService _quotaService;

    public JobsController(IApplicationDbContext context, IQuotaService quotaService)
    {
        _context = context;
        _quotaService = quotaService;
    }

    [HttpPost("trigger")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Guid>> Trigger(TriggerGenerationCommand command)
    {
        var project = await _context.Projects.FindAsync(command.ProjectId);
        if (project == null) return BadRequest("Project not found.");

        var canGenerate = await _quotaService.CanGenerateAsync(project.OrganizationId);
        if (!canGenerate)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Quota exceeded or plan features restriction.");
        }

        var jobId = await Mediator.Send(command);
        return Accepted(jobId);
    }
}
