using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DevForge.ControlPlane.Application.Common.Interfaces;
using DevForge.ControlPlane.Domain.Entities;

namespace DevForge.ControlPlane.Infrastructure.Services;

public class CodeInjectionEngine : ICodeInjectionEngine
{
    private readonly IApplicationDbContext _context;
    private readonly IWorkspaceManager _workspace;
    private readonly IAiService _ai;

    public CodeInjectionEngine(IApplicationDbContext context, IWorkspaceManager workspace, IAiService ai)
    {
        _context = context;
        _workspace = workspace;
        _ai = ai;
    }

    public async Task InjectSchemaIntoTemplateAsync(Guid jobId, Guid templateId)
    {
        // 1. Fetch Job and Template Catalog
        var job = await _context.GenerationJobs
            .Include(j => j.Schema)
                .ThenInclude(s => s.Entities)
                    .ThenInclude(e => e.Attributes)
            .FirstOrDefaultAsync(j => j.Id == jobId);

        if (job == null) throw new Exception("Job context was not found.");

        var template = await _context.TemplateCatalogs.FindAsync(templateId);
        if (template == null) throw new Exception("Template catalog record not found.");

        // 2. Resolve paths
        var workspacePath = _workspace.CreateWorkspace(jobId);
        var templatePath = template.Path;

        if (!Directory.Exists(templatePath))
        {
            // For testing: if the local template directory is empty, create mock assets
            CreateMockTemplateAssets(templatePath);
        }

        // 3. Copy Boilerplate to Workspace
        CopyDirectoryRecursively(templatePath, workspacePath);

        // 4. Inject Entities Code Mappings
        var dbSetStringBuilder = new StringBuilder();
        var modelCreatingBuilder = new StringBuilder();

        foreach (var entity in job.Schema.Entities)
        {
            // Scaffold dynamic C# class structure for target entity
            var classCode = BuildEntityClassCode(entity);
            _workspace.WriteFile(jobId, $"src/Domain/Entities/{entity.Name}.cs", classCode);

            // Scaffold MediatR CQRS commands
            var createCommandCode = BuildCreateCommandCode(entity);
            _workspace.WriteFile(jobId, $"src/Application/Features/{entity.Name}s/Commands/Create{entity.Name}.cs", createCommandCode);

            // Scaffold Web API Controller endpoints
            var controllerCode = BuildControllerCode(entity);
            _workspace.WriteFile(jobId, $"src/WebApi/Controllers/{entity.Name}sController.cs", controllerCode);

            // Build DbSet configurations
            dbSetStringBuilder.AppendLine($"    public DbSet<{entity.Name}> {entity.Name}s => Set<{entity.Name}>();");

            // Build Relationships maps
            var foreignKeys = entity.Attributes.Where(a => a.IsForeignKey && a.ReferencedEntityId != null);
            foreach (var fk in foreignKeys)
            {
                var refEntity = job.Schema.Entities.FirstOrDefault(e => e.Id == fk.ReferencedEntityId);
                if (refEntity != null)
                {
                    modelCreatingBuilder.AppendLine($"        modelBuilder.Entity<{entity.Name}>()")
                                        .AppendLine($"            .HasOne(x => x.{refEntity.Name})")
                                        .AppendLine($"            .WithMany()")
                                        .AppendLine($"            .HasForeignKey(x => x.{fk.ColumnName});");
                }
            }
        }

        // 5. Inject DBContext placeholders replacement
        var dbContextPath = Path.Combine(workspacePath, "src/Infrastructure/Persistence/GeneratedDbContext.cs");
        if (File.Exists(dbContextPath))
        {
            var dbContextContent = File.ReadAllText(dbContextPath);
            dbContextContent = dbContextContent.Replace("// [DEVFORGE:DBSET_INJECTION]", dbSetStringBuilder.ToString());
            dbContextContent = dbContextContent.Replace("// [DEVFORGE:RELATIONAL_INJECTION]", modelCreatingBuilder.ToString());
            File.WriteAllText(dbContextPath, dbContextContent);
        }
    }

    private void CopyDirectoryRecursively(string sourceDir, string targetDir)
    {
        foreach (string dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourceDir, targetDir));
        }

        foreach (string newPath in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(sourceDir, targetDir), true);
        }
    }

    private void CreateMockTemplateAssets(string templatePath)
    {
        Directory.CreateDirectory(Path.Combine(templatePath, "src/Infrastructure/Persistence"));
        Directory.CreateDirectory(Path.Combine(templatePath, "src/Domain/Entities"));
        Directory.CreateDirectory(Path.Combine(templatePath, "src/Application/Features"));
        Directory.CreateDirectory(Path.Combine(templatePath, "src/WebApi/Controllers"));

        // Create baseline DbContext containing markers
        var mockContext = @"using Microsoft.EntityFrameworkCore;
namespace TemplateApp.Infrastructure.Persistence;

public class GeneratedDbContext : DbContext
{
    // [DEVFORGE:DBSET_INJECTION]

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // [DEVFORGE:RELATIONAL_INJECTION]
    }
}";
        File.WriteAllText(Path.Combine(templatePath, "src/Infrastructure/Persistence/GeneratedDbContext.cs"), mockContext);
    }

    private string BuildEntityClassCode(Entity entity)
    {
        var properties = new StringBuilder();
        foreach (var attr in entity.Attributes)
        {
            var csharpType = GetCsharpType(attr.DataType);
            properties.AppendLine($"    public {csharpType}{(attr.IsNullable ? "?" : "")} {attr.Name} {{ get; set; }}");
        }

        return $@"using System;

namespace TemplateApp.Domain.Entities;

public class {entity.Name}
{{
{properties}
}}";
    }

    private string BuildCreateCommandCode(Entity entity)
    {
        return $@"using System;
using MediatR;

namespace TemplateApp.Application.Features.{entity.Name}s.Commands;

public record Create{entity.Name}Command : IRequest<Guid>
{{
    // Generated Command parameters
}}";
    }

    private string BuildControllerCode(Entity entity)
    {
        return $@"using System;
using Microsoft.AspNetCore.Mvc;

namespace TemplateApp.WebApi.Controllers;

[ApiController]
[Route(""api/[controller]"")]
public class {entity.Name}sController : ControllerBase
{{
    // Generated Endpoint controller mappings
}}";
    }

    private string GetCsharpType(string dbType)
    {
        return dbType.ToLower() switch
        {
            "varchar" => "string",
            "integer" => "int",
            "decimal" => "decimal",
            "boolean" => "bool",
            "datetime" => "DateTime",
            "uuid" => "Guid",
            _ => "string"
        };
    }
}
