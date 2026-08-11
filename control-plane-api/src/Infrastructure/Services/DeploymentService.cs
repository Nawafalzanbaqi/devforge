using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using DevForge.ControlPlane.Application.Common.Interfaces;

namespace DevForge.ControlPlane.Infrastructure.Services;

public class DeploymentService : IDeploymentService
{
    private readonly ILogger<DeploymentService> _logger;
    private readonly IContainerRuntime _runtime;
    private readonly IDomainService _domainService;
    private readonly string _deploymentsRoot;
    private readonly HttpClient _httpClient;

    public DeploymentService(
        ILogger<DeploymentService> logger,
        IContainerRuntime runtime,
        IDomainService domainService,
        IConfiguration configuration)
    {
        _logger = logger;
        _runtime = runtime;
        _domainService = domainService;
        _deploymentsRoot = configuration["Deployments:RootPath"] 
                           ?? Path.Combine("C:\\Users\\alzan\\Desktop\\test\\devforge", "deployments");
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
    }

    public async Task<DeploymentResult> DeployAsync(Guid jobId, Guid projectId, string workspacePath)
    {
        _logger.LogInformation("Orchestrating deployment for Job {JobId}, Project {ProjectId} in {Path}", jobId, projectId, workspacePath);

        // Define file and logs storage
        var targetDeployFolder = Path.Combine(_deploymentsRoot, projectId.ToString());
        Directory.CreateDirectory(targetDeployFolder);

        var logWriter = new StringWriter();
        await logWriter.WriteLineAsync($"[DEPLOY START] Initiating deployment pipeline at {DateTimeOffset.UtcNow}");

        try
        {
            // 1. Domain Allocation (Deliverable 3 & 4)
            var projectName = Path.GetFileName(workspacePath) ?? $"Project_{projectId}";
            if (projectName.StartsWith("job-"))
            {
                projectName = projectName.Substring(4);
            }
            var domain = await _domainService.AllocateDomainAsync(projectId, projectName);
            await logWriter.WriteLineAsync($"[DOMAIN] Allocated local routing domain: {domain}");

            // Assign a random local port for mapping (e.g. 5000-5100 range)
            var random = new Random();
            var hostPort = random.Next(5000, 5200);
            await logWriter.WriteLineAsync($"[NETWORK] Mapping endpoint route to host port: {hostPort}");

            // 2. Scaffold docker-compose.generated.yml under the project workspace (Deliverable 3)
            var composeContent = $$"""
            version: '3.8'

            services:
              api-{{projectId.ToString().Substring(0, 8)}}:
                image: devforge/generated-api:latest
                container_name: df-api-{{projectId.ToString().Substring(0, 8)}}
                ports:
                  - "{{hostPort}}:8080"
                environment:
                  - ASPNETCORE_ENVIRONMENT=Development
                  - ConnectionStrings__DefaultConnection=Host=postgres-{{projectId.ToString().Substring(0, 8)}};Database=postgres;
                networks:
                  - df-net-{{projectId.ToString().Substring(0, 8)}}

              postgres-{{projectId.ToString().Substring(0, 8)}}:
                image: postgres:17-alpine
                container_name: df-db-{{projectId.ToString().Substring(0, 8)}}
                environment:
                  - POSTGRES_PASSWORD=postgres
                networks:
                  - df-net-{{projectId.ToString().Substring(0, 8)}}

            networks:
              df-net-{{projectId.ToString().Substring(0, 8)}}:
                driver: bridge
            """;

            var composePath = Path.Combine(targetDeployFolder, "compose.yml");
            await File.WriteAllTextAsync(composePath, composeContent);
            await logWriter.WriteLineAsync($"[COMPOSE] Scaffolded deployment compose schema at: {composePath}");

            // 3. Launch Containers using the runtime engine
            await logWriter.WriteLineAsync("[RUNTIME] Launching docker-compose engine up...");
            var runtimeLog = await _runtime.RunComposeUpAsync(composePath);
            await logWriter.WriteLineAsync($"[RUNTIME] {runtimeLog}");

            // 4. Health Check Monitoring GET /health (Deliverable 4 - 90s timeout with retries)
            await logWriter.WriteLineAsync("[HEALTH] Initiating HTTP status verification...");
            var healthCheckPassed = false;
            var maxRetries = 18; // 18 retries * 5s delay = 90 seconds timeout
            var targetHealthUrl = $"http://localhost:{hostPort}/health";

            for (int i = 1; i <= maxRetries; i++)
            {
                await logWriter.WriteLineAsync($"[HEALTH] Attempt {i}/{maxRetries} to query health status endpoint...");
                try
                {
                    // Check if process/container runtime is mock or live
                    if (runtimeLog.Contains("Simulated"))
                    {
                        // In simulation mode, skip HTTP requests and succeed instantly
                        healthCheckPassed = true;
                        await logWriter.WriteLineAsync("[HEALTH] Verification simulated successfully (Mock runtime active).");
                        break;
                    }

                    var response = await _httpClient.GetAsync(targetHealthUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        healthCheckPassed = true;
                        await logWriter.WriteLineAsync($"[HEALTH] Verification succeeded: 200 OK received from {targetHealthUrl}.");
                        break;
                    }
                }
                catch
                {
                    // Ignore transient network errors and wait
                }
                await Task.Delay(5000);
            }

            if (!healthCheckPassed)
            {
                throw new TimeoutException($"Application failed health status checks within the 90s promotion limit at {targetHealthUrl}");
            }

            // 5. Update Reverse Proxy
            await _domainService.ConfigureProxyRouteAsync(domain, hostPort);
            await logWriter.WriteLineAsync($"[PROXY] Reverse proxy routing mapped successfully: {domain} -> localhost:{hostPort}");

            // Generate Score based on health
            var healthScore = healthCheckPassed ? 100 : 0;
            var healthStatus = healthCheckPassed ? "Healthy" : "Offline";

            // 6. Write final deployment.json summary metadata (Deliverable 5)
            var deploymentMetadata = new
            {
                JobId = jobId,
                ProjectId = projectId,
                Domain = domain,
                Port = hostPort,
                ContainerId = $"df-api-{projectId.ToString().Substring(0, 8)}",
                ImageTag = "devforge/generated-api:latest",
                Status = "Running",
                HealthStatus = healthStatus,
                HealthScore = healthScore,
                DeployedAt = DateTimeOffset.UtcNow
            };

            await File.WriteAllTextAsync(
                Path.Combine(targetDeployFolder, "deployment.json"), 
                JsonSerializer.Serialize(deploymentMetadata, new JsonSerializerOptions { WriteIndented = true })
            );

            await logWriter.WriteLineAsync($"[DEPLOY COMPLETE] Process completed successfully at {DateTimeOffset.UtcNow}. Application live at http://{domain}");
            await File.WriteAllTextAsync(Path.Combine(targetDeployFolder, "startup.log"), logWriter.ToString());

            return new DeploymentResult
            {
                Success = true,
                ContainerId = deploymentMetadata.ContainerId,
                AllocatedDomain = domain,
                ImageTag = deploymentMetadata.ImageTag,
                HealthStatus = healthStatus,
                HealthScore = healthScore
            };
        }
        catch (Exception ex)
        {
            await logWriter.WriteLineAsync($"[DEPLOY ERROR] Verification run failed: {ex.Message}");
            _logger.LogError(ex, "Deployment failed for Job {JobId}", jobId);
            
            try
            {
                await File.WriteAllTextAsync(Path.Combine(targetDeployFolder, "startup.log"), logWriter.ToString());
            }
            catch
            {
                // Ignore log writer persistence crash
            }

            return new DeploymentResult
            {
                Success = false,
                HealthStatus = "Offline",
                HealthScore = 0,
                ErrorMessage = ex.Message
            };
        }
    }
}
