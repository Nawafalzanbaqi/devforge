using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using DevForge.ControlPlane.Application.Common.Interfaces;
using DevForge.ControlPlane.Application.Common.Messages;
using DevForge.ControlPlane.Domain.Entities;
using MassTransit;

namespace DevForge.ControlPlane.Application.Features.Jobs.Commands;

public record TriggerGenerationCommand : IRequest<Guid>
{
    public Guid ProjectId { get; init; }
    public Guid? InitiatedBy { get; init; }
}

public class TriggerGenerationCommandValidator : AbstractValidator<TriggerGenerationCommand>
{
    public TriggerGenerationCommandValidator()
    {
        RuleFor(v => v.ProjectId)
            .NotEmpty().WithMessage("Project identifier is required.");
    }
}

public class TriggerGenerationCommandHandler : IRequestHandler<TriggerGenerationCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public TriggerGenerationCommandHandler(IApplicationDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Guid> Handle(TriggerGenerationCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch active schema
        var activeSchema = await _context.Schemas
            .Where(s => s.ProjectId == request.ProjectId && s.IsActive)
            .FirstOrDefaultAsync(cancellationToken);
            
        if (activeSchema == null)
        {
            throw new Exception("No active schema draft found for this project. Save a schema first.");
        }

        // 2. Update project status
        var project = await _context.Projects.FindAsync(new object[] { request.ProjectId }, cancellationToken);
        if (project != null)
        {
            project.Status = "Generating";
        }

        // 3. Create Generation Job
        var job = new GenerationJob
        {
            ProjectId = request.ProjectId,
            SchemaId = activeSchema.Id,
            Status = "Pending",
            InitiatedBy = request.InitiatedBy,
            StartedAt = DateTimeOffset.UtcNow
        };

        _context.GenerationJobs.Add(job);
        await _context.SaveChangesAsync(cancellationToken);

        // 4. Publish job notification via RabbitMQ MassTransit message bus
        await _publishEndpoint.Publish(new GenerateApplicationJob
        {
            JobId = job.Id,
            ProjectId = request.ProjectId
        }, cancellationToken);

        return job.Id;
    }
}
