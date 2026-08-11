using System.Threading.Tasks;

namespace DevForge.ControlPlane.Application.Common.Interfaces;

public interface IGitService
{
    Task InitializeRepositoryAsync(string path);
    Task CommitChangesAsync(string path, string message);
    Task PushChangesAsync(string path, string remoteUrl);
    Task CreateBranchAsync(string path, string branchName);
    Task CheckoutBranchAsync(string path, string branchName);
    Task MergeBranchAsync(string path, string sourceBranch, string targetBranch);
}
