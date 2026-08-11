using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DevForge.ControlPlane.Application.Features.Projects.Commands;
using DevForge.ControlPlane.Application.Features.Projects.Queries;

namespace DevForge.ControlPlane.WebApi.Controllers;

public class ProjectsController : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> Create(CreateProjectCommand command)
    {
        var projectId = await Mediator.Send(command);
        return CreatedAtAction(nameof(Create), new { id = projectId }, projectId);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProjectDto>>> GetList([FromQuery] Guid organizationId)
    {
        var projects = await Mediator.Send(new GetProjectsQuery(organizationId));
        return Ok(projects);
    }
}
