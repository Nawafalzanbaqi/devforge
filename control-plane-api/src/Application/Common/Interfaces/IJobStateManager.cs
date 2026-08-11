using System;
using System.Threading.Tasks;

namespace DevForge.ControlPlane.Application.Common.Interfaces;

public interface IJobStateManager
{
    Task TransitionToQueuedAsync(Guid jobId);
    Task TransitionToGeneratingAsync(Guid jobId);
    Task TransitionToCompilingAsync(Guid jobId);
    Task TransitionToTestingAsync(Guid jobId);
    Task TransitionToDeployingAsync(Guid jobId);
    Task TransitionToPassedQaAsync(Guid jobId);
    Task TransitionToFailedQaAsync(Guid jobId, string errorSummary);
    Task TransitionToCompletedAsync(Guid jobId);
    Task TransitionToFailedAsync(Guid jobId, string errorSummary);
}
