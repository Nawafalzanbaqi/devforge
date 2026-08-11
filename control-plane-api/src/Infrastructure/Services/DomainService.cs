using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using DevForge.ControlPlane.Application.Common.Interfaces;

namespace DevForge.ControlPlane.Infrastructure.Services;

public class DomainService : IDomainService
{
    private readonly ILogger<DomainService> _logger;
    private readonly string _nginxConfigPath;

    public DomainService(ILogger<DomainService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _nginxConfigPath = configuration["Nginx:ProxyConfigPath"] 
                           ?? Path.Combine("C:\\Users\\alzan\\Desktop\\test\\devforge", "infra", "nginx", "data");
    }

    public Task<string> AllocateDomainAsync(Guid projectId, string projectName)
    {
        // Generate a URL-friendly slug (e.g., "Acme CRM Logistics" -> "acmecrmlogistics")
        var cleanName = Regex.Replace(projectName.ToLower(), @"[^a-z0-9]", "");
        if (string.IsNullOrWhiteSpace(cleanName))
        {
            cleanName = $"app-{projectId.ToString().Substring(0, 8)}";
        }
        var domain = $"{cleanName}.local";
        _logger.LogInformation("Allocated domain {Domain} for Project {ProjectId}", domain, projectId);
        return Task.FromResult(domain);
    }

    public async Task ConfigureProxyRouteAsync(string domain, int targetPort)
    {
        _logger.LogInformation("Configuring Nginx Proxy Manager route: {Domain} -> localhost:{Port}", domain, targetPort);
        try
        {
            if (!Directory.Exists(_nginxConfigPath))
            {
                Directory.CreateDirectory(_nginxConfigPath);
            }

            var proxyConfigContent = $$"""
            server {
                listen 80;
                server_name {{domain}};

                location / {
                    proxy_pass http://localhost:{{targetPort}};
                    proxy_set_header Host $host;
                    proxy_set_header X-Real-IP $remote_addr;
                    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
                    proxy_set_header X-Forwarded-Proto $scheme;
                }
            }
            """;

            var configFilePath = Path.Combine(_nginxConfigPath, $"proxy-{domain}.conf");
            await File.WriteAllTextAsync(configFilePath, proxyConfigContent);
            _logger.LogInformation("Saved proxy route configuration to {Path}", configFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write Nginx Proxy config file. Proxy mapping simulated.");
        }
    }
}
