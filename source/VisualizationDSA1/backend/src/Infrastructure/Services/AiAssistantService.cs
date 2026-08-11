using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using VisualizationDSA.Application.Common.Interfaces;

namespace VisualizationDSA.Infrastructure.Services
{
    public class AiAssistantService : IAiAssistantService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public AiAssistantService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["DeepSeek:ApiKey"];
        }

        public async Task<string> GenerateContentAsync(string prompt)
        {
            if (string.IsNullOrEmpty(_apiKey) || _apiKey == "MOCK_KEY" || _apiKey == "YOUR_GEMINI_API_KEY")
            {
                throw new NotImplementedException("AI Assistant chưa được cấu hình API Key (DeepSeek:ApiKey). Hãy thiết lập trước khi sử dụng.");
            }

            var requestUri = "https://api.deepseek.com/chat/completions";

            var requestBody = new
            {
                model = "deepseek-v4-flash",
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = "You are a helpful teaching assistant for a Data Structures and Algorithms learning platform. Please answer concisely in Vietnamese and format code blocks properly using markdown."
                    },
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },
                max_tokens = 1024
            };

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            try
            {
                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"AI Server trả về lỗi: {(int)response.StatusCode} - {error}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(responseString);

                if (jsonDoc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var firstChoice = choices[0];
                    if (firstChoice.TryGetProperty("message", out var message) &&
                        message.TryGetProperty("content", out var content))
                    {
                        var text = content.GetString();
                        if (!string.IsNullOrEmpty(text))
                        {
                            return text;
                        }
                    }
                }

                throw new InvalidOperationException("Không thể phân tích nội dung phản hồi từ AI.");
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (JsonException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (NotImplementedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"Lỗi kết nối AI: {ex.Message}", ex);
            }
        }
    }
}
