using System.Threading.Tasks;

namespace DevForge.ControlPlane.Application.Common.Interfaces;

public class RepairProposal
{
    public string Action { get; set; } = "Modify"; // Create, Modify, Delete
    public string FilePath { get; set; } = string.Empty;
    public string TargetContent { get; set; } = string.Empty;
    public string ReplacementContent { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
}

public interface IRepairAgent
{
    Task<RepairProposal> GenerateFixAsync(string contextText, string source, string errorSummary);
}
