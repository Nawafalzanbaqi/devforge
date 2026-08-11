using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using DevForge.ControlPlane.Application.Common.Interfaces;
using DevForge.ControlPlane.Domain.Entities;

namespace DevForge.ControlPlane.Application.Features.Schemas.Commands;

public record SaveSchemaCommand : IRequest<Guid>
{
    public Guid ProjectId { get; init; }
    public string RawJson { get; init; } = "{}";
}

public class SaveSchemaCommandValidator : AbstractValidator<SaveSchemaCommand>
{
    public SaveSchemaCommandValidator()
    {
        RuleFor(v => v.ProjectId)
            .NotEmpty().WithMessage("Project identifier is required.");
            
        RuleFor(v => v.RawJson)
            .NotEmpty().WithMessage("Schema raw layout JSON is required.");
    }
}

public class SaveSchemaCommandHandler : IRequestHandler<SaveSchemaCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public SaveSchemaCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(SaveSchemaCommand request, CancellationToken cancellationToken)
    {
        // Check if project exists
        var projectExists = await _context.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken);
        if (!projectExists)
        {
            throw new Exception("Target project was not found.");
        }

        // Deactivate previous schemas
        var activeSchemas = await _context.Schemas
            .Where(s => s.ProjectId == request.ProjectId && s.IsActive)
            .ToListAsync(cancellationToken);
            
        int nextVersion = 1;
        
        foreach (var oldSchema in activeSchemas)
        {
            oldSchema.IsActive = false;
            nextVersion = Math.Max(nextVersion, oldSchema.Version + 1);
        }

        var schema = new Schema
        {
            ProjectId = request.ProjectId,
            Version = nextVersion,
            RawJson = request.RawJson,
            IsActive = true
        };

        _context.Schemas.Add(schema);
        await _context.SaveChangesAsync(cancellationToken);

        return schema.Id;
    }
}
