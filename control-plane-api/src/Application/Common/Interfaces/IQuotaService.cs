using System;
using System.Threading.Tasks;

namespace DevForge.ControlPlane.Application.Common.Interfaces;

public interface IQuotaService
{
    Task<bool> CanGenerateAsync(Guid organizationId);
    Task<bool> CanDeployAsync(Guid organizationId);
    Task<bool> CanRepairAsync(Guid organizationId);
    Task<bool> CanCreateProjectAsync(Guid organizationId);
    
    Task RecordUsageAsync(Guid organizationId, string quotaType, double amount);
}
