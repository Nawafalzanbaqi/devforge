using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DevForge.ControlPlane.Application.Features.Schemas.Commands;

namespace DevForge.ControlPlane.WebApi.Controllers;

public class SchemasController : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> SaveDraft(SaveSchemaCommand command)
    {
        var schemaId = await Mediator.Send(command);
        return Ok(schemaId);
    }
}
