using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using DevForge.ControlPlane.Application.Common.Interfaces;

namespace DevForge.ControlPlane.Infrastructure.Services;

public class AiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public AiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["AI:GeminiApiKey"] ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? string.Empty;
    }

    public async Task<string> GenerateTextAsync(string prompt, string? systemInstruction = null)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("Gemini API key is not configured. Configure AI:GeminiApiKey in appsettings.");
        }

        var url = $"v1beta/models/gemini-1.5-pro:generateContent?key={_apiKey}";

        var requestBody = new GeminiRequest
        {
            Contents = new[]
            {
                new GeminiContent
                {
                    Parts = new[] { new GeminiPart { Text = prompt } }
                }
            }
        };

        if (!string.IsNullOrEmpty(systemInstruction))
        {
            requestBody.SystemInstruction = new GeminiInstruction
            {
                Parts = new[] { new GeminiPart { Text = systemInstruction } }
            };
        }

        var response = await _httpClient.PostAsJsonAsync(url, requestBody);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Gemini API call failed with status: {response.StatusCode}. Details: {error}");
        }

        var geminiResponse = await response.Content.ReadFromJsonAsync<GeminiResponse>();
        var resultText = geminiResponse?.Candidates?[0]?.Content?.Parts?[0]?.Text;

        return resultText ?? string.Empty;
    }

    public async Task<T?> GenerateStructuredJsonAsync<T>(string prompt, string? systemInstruction = null) where T : class
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("Gemini API key is not configured.");
        }

        var url = $"v1beta/models/gemini-1.5-pro:generateContent?key={_apiKey}";

        var requestBody = new GeminiRequest
        {
            Contents = new[]
            {
                new GeminiContent
                {
                    Parts = new[] { new GeminiPart { Text = prompt } }
                }
            },
            GenerationConfig = new GeminiConfig
            {
                ResponseMimeType = "application/json"
            }
        };

        if (!string.IsNullOrEmpty(systemInstruction))
        {
            requestBody.SystemInstruction = new GeminiInstruction
            {
                Parts = new[] { new GeminiPart { Text = systemInstruction } }
            };
        }

        var response = await _httpClient.PostAsJsonAsync(url, requestBody);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Gemini API call failed with status: {response.StatusCode}. Details: {error}");
        }

        var geminiResponse = await response.Content.ReadFromJsonAsync<GeminiResponse>();
        var jsonText = geminiResponse?.Candidates?[0]?.Content?.Parts?[0]?.Text;

        if (string.IsNullOrEmpty(jsonText))
        {
            return null;
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<T>(jsonText, options);
    }

    // Gemini API Request mappings
    private class GeminiRequest
    {
        public GeminiContent[] Contents { get; set; } = Array.Empty<GeminiContent>();
        public GeminiInstruction? SystemInstruction { get; set; }
        public GeminiConfig? GenerationConfig { get; set; }
    }

    private class GeminiContent
    {
        public GeminiPart[] Parts { get; set; } = Array.Empty<GeminiPart>();
    }

    private class GeminiPart
    {
        public string Text { get; set; } = string.Empty;
    }

    private class GeminiInstruction
    {
        public GeminiPart[] Parts { get; set; } = Array.Empty<GeminiPart>();
    }

    private class GeminiConfig
    {
        public string ResponseMimeType { get; set; } = "text/plain";
    }

    // Gemini API Response mappings
    private class GeminiResponse
    {
        public GeminiCandidate[]? Candidates { get; set; }
    }

    private class GeminiCandidate
    {
        public GeminiResponseContent? Content { get; set; }
    }

    private class GeminiResponseContent
    {
        public GeminiPart[]? Parts { get; set; }
    }
}
