using System;
using System.Threading.Tasks;

namespace DevForge.ControlPlane.Application.Common.Interfaces;

public interface IDomainService
{
    Task<string> AllocateDomainAsync(Guid projectId, string projectName);
    Task ConfigureProxyRouteAsync(string domain, int targetPort);
}
