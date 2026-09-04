using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Persistence.Seed;

/// <summary>Phần 2 — helpers import Algorithms (quiz/lab/code exercises, final test).</summary>
public static partial class SeedGrokkingAlgorithmsData
{
    private const int StageQuiz = 1;
    private const int StageLab = 2;
    private const int StageCode = 3;

    // ── Source lesson (từ JSON) ────────────────────────────────
    private sealed record SourceLesson(string Id, string Title, int ModuleOrder, int OrderIndex, string SandboxType, string ContentMd, string? SandboxConfig);

    private static string? FindSeedJson()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, SeedJsonRelative);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            var local = Path.Combine(dir.FullName, "grokking-algorithms.json");
            if (File.Exists(local))
            {
                return local;
            }

            dir = dir.Parent;
        }

        return null;
    }

    private static string MakeDescription(SourceLesson src)
    {
        return src.SandboxType switch
        {
            "quiz" => "Bài trắc nghiệm kiểm tra nhanh kiến thức — câu hỏi có giải thích tiếng Việt sau khi nộp.",
            "codelab" => "Bài tập lập trình thực hành (Assignment) — viết code giải quyết bài toán thực tế, chấm theo test ẩn.",
            _ => "Bài học lý thuyết trực quan — khái niệm cốt lõi, minh họa động và ví dụ ứng dụng thực tế.",
        };
    }

    // ── Lessons → LessonSimulations ────────────────────────────
    private static async Task SeedLessonSimulationsAsync(
        AppDbContext db, Dictionary<string, Lesson> lessons, ILogger logger, CancellationToken ct)
    {
        foreach (var (lessonTitle, keys) in LabSimKeys)
        {
            if (!lessons.TryGetValue(lessonTitle, out var lesson))
            {
                continue;
            }

            var existing = await db.LessonSimulations
                .Where(s => s.LessonId == lesson.Id)
                .Select(s => s.SimulationKey)
                .ToListAsync(ct);

            var order = 1;
            foreach (var key in keys)
            {
                if (existing.Contains(key))
                {
                    continue;
                }

                db.LessonSimulations.Add(new LessonSimulation
                {
                    LessonId = lesson.Id,
                    SimulationKey = key,
                    Title = $"{lesson.Title} — mô phỏng {key}",
                    SortOrder = order
                });
                order++;
            }
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("SeedGrokkingAlgorithms: LessonSimulations xong (lab keys theo bài)");
    }

    // ── Quiz loading ───────────────────────────────────────────
    private sealed record SourceQuestion(string Text, List<string> Options, int Correct, string? Explanation);

    private static Dictionary<string, List<SourceQuestion>> LoadQuizzes(JsonElement root)
    {
        var result = new Dictionary<string, List<SourceQuestion>>(StringComparer.Ordinal);
        foreach (var q in root.GetProperty("quizzes").EnumerateArray())
        {
            var id = q.GetProperty("id").GetString() ?? string.Empty;
            var title = q.GetProperty("title").GetString() ?? string.Empty;
            var list = new List<SourceQuestion>();
            if (q.TryGetProperty("questions", out var questions))
            {
                foreach (var qq in questions.EnumerateArray())
                {
                    var text = qq.GetProperty("question").GetString() ?? string.Empty;
                    var options = new List<string>();
                    if (qq.TryGetProperty("options", out var opts) && opts.ValueKind == JsonValueKind.String)
                    {
                        try
                        {
                            var arr = JsonSerializer.Deserialize<string[]>(opts.GetString()!) ?? [];
                            options.AddRange(arr);
                        }
                        catch
                        {
                            // bỏ qua câu hỏi lỗi options
                        }
                    }
                    else if (qq.TryGetProperty("options", out var optsArr) && optsArr.ValueKind == JsonValueKind.Array)
                    {
                        options.AddRange(optsArr.EnumerateArray().Select(x => x.GetString() ?? string.Empty));
                    }

                    var correct = qq.TryGetProperty("correctIndex", out var c) && c.TryGetInt32(out var ci) ? ci : -1;
                    var explanation = qq.TryGetProperty("explanation", out var e) ? e.GetString() : null;
                    if (!string.IsNullOrWhiteSpace(text) && options.Count > 0 && correct >= 0 && correct < options.Count)
                    {
                        list.Add(new SourceQuestion(text, options, correct, explanation));
                    }
                }
            }

            // Index theo cả id lẫn title (title cho Final-Quizz truy cập theo manifest).
            result[id] = list;
            if (!result.ContainsKey(title))
            {
                result[title] = list;
            }
        }

        return result;
    }

    private static Question ToQuestion(SourceQuestion q, int sortOrder)
    {
        return new Question
        {
            Type = QuestionType.Single,
            Content = q.Text,
            OptionsJson = JsonSerializer.Serialize(q.Options),
            AnswerJson = $"[{q.Correct}]",
            Explanation = q.Explanation,
            Points = 1,
            SortOrder = sortOrder
        };
    }

    // ── Quiz exercises (MCQ) ───────────────────────────────────
    private static async Task SeedQuizExercisesAsync(
        AppDbContext db, Dictionary<string, Lesson> lessons, Dictionary<string, LearningPathNode> nodes,
        Dictionary<string, List<SourceQuestion>> quizByTitle, int adminId, DateTime now, ILogger logger, CancellationToken ct)
    {
        foreach (var (lessonTitle, lesson) in lessons)
        {
            if (!lesson.Title.StartsWith("Mini-Quizz", StringComparison.Ordinal) || !nodes.TryGetValue(lessonTitle, out var node))
            {
                continue;
            }

            var title = $"Quiz: {lessonTitle}";
            var existing = await db.Exercises
                .Include(e => e.Questions)
                .FirstOrDefaultAsync(e => e.LessonId == lesson.Id && e.Title == title && e.DeletedAt == null, ct);

            // Quiz nguồn gắn theo id (map title → quizId)
            var quizId = FindQuizIdForLesson(lessonTitle);
            var questions = (quizId is not null && quizByTitle.TryGetValue(quizId, out var list) ? list : [])
                .Select((q, i) => ToQuestion(q, i + 1))
                .ToList();

            // Self-heal quizzes created by older seeds with zero questions.
            if (existing is not null)
            {
                if (existing.Questions.Count == 0 && questions.Count > 0)
                {
                    existing.Type = ExerciseType.Mcq;
                    existing.MaxScore = questions.Count;
                    existing.Status = ExerciseStatus.Active;
                    existing.NodeId = node.Id;
                    existing.Questions = questions;
                    await db.SaveChangesAsync(ct);
                    logger.LogInformation("SeedGrokkingAlgorithms: Tự chữa Quiz rỗng {Title} ({Count} câu)", title, questions.Count);
                }
                continue;
            }
            if (questions.Count == 0)
            {
                logger.LogWarning("SeedGrokkingAlgorithms: Quiz bỏ qua {Title} — không có câu hỏi nguồn", title);
                continue;
            }

            db.Exercises.Add(new Exercise
            {
                LessonId = lesson.Id,
                NodeId = node.Id,
                Stage = StageQuiz,
                Title = title,
                Description = $"Trắc nghiệm {lessonTitle} — {questions.Count} câu, giải thích tiếng Việt sau khi nộp.",
                Type = ExerciseType.Mcq,
                MaxScore = questions.Count,
                Status = ExerciseStatus.Active,
                CreatedBy = adminId,
                CreatedAt = now,
                Questions = questions
            });
            await db.SaveChangesAsync(ct);
            logger.LogInformation("SeedGrokkingAlgorithms: Exercises thêm {Title} ({Count} câu)", title, questions.Count);
        }
    }

    private static string? FindQuizIdForLesson(string lessonTitle)
    {
        return lessonTitle switch
        {
            "Mini-Quizz 1: Khái niệm thuật toán" => "q1-concept",
            "Mini-Quizz 2: Big-O" => "q2-bigo",
            "Mini-Quizz 3: Tìm kiếm" => "q3-search",
            "Mini-Quizz 4: Sắp xếp cơ bản" => "q4-sort",
            "Mini-Quizz 5: Đệ quy & chia để trị" => "q5-recursion",
            "Mini-Quizz 6: Heap" => "q6-heap",
            "Mini-Quizz 7: Đồ thị" => "q7-graph",
            "Mini-Quizz 8: Tham lam & QHĐ" => "q8-greedy-dp",
            _ => null,
        };
    }

    // ── Lab exercises (SIMULATION_LAB) ─────────────────────────
    private static async Task SeedLabExercisesAsync(
        AppDbContext db, Dictionary<string, Lesson> lessons, Dictionary<string, LearningPathNode> nodes,
        int adminId, DateTime now, ILogger logger, CancellationToken ct)
    {
        foreach (var (lessonTitle, keys) in LabSimKeys)
        {
            if (!lessons.TryGetValue(lessonTitle, out var lesson) || !nodes.TryGetValue(lessonTitle, out var node))
            {
                continue;
            }

            var title = $"Lab: {lessonTitle}";
            var existing = await db.Exercises.FirstOrDefaultAsync(
                e => e.LessonId == lesson.Id && e.Title == title && e.DeletedAt == null, ct);
            if (existing is not null)
            {
                continue;
            }

            var simulationKey = keys[0];
            var exercise = new Exercise
            {
                LessonId = lesson.Id,
                NodeId = node.Id,
                Stage = StageLab,
                Title = title,
                Description = $"Mô phỏng {simulationKey} từng bước trên canvas — thực hành trực quan thuật toán.",
                Type = ExerciseType.SimulationLab,
                ConfigJson = JsonSerializer.Serialize(new { simulationKey, maxSteps = 10 }),
                MaxScore = 10,
                Status = ExerciseStatus.Active,
                CreatedBy = adminId,
                CreatedAt = now
            };
            db.Exercises.Add(exercise);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("SeedGrokkingAlgorithms: Exercises thêm {Title} (key={Key})", title, simulationKey);
        }
    }

    // ── Assignment exercises (CODE — testCases từ sandboxConfig nguồn) ──
    private static async Task SeedAssignmentExercisesAsync(
        AppDbContext db, Dictionary<string, Lesson> lessons, Dictionary<string, LearningPathNode> nodes,
        int adminId, DateTime now, ILogger logger, CancellationToken ct)
    {
        foreach (var (lessonTitle, lesson) in lessons)
        {
            if (!lesson.Title.StartsWith("Assignment", StringComparison.Ordinal) || !nodes.TryGetValue(lessonTitle, out var node))
            {
                continue;
            }

            var title = $"Code: {lessonTitle}";
            var existing = await db.Exercises.FirstOrDefaultAsync(
                e => e.LessonId == lesson.Id && e.Title == title && e.DeletedAt == null, ct);
            if (existing is not null)
            {
                var freshConfigJson = JsonSerializer.Serialize(BuildAssignmentConfig(lessonTitle));
                if (existing.ConfigJson != freshConfigJson)
                {
                    existing.ConfigJson = freshConfigJson;
                    await db.SaveChangesAsync(ct);
                    logger.LogInformation("SeedGrokkingAlgorithms: Exercises tự chữa ConfigJson {Title} (Id={Id})", title, existing.Id);
                }
                continue;
            }

            var config = BuildAssignmentConfig(lessonTitle);
            var exercise = new Exercise
            {
                LessonId = lesson.Id,
                NodeId = node.Id,
                Stage = StageCode,
                Title = title,
                Description = $"Assignment {lessonTitle} — code giải bài toán thực tế, chấm theo test ẩn (pass ≥ 70%).",
                Type = ExerciseType.Code,
                ConfigJson = JsonSerializer.Serialize(config),
                MaxScore = 100,
                Status = ExerciseStatus.Active,
                CreatedBy = adminId,
                CreatedAt = now
            };
            db.Exercises.Add(exercise);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("SeedGrokkingAlgorithms: Exercises thêm {Title} (code)", title);
        }
    }

    private static object? BuildAssignmentConfig(string lessonTitle)
    {
        var jsonPath = FindSeedJson();
        if (jsonPath is null)
        {
            return new { signature = $"Hoàn thành {lessonTitle}", language = "javascript", testCases = Array.Empty<object>() };
        }

        using var doc = JsonDocument.Parse(File.ReadAllText(jsonPath, new UTF8Encoding(false)));
        foreach (var l in doc.RootElement.GetProperty("lessons").EnumerateArray())
        {
            if (l.GetProperty("title").GetString() != lessonTitle || !l.TryGetProperty("sandboxConfig", out var sc) || sc.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            try
            {
                var config = JsonNode.Parse(sc.GetString()!);
                if (config is JsonArray tasksArray)
                {
                    return ConvertTasks(tasksArray, lessonTitle);
                }

                if (config is JsonObject obj)
                {
                    // Config là 1 TASK ĐƠN → bọc thành mảng 1 phần tử (như Grokking Assignment 3/4)
                    return ConvertTasks(new JsonArray(obj.DeepClone()), lessonTitle);
                }
            }
            catch
            {
                // config hỏng → exercise vẫn tạo với signature đơn giản
            }
        }

        return new { signature = $"Hoàn thành {lessonTitle}", language = "javascript", testCases = Array.Empty<object>() };
    }

    private static object ConvertTasks(JsonArray tasks, string lessonTitle)
    {
        var converted = new List<object>();
        foreach (var t in tasks)
        {
            if (t is not JsonObject task)
            {
                continue;
            }

            var entry = task["entryFunction"]?.GetValue<string>() ?? "solve";
            var testCases = new List<object>();
            if (task["testCases"] is JsonArray tests)
            {
                foreach (var tc in tests)
                {
                    if (tc is not JsonObject test)
                    {
                        continue;
                    }

                    testCases.Add(new
                    {
                        name = $"{entry}: {test["input"]?.ToJsonString() ?? "?"}",
                        input = test["input"]?.GetValue<string>() ?? "",
                        expectedOutput = test["expectedOutput"]?.GetValue<string>() ?? "null",
                        isHidden = test.TryGetPropertyValue("isHidden", out var h) && h?.GetValue<bool>() == true
                    });
                }
            }

            var hints = new List<string>();
            if (task["hints"] is JsonArray hintArr)
            {
                foreach (var h in hintArr)
                {
                    if (h?.GetValue<string>() is { } hint)
                    {
                        hints.Add(hint);
                    }
                }
            }

            converted.Add(new
            {
                id = task["id"]?.GetValue<string>() ?? Guid.NewGuid().ToString("N"),
                title = task["title"]?.GetValue<string>() ?? lessonTitle,
                description = task["description"]?.GetValue<string>() ?? string.Empty,
                initialCode = task["initialCode"]?.GetValue<string>() ?? string.Empty,
                solution = string.Empty,
                entryFunction = entry,
                hints,
                testCases
            });
        }

        return converted;
    }

    // ── Final-Quizz (20 câu tổng hợp — 2-3 câu/module theo manifest) ──
    private static readonly (string QuizId, int Take)[] FinalQuizzModuleQuizzes =
    [
        ("q1-concept", 2),
        ("q2-bigo", 3),
        ("q3-search", 3),
        ("q4-sort", 3),
        ("q5-recursion", 2),
        ("q6-heap", 2),
        ("q7-graph", 3),
        ("q8-greedy-dp", 2),
    ];

    private static async Task SeedFinalQuizzAsync(
        AppDbContext db, LearningPath path, LearningPathNode practiceNode,
        Dictionary<string, Lesson> lessons, Dictionary<string, List<SourceQuestion>> quizByTitle,
        int adminId, DateTime now, ILogger logger, CancellationToken ct)
    {
        const string title = "Final-Quizz: Grokking Algorithms";
        var existing = await db.Exercises.FirstOrDefaultAsync(
            e => e.NodeId == practiceNode.Id && e.Title == title && e.DeletedAt == null, ct);
        if (existing is not null)
        {
            return;
        }

        var picked = new List<Question>();
        var sort = 1;
        foreach (var (quizId, take) in FinalQuizzModuleQuizzes)
        {
            if (!quizByTitle.TryGetValue(quizId, out var questions))
            {
                continue;
            }

            foreach (var q in questions.Take(take))
            {
                picked.Add(ToQuestion(q, sort++));
            }
        }

        if (picked.Count == 0)
        {
            logger.LogWarning("SeedGrokkingAlgorithms: Final-Quizz bỏ qua — không có câu hỏi nguồn");
            return;
        }

        var lastLesson = lessons.Values
            .Where(l => l.Title.StartsWith("Bài 16", StringComparison.Ordinal))
            .FirstOrDefault();
        var lessonId = lastLesson?.Id ?? lessons.Values.FirstOrDefault(l => l.Title.StartsWith("Bài", StringComparison.Ordinal))?.Id ?? 0;

        db.Exercises.Add(new Exercise
        {
            LessonId = lessonId,
            NodeId = practiceNode.Id,
            Stage = StageQuiz,
            Title = title,
            Description = $"Final-Quizz {path.Title} — {picked.Count} câu tổng hợp bao quát 8 module (2-3 câu/module), pass ≥ 70%.",
            Type = ExerciseType.Mcq,
            MaxScore = picked.Sum(q => q.Points),
            Status = ExerciseStatus.Active,
            CreatedBy = adminId,
            CreatedAt = now,
            Questions = picked
        });
        await db.SaveChangesAsync(ct);
        logger.LogInformation("SeedGrokkingAlgorithms: Final-Quizz thêm {Title} ({Count} câu)", title, picked.Count);
    }

    // ── Kiểm tra cuối lộ trình (3 bài code phong cách Assignment) ──
    private static async Task SeedFinalCodingTestAsync(
        AppDbContext db, LearningPath path, LearningPathNode finalNode,
        Dictionary<string, Lesson> lessons, int adminId, DateTime now, ILogger logger, CancellationToken ct)
    {
        const string title = "Kiểm tra cuối: Grokking Algorithms";
        var existing = await db.Exercises.FirstOrDefaultAsync(
            e => e.NodeId == finalNode.Id && e.Title == title && e.DeletedAt == null, ct);

        var tasks = BuildFinalCodingTasks();
        var configJson = JsonSerializer.Serialize(tasks);
        var lessonId = lessons.Values.FirstOrDefault(l => l.Title.StartsWith("Bài 16", StringComparison.Ordinal))?.Id ?? 0;

        if (existing is null)
        {
            var exercise = new Exercise
            {
                LessonId = lessonId,
                NodeId = finalNode.Id,
                Stage = StageCode,
                Title = title,
                Description = $"Kiểm tra cuối {path.Title} — 3 bài code tổng hợp 8 module (Monaco + testcase, pass tất cả test để hoàn thành).",
                Type = ExerciseType.Code,
                ConfigJson = configJson,
                MaxScore = 100,
                Status = ExerciseStatus.Active,
                CreatedBy = adminId,
                CreatedAt = now
            };
            db.Exercises.Add(exercise);
            await db.SaveChangesAsync(ct);

            finalNode.FinalTestId = exercise.Id;
            await db.SaveChangesAsync(ct);
            logger.LogInformation("SeedGrokkingAlgorithms: kiểm tra cuối thêm {Title} (Id={Id}, {Count} bài code)", title, exercise.Id, tasks.Count);
            return;
        }

        if (existing.Type == ExerciseType.Mcq)
        {
            db.Questions.RemoveRange(db.Questions.Where(q => q.ExerciseId == existing.Id));
            existing.Type = ExerciseType.Code;
            existing.Stage = StageCode;
            existing.ConfigJson = configJson;
            existing.MaxScore = 100;
            existing.Description = $"Kiểm tra cuối {path.Title} — 3 bài code tổng hợp 8 module (Monaco + testcase, pass tất cả test để hoàn thành).";
            await db.SaveChangesAsync(ct);
            logger.LogInformation("SeedGrokkingAlgorithms: kiểm tra cuối chuyển MCQ → CODE ({Title}, Id={Id}, {Count} bài code)", title, existing.Id, tasks.Count);
        }
        else if (existing.ConfigJson != configJson)
        {
            existing.ConfigJson = configJson;
            await db.SaveChangesAsync(ct);
            logger.LogInformation("SeedGrokkingAlgorithms: kiểm tra cuối cập nhật ConfigJson ({Title}, Id={Id})", title, existing.Id);
        }
    }

    /// <summary>3 bài code kiểm tra cuối — mỗi bài đại diện cho nhóm module (tìm kiếm · sắp xếp/đệ quy · đồ thị/heap).</summary>
    private static List<object> BuildFinalCodingTasks()
    {
        return
        [
            new
            {
                id = "alg-final-1-binary-search",
                title = "Tìm kiếm nhị phân (Binary Search)",
                description = "MODULE 3 — Tìm kiếm.\n\n" +
                    "Viết hàm `binarySearch(arr, target)` trả về chỉ số của `target` trong mảng ĐÃ SẮP XẾP tăng dần, dùng thuật toán chia đôi (O(log n)). Trả về `-1` nếu không tồn tại.\n\n" +
                    "Ví dụ: `binarySearch([1,3,5,7,9], 7)` → `3`; `binarySearch([1,3,5,7,9], 2)` → `-1`.",
                initialCode = """
                    function binarySearch(arr, target) {
                      let lo = 0;
                      let hi = arr.length - 1;
                      // TODO: lặp với lo <= hi, tìm mid, so sánh arr[mid] với target
                      return -1;
                    }
                    """,
                solution = string.Empty,
                entryFunction = "binarySearch",
                hints = new[]
                {
                    "Mid = Math.floor((lo + hi) / 2).",
                    "Nếu arr[mid] === target → trả về mid.",
                    "Nếu arr[mid] < target → lo = mid + 1; ngược lại hi = mid - 1.",
                    "Dừng khi lo > hi → trả về -1.",
                },
                testCases = new object[]
                {
                    new { name = "binarySearch: ở giữa", input = "[[1,3,5,7,9],7]", expectedOutput = "3", isHidden = false },
                    new { name = "binarySearch: không có", input = "[[1,3,5,7,9],2]", expectedOutput = "-1", isHidden = false },
                    new { name = "binarySearch: ở đầu", input = "[[1,3,5,7,9],1]", expectedOutput = "0", isHidden = false },
                    new { name = "binarySearch: rỗng", input = "[[],5]", expectedOutput = "-1", isHidden = false },
                    new { name = "binarySearch: 1 phần tử", input = "[[42],42]", expectedOutput = "0", isHidden = true },
                    new { name = "binarySearch: mảng dài", input = "[[1,2,3,4,5,6,7,8,9,10],10]", expectedOutput = "9", isHidden = true },
                }
            },
            new
            {
                id = "alg-final-2-merge-sort",
                title = "Sắp xếp trộn (Merge Sort)",
                description = "MODULE 5 — Đệ quy & Chia để trị.\n\n" +
                    "Viết hàm `mergeSort(arr)` sắp xếp mảng tăng dần bằng thuật toán chia để trị (chia đôi → sắp từng nửa → trộn). Trả về mảng mới đã sắp xếp.\n\n" +
                    "Ví dụ: `mergeSort([5,3,8,1])` → `[1,3,5,8]`.",
                initialCode = """
                    function mergeSort(arr) {
                      if (arr.length <= 1) return arr;
                      const mid = Math.floor(arr.length / 2);
                      const left = mergeSort(arr.slice(0, mid));
                      const right = mergeSort(arr.slice(mid));
                      return merge(left, right);
                    }

                    function merge(a, b) {
                      // TODO: trộn 2 mảng đã sắp xếp thành mảng tăng dần
                      return [];
                    }
                    """,
                solution = string.Empty,
                entryFunction = "mergeSort",
                hints = new[]
                {
                    "Trong merge: dùng 2 con trỏ i, j; mỗi bước đưa phần tử nhỏ hơn vào result.",
                    "Sau vòng lặp chính, nối nốt phần còn lại của a và b.",
                    "Merge Sort: O(n log n) thời gian, O(n) bộ nhớ.",
                },
                testCases = new object[]
                {
                    new { name = "mergeSort: cơ bản", input = "[[5,3,8,1]]", expectedOutput = "[1,3,5,8]", isHidden = false },
                    new { name = "mergeSort: rỗng", input = "[[]]", expectedOutput = "[]", isHidden = false },
                    new { name = "mergeSort: 1 phần tử", input = "[[7]]", expectedOutput = "[7]", isHidden = false },
                    new { name = "mergeSort: giảm dần", input = "[[5,4,3,2,1]]", expectedOutput = "[1,2,3,4,5]", isHidden = true },
                    new { name = "mergeSort: trùng", input = "[[3,1,3,1]]", expectedOutput = "[1,1,3,3]", isHidden = true },
                    new { name = "mergeSort: âm", input = "[[-3,7,-1,0]]", expectedOutput = "[-3,-1,0,7]", isHidden = true },
                }
            },
            new
            {
                id = "alg-final-3-topk-heap",
                title = "Top-K lớn nhất (Heap)",
                description = "MODULE 6 — Heap.\n\n" +
                    "Viết hàm `topK(nums, k)` trả về **K phần tử LỚN NHẤT** của mảng (kết quả tăng dần). Gợi ý dùng min-heap kích thước k — tối ưu O(n log k).\n\n" +
                    "Ví dụ: `topK([10,3,9,7,12,5], 2)` → `[10,12]`.",
                initialCode = """
                    function topK(nums, k) {
                      const heap = []; // min-heap kích thước k
                      // TODO: duyệt nums, giữ k phần tử lớn nhất trong heap
                      return heap.sort((a, b) => a - b);
                    }
                    """,
                solution = string.Empty,
                entryFunction = "topK",
                hints = new[]
                {
                    "Heap chưa đủ k → chèn thẳng (bubble up).",
                    "Heap đủ k và x > heap[0] → thay phần tử nhỏ nhất (extract + insert).",
                    "Kết quả trả về tăng dần.",
                },
                testCases = new object[]
                {
                    new { name = "topK: k=2", input = "[[10,3,9,7,12,5],2]", expectedOutput = "[10,12]", isHidden = false },
                    new { name = "topK: k=1", input = "[[1,5,3,9,2],1]", expectedOutput = "[9]", isHidden = false },
                    new { name = "topK: k=len", input = "[[4,2,8,6],4]", expectedOutput = "[2,4,6,8]", isHidden = false },
                    new { name = "topK: trùng", input = "[[7,7,3,7],2]", expectedOutput = "[7,7]", isHidden = true },
                    new { name = "topK: âm", input = "[[-5,-1,-9,-3],2]", expectedOutput = "[-3,-1]", isHidden = true },
                }
            },
        ];
    }
}