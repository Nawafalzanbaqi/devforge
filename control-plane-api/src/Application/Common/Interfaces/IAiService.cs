using System.Threading.Tasks;

namespace DevForge.ControlPlane.Application.Common.Interfaces;

public interface IAiService
{
    Task<string> GenerateTextAsync(string prompt, string? systemInstruction = null);
    Task<T?> GenerateStructuredJsonAsync<T>(string prompt, string? systemInstruction = null) where T : class;
}
