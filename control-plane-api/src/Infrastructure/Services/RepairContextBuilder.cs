using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DevForge.ControlPlane.Application.Common.Interfaces;

namespace DevForge.ControlPlane.Infrastructure.Services;

public class RepairContextBuilder : IRepairContextBuilder
{
    private readonly IApplicationDbContext _db;

    public RepairContextBuilder(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<string> BuildContextAsync(
        Guid projectId, 
        Guid buildId, 
        string source, 
        string errorMessage, 
        string? filePath, 
        int lineNumber, 
        string workspacePath)
    {
        var context = new System.Text.StringBuilder();
        context.AppendLine("=== DEVFORGE AI REPAIR CONTEXT ===");
        context.AppendLine($"Project ID: {projectId}");
        context.AppendLine($"Build ID: {buildId}");
        context.AppendLine($"Failure Source: {source}");
        context.AppendLine($"Error Message: {errorMessage}");
        context.AppendLine($"Target File: {filePath ?? "Unknown"}");
        context.AppendLine($"Line Number: {lineNumber}");
        context.AppendLine();

        // 1. Fetch related schema DTO definitions
        var activeSchema = await _db.Schemas
            .Include(s => s.Entities)
            .ThenInclude(e => e.Attributes)
            .Where(s => s.ProjectId == projectId && s.IsActive)
            .FirstOrDefaultAsync();

        if (activeSchema != null)
        {
            context.AppendLine("=== ACTIVE ENTITY SCHEMA METADATA ===");
            foreach (var entity in activeSchema.Entities)
            {
                context.AppendLine($"Entity: {entity.Name} (Table: {entity.TableName})");
                foreach (var attr in entity.Attributes)
                {
                    context.AppendLine($"  - Attribute: {attr.Name} (Type: {attr.DataType}, Nullable: {attr.IsNullable}, Key: {attr.IsPrimaryKey})");
                }
            }
            context.AppendLine();
        }

        // 2. Fetch target file contents if available
        if (!string.IsNullOrEmpty(filePath))
        {
            var absolutePath = Path.Combine(workspacePath, filePath);
            if (File.Exists(absolutePath))
            {
                context.AppendLine("=== FAILING FILE CONTENTS ===");
                var lines = await File.ReadAllLinesAsync(absolutePath);
                for (int i = 0; i < lines.Length; i++)
                {
                    context.AppendLine($"{i + 1}: {lines[i]}");
                }
                context.AppendLine();
            }
            else
            {
                // Fallback to searching files inside workspace
                context.AppendLine("=== WORKSPACE FILES LIST ===");
                try
                {
                    var files = Directory.GetFiles(workspacePath, "*.cs", SearchOption.AllDirectories)
                        .Select(f => Path.GetRelativePath(workspacePath, f))
                        .Take(15);
                    foreach (var file in files)
                    {
                        context.AppendLine($"- {file}");
                    }
                }
                catch
                {
                    // Ignore directory read errors
                }
                context.AppendLine();
            }
        }

        return context.ToString();
    }
}
