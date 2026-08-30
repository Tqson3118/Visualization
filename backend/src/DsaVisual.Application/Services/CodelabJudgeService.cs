using System.Text.Json;
using Jint;
using Jint.Runtime;

namespace DsaVisual.Application.Services;

/// <summary>Test case của 1 task code (từ ConfigJson của exercise — khớp CodeLabTask FE).</summary>
public sealed record CodelabTestCaseSpec(string Input, string ExpectedOutput, bool IsHidden);

/// <summary>Task code (một bài con trong ConfigJson dạng mảng — Assignment / Kiểm tra cuối Grokking).</summary>
public sealed record CodelabTaskSpec(string Id, string EntryFunction, IReadOnlyList<CodelabTestCaseSpec> TestCases);

/// <summary>Kết quả 1 test case do MÁY CHỦ chấm.</summary>
public sealed record CodelabCaseResult(bool Passed, string? Error);

public sealed record CodelabJudgeResult(
    bool CompileError,
    string? CompileErrorText,
    bool TimedOut,
    string? TimeoutError,
    IReadOnlyList<CodelabCaseResult> Cases);

/// <summary>
/// Chấm code bài tập CODE PHÍA MÁY CHỦ bằng Jint (JS interpreter thuần .NET, sandboxed):
/// - Không cho truy cập CLR (mặc định), giới hạn thời gian / số statement / bộ nhớ / đệ quy —
///   code học viên vòng lặp vô hạn hay đệ quy sâu không làm treo server.
/// - Chấm GIỐNG FE codelabExecutor: parse `input` (JSON mảng tham số) → gọi entryFunction →
///   JSON.stringify kết quả → so sánh đã bỏ khoảng trắng với `expectedOutput`.
/// Nghiệp vụ (yêu cầu 15/08): bài ASM chỉ PASS khi code chạy ĐÚNG trên máy chủ — không tin client.
/// </summary>
public sealed class CodelabJudgeService
{
    public const int DefaultTimeoutMs = 1500;
    private const int MaxStatements = 200_000;
    private const long MaxMemoryBytes = 32 * 1024 * 1024;

    /// <summary>Đọc danh sách task từ ConfigJson — CHỈ khi top-level là JSON ARRAY (định dạng Grokking);
    /// config kiểu object cũ (signature/testCases — các lộ trình ẩn) → trả null → giữ hành vi client-declared.</summary>
    public static IReadOnlyList<CodelabTaskSpec>? TryParseTasks(string? configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(configJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            var tasks = new List<CodelabTaskSpec>();
            foreach (var t in doc.RootElement.EnumerateArray())
            {
                if (t.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                var id = t.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
                var entry = t.TryGetProperty("entryFunction", out var efProp) && efProp.ValueKind == JsonValueKind.String
                    ? efProp.GetString()!
                    : "solution";

                var tests = new List<CodelabTestCaseSpec>();
                if (t.TryGetProperty("testCases", out var tcProp) && tcProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var tc in tcProp.EnumerateArray())
                    {
                        var input = tc.TryGetProperty("input", out var i) && i.ValueKind == JsonValueKind.String ? i.GetString() : null;
                        var expected = tc.TryGetProperty("expectedOutput", out var e) && e.ValueKind == JsonValueKind.String ? e.GetString() : null;
                        var hidden = tc.TryGetProperty("isHidden", out var h) && h.ValueKind == JsonValueKind.True;
                        if (input is null || expected is null)
                        {
                            continue;
                        }

                        tests.Add(new CodelabTestCaseSpec(input, expected, hidden));
                    }
                }

                if (id is not null && tests.Count > 0)
                {
                    tasks.Add(new CodelabTaskSpec(id, entry, tests));
                }
            }

            return tasks.Count > 0 ? tasks : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>Bỏ toàn bộ khoảng trắng để so sánh mảng/chuỗi linh hoạt — khớp normalizeOutput của FE.</summary>
    private static string Normalize(string raw) => raw.Replace(" ", string.Empty).Replace("\t", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty);

    public CodelabJudgeResult Judge(string code, CodelabTaskSpec task, int timeoutMs = DefaultTimeoutMs)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return new CodelabJudgeResult(true, "Mã nguồn trống.", false, null, []);
        }

        var engine = new Engine(options =>
        {
            options.TimeoutInterval(TimeSpan.FromMilliseconds(timeoutMs));
            options.MaxStatements(MaxStatements);
            options.LimitMemory(MaxMemoryBytes);
            options.Constraints.StackOverflowGuard = true;
        });

        // 1) Biên dịch code học viên (định nghĩa entryFunction + helper)
        try
        {
            engine.Execute(code);
        }
        catch (JavaScriptException ex)
        {
            return new CodelabJudgeResult(true, $"Lỗi biên dịch code: {ex.Message}", false, null, []);
        }
        catch (TimeoutException)
        {
            return new CodelabJudgeResult(false, null, true, $"Hết thời gian chạy ({timeoutMs}ms) — code có thể bị vòng lặp vô hạn!", []);
        }
        catch (StatementsCountOverflowException)
        {
            return new CodelabJudgeResult(false, null, true, $"Vượt quá giới hạn {MaxStatements:N0} lệnh — nghi vòng lặp vô hạn!", []);
        }
        catch (MemoryLimitExceededException)
        {
            return new CodelabJudgeResult(false, null, false, "Code vượt giới hạn bộ nhớ (32MB) — bị huỷ.", []);
        }
        catch (RecursionDepthOverflowException)
        {
            return new CodelabJudgeResult(false, null, false, "Code đệ quy quá sâu — bị huỷ.", []);
        }

        // 2) Chạy từng test case: input có thể là JS array literal hoặc giá trị đơn
        var cases = new List<CodelabCaseResult>();
        foreach (var tc in task.TestCases)
        {
            try
            {
                var inputTrimmed = tc.Input?.Trim() ?? string.Empty;
                var callExpr = string.IsNullOrEmpty(inputTrimmed)
                    ? $"{task.EntryFunction}()"
                    : (inputTrimmed.StartsWith('[') && inputTrimmed.EndsWith(']'))
                        ? $"{task.EntryFunction}(...{inputTrimmed})"
                        : $"{task.EntryFunction}({inputTrimmed})";

                var actual = engine.Evaluate($"JSON.stringify({callExpr})");
                var actualText = actual.IsUndefined() ? string.Empty : actual.AsString();
                var passed = Normalize(actualText) == Normalize(tc.ExpectedOutput);
                cases.Add(new CodelabCaseResult(passed, null));
            }
            catch (JavaScriptException ex)
            {
                cases.Add(new CodelabCaseResult(false, ex.Message));
            }
            catch (TimeoutException)
            {
                return new CodelabJudgeResult(false, null, true, $"Hết thời gian chạy ({timeoutMs}ms) — code có thể bị vòng lặp vô hạn!", cases);
            }
            catch (StatementsCountOverflowException)
            {
                return new CodelabJudgeResult(false, null, true, $"Vượt quá giới hạn {MaxStatements:N0} lệnh — nghi vòng lặp vô hạn!", cases);
            }
            catch (MemoryLimitExceededException)
            {
                return new CodelabJudgeResult(false, null, false, "Code vượt giới hạn bộ nhớ (32MB) — bị huỷ.", cases);
            }
            catch (RecursionDepthOverflowException)
            {
                return new CodelabJudgeResult(false, null, false, "Code đệ quy quá sâu — bị huỷ.", cases);
            }
            catch (Exception ex)
            {
                cases.Add(new CodelabCaseResult(false, ex.Message));
            }
        }

        return new CodelabJudgeResult(false, null, false, null, cases);
    }
}
