using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VisualizationDSA.Application.DTOs.PracticeLadder;
using VisualizationDSA.Application.Services;

namespace VisualizationDSA.Infrastructure.Services
{
    public class Judge0Service : IJudge0Service
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<Judge0Service> _logger;

        public Judge0Service(HttpClient httpClient, IConfiguration config, ILogger<Judge0Service> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            var baseUrl = config["Judge0:BaseUrl"] ?? "http://localhost:2358";
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        private static int MapLanguageId(string language)
        {
            return language.ToLower() switch
            {
                "cpp" => 54,
                "c++" => 54,
                "java" => 62,
                "python" => 71,
                "js" => 63,
                "javascript" => 63,
                "csharp" => 51,
                "c#" => 51,
                _ => 54 // Default to C++
            };
        }

        public async Task<LeetCodeSubmitResponseDto> ExecuteAsync(string sourceCode, string language, IEnumerable<TestCaseDto> testCases)
        {
            int languageId = MapLanguageId(language);

            var testCaseList = testCases.ToList();
            if (testCaseList.Count == 0)
            {
                return new LeetCodeSubmitResponseDto
                {
                    TotalTestcases = 0,
                    PassedTestcases = 0,
                    Passed = false,
                    Result = "CE",
                    CompilerOutput = "Không có test case nào."
                };
            }

            var submissions = testCaseList.Select(tc => new
            {
                source_code = sourceCode,
                language_id = languageId,
                stdin = tc.Input,
                expected_output = tc.ExpectedOutput,
                cpu_time_limit = 5.0,
                memory_limit = 262144
            }).ToList();

            var response = new LeetCodeSubmitResponseDto
            {
                TotalTestcases = testCaseList.Count,
                PassedTestcases = 0,
                Passed = false
            };

            try
            {
                // 1. POST /submissions/batch → nhận tokens
                var batchRes = await _httpClient.PostAsJsonAsync("/submissions/batch?base64_encoded=false", new { submissions });
                batchRes.EnsureSuccessStatusCode();

                var batchBody = await batchRes.Content.ReadFromJsonAsync<BatchSubmissionResponse>();
                if (batchBody == null || batchBody.submissions == null || batchBody.submissions.Count == 0)
                {
                    response.Result = "CE";
                    response.CompilerOutput = "Judge0 không trả token.";
                    return response;
                }

                var tokens = string.Join(",", batchBody.submissions.Select(s => s.token).Where(t => !string.IsNullOrEmpty(t)));

                // 2. Poll /submissions/batch?tokens=... (tối đa 20 lần, mỗi 500ms)
                var statuses = new List<SubmissionResult>();
                for (int attempt = 0; attempt < 20; attempt++)
                {
                    await Task.Delay(500);
                    var statusRes = await _httpClient.GetAsync($"/submissions/batch?tokens={tokens}&base64_encoded=false");
                    if (!statusRes.IsSuccessStatusCode) continue;

                    var statusBody = await statusRes.Content.ReadFromJsonAsync<BatchStatusResponse>();
                    if (statusBody == null || statusBody.submissions == null) continue;

                    var allDone = statusBody.submissions.All(s => s.status != null && s.status.id >= 3); // 3 = In Queue, 4+ = finished
                    statuses = statusBody.submissions;
                    if (allDone) break;
                }

                // 3. Chấm điểm từng test case
                var maxMemoryKb = 0L;
                long totalRuntimeMs = 0;
                int processed = 0;

                for (int i = 0; i < testCaseList.Count && i < statuses.Count; i++)
                {
                    var s = statuses[i];
                    var statusId = s.status?.id ?? 0;
                    maxMemoryKb = Math.Max(maxMemoryKb, s.memory ?? 0);
                    if (s.time != null)
                    {
                        try { totalRuntimeMs += (long)(double.Parse(s.time, System.Globalization.CultureInfo.InvariantCulture) * 1000); } catch { }
                    }
                    processed++;

                    // CE → không tính fail (D10: CE không trừ heart, không tính fail)
                    if (statusId == 6) // Compilation Error
                    {
                        response.Result = "CE";
                        response.CompilerOutput = s.compile_output ?? "Lỗi biên dịch.";
                        response.Passed = false;
                        return response;
                    }

                    if (statusId == 3) // Accepted
                    {
                        response.PassedTestcases++;
                    }
                    else
                    {
                        response.Result = "WA";
                        response.FailedTestcase = i + 1;
                        response.Expected = testCaseList[i].ExpectedOutput;
                        response.Got = s.stdout ?? string.Empty;
                    }
                }

                response.Passed = response.PassedTestcases == testCaseList.Count;
                response.Result = response.Passed ? "AC" : (string.IsNullOrEmpty(response.Result) ? "WA" : response.Result);
                response.Score = response.TotalTestcases == 0 ? 0 : (int)Math.Round((double)response.PassedTestcases / response.TotalTestcases * 100);
                response.RuntimeMs = processed > 0 ? (int)(totalRuntimeMs / processed) : 0;
                response.MemoryKb = (int)maxMemoryKb;
                response.Percentile = response.Passed ? 90 : 40;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing code via Judge0.");
                response.Result = "CE";
                response.CompilerOutput = ex.Message;
            }

            return response;
        }

        private class BatchSubmissionResponse
        {
            public List<SubmissionToken> submissions { get; set; } = new();
        }

        private class SubmissionToken
        {
            public string token { get; set; } = string.Empty;
        }

        private class BatchStatusResponse
        {
            public List<SubmissionResult> submissions { get; set; } = new();
        }

        private class SubmissionResult
        {
            public string token { get; set; } = string.Empty;
            public Judge0Status? status { get; set; }
            public string? stdout { get; set; }
            public string? compile_output { get; set; }
            public string? time { get; set; }
            public long? memory { get; set; }
        }

        private class Judge0Status
        {
            public int id { get; set; }
            public string description { get; set; } = string.Empty;
        }
    }
}
