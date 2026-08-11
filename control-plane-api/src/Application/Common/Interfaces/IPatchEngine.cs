using System;
using System.Threading.Tasks;

namespace DevForge.ControlPlane.Application.Common.Interfaces;

public interface IPatchEngine
{
    Task ApplyPatchAsync(string workspacePath, RepairProposal proposal);
    Task SavePatchAuditLogAsync(Guid ticketId, string prompt, string response, string diff, string resultJson);
}
