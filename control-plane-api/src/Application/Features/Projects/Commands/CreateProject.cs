using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using FluentValidation;
using DevForge.ControlPlane.Application.Common.Interfaces;
using DevForge.ControlPlane.Domain.Entities;

namespace DevForge.ControlPlane.Application.Features.Projects.Commands;

public record CreateProjectCommand : IRequest<Guid>
{
    public Guid OrganizationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? RepositoryUrl { get; init; }
}

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(100).WithMessage("Project name must not exceed 100 characters.");
            
        RuleFor(v => v.OrganizationId)
            .NotEmpty().WithMessage("Organization identifier is required.");
    }
}

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateProjectCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = new Project
        {
            OrganizationId = request.OrganizationId,
            Name = request.Name,
            Description = request.Description,
            RepositoryUrl = request.RepositoryUrl,
            Status = "Draft"
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync(cancellationToken);

        return project.Id;
    }
}
