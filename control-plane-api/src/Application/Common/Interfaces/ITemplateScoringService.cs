using System;
using System.Threading.Tasks;

namespace DevForge.ControlPlane.Application.Common.Interfaces;

public interface ITemplateScoringService
{
    Task RecalculateTemplateStatsAsync(Guid templateId);
    Task RecalculateAllTemplatesStatsAsync();
}
