using System;
using System.Threading.Tasks;

namespace DevForge.ControlPlane.Application.Common.Interfaces;

public interface ICodeInjectionEngine
{
    Task InjectSchemaIntoTemplateAsync(Guid jobId, Guid templateId);
}
