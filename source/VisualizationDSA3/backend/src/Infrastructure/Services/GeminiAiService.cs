using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class GeminiAiService : IAIVisualizerService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiAiService> _logger;

    public GeminiAiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiAiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AIVisualizerResponse> GenerateVisualizationAsync(AIVisualizerRequest request)
    {
        var apiKey = _configuration["GeminiAi:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new Exception("Gemini API Key is not configured in appsettings.json.");
        }

        string systemPrompt = @"You are a Code-to-Animation JSON Engine for a DSA educational platform.
The user provides a code snippet (e.g. Bubble Sort, Selection Sort, etc).
Analyze the step-by-step memory changes and output a STRICT JSON array of animation frames.
DO NOT use markdown wrappers. OUTPUT RAW JSON ONLY.

JSON format rule:
Return an array of objects.
For comparing two indices: { ""type"": ""compare"", ""i"": <int>, ""j"": <int> }
For swapping two indices: { ""type"": ""swap"", ""i"": <int>, ""j"": <int> }
For marking a sorted index: { ""type"": ""highlight"", ""i"": <int> }

Example Output:
[
  { ""type"": ""compare"", ""i"": 0, ""j"": 1 },
  { ""type"": ""swap"", ""i"": 0, ""j"": 1 },
  { ""type"": ""highlight"", ""i"": 4 }
]";

        var payload = new
        {
            contents = new[]
            {
                new { role = "user", parts = new[] { new { text = request.Code } } }
            },
            systemInstruction = new
            {
                role = "user",
                parts = new[] { new { text = systemPrompt } }
            },
            generationConfig = new
            {
                temperature = 0.1,
                responseMimeType = "application/json"
            }
        };

        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";
        
        var response = await _httpClient.PostAsync(url, content);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError($"Gemini API Error: {error}");
            throw new Exception($"AI Analysis failed with status: {response.StatusCode}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        
        using var doc = JsonDocument.Parse(jsonResponse);
        try 
        {
            var textResult = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return new AIVisualizerResponse { JsonData = textResult ?? "[]" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Gemini JSON response.");
            return new AIVisualizerResponse { JsonData = "[]" };
        }
    }
}
