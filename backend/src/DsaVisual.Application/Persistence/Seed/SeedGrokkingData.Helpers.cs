using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Persistence.Seed;

/// <summary>Phần 2 — helpers import Grokking (quiz/lab/code exercises, final test, sanitizer mở rộng).</summary>
public static partial class SeedGrokkingData
{
    private const int StageQuiz = 1;
    private const int StageLab = 2;
    private const int StageCode = 3;

    // ── Find seed JSON ─────────────────────────────────────────

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

            // fallback: file nằm ngay trong thư mục chạy (test)
            var local = Path.Combine(dir.FullName, "grokking-course.json");
            if (File.Exists(local))
            {
                return local;
            }

            dir = dir.Parent;
        }

        return null;
    }

    // ── Description cho lesson theo loại ───────────────────────

    private static string MakeDescription(SourceLesson src)
    {
        return src.SandboxType switch
        {
            "quiz" => "Bài trắc nghiệm kiểm tra nhanh kiến thức — câu hỏi có giải thích tiếng Việt sau khi nộp.",
            "codelab" => "Bài tập lập trình thực hành (Assignment) — viết code giải quyết bài toán thực tế, chấm theo test ẩn.",
            _ => "Bài học lý thuyết trực quan — khái niệm cốt lõi, minh họa từng bước và ví dụ ứng dụng thực tế.",
        };
    }

    // ── Lessons → LessonSimulations (lab keys) ─────────────────

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
        logger.LogInformation("SeedGrokking: LessonSimulations xong (lab keys theo bài)");
    }

    // ── Load quizzes từ JSON nguồn ─────────────────────────────

    private sealed record SourceQuestion(string Text, List<string> Options, int Correct, string? Explanation);

    private static Dictionary<string, List<SourceQuestion>> LoadQuizzes(JsonElement root)
    {
        var result = new Dictionary<string, List<SourceQuestion>>(StringComparer.Ordinal);
        foreach (var q in root.GetProperty("quizzes").EnumerateArray())
        {
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

            result[title] = list;
        }

        return result;
    }

    private static List<SourceQuestion> GetQuizForLesson(
        string lessonTitle, Dictionary<string, List<SourceQuestion>> quizByTitle)
    {
        var matched = QuizMap.FirstOrDefault(m => m.LessonTitle == lessonTitle);
        if (matched.LessonTitle is null)
        {
            return [];
        }

        return quizByTitle.TryGetValue(matched.QuizTitle, out var list) ? list : [];
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
        foreach (var (lessonTitle, _) in QuizMap)
        {
            if (!lessons.TryGetValue(lessonTitle, out var lesson) || !nodes.TryGetValue(lessonTitle, out var node))
            {
                continue;
            }

            var title = $"Quiz: {lessonTitle}";
            var existing = await db.Exercises.FirstOrDefaultAsync(
                e => e.LessonId == lesson.Id && e.Title == title && e.DeletedAt == null, ct);
            if (existing is not null)
            {
                continue;
            }

            var questions = GetQuizForLesson(lessonTitle, quizByTitle)
                .Select((q, i) => ToQuestion(q, i + 1))
                .ToList();
            if (questions.Count == 0)
            {
                logger.LogWarning("SeedGrokking: Quiz bỏ qua {Title} — không có câu hỏi nguồn", title);
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
            logger.LogInformation("SeedGrokking: Exercises thêm {Title} ({Count} câu)", title, questions.Count);
        }
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
                Description = $"Mô phỏng {simulationKey} từng bước trên canvas — thực hành trực quan cấu trúc dữ liệu.",
                Type = ExerciseType.SimulationLab,
                ConfigJson = JsonSerializer.Serialize(new { simulationKey, maxSteps = 10 }),
                MaxScore = 10,
                Status = ExerciseStatus.Active,
                CreatedBy = adminId,
                CreatedAt = now
            };
            db.Exercises.Add(exercise);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("SeedGrokking: Exercises thêm {Title} (key={Key})", title, simulationKey);
        }
    }

    // ── Assignment exercises (CODE — test từ sandboxConfig nguồn) ──

    private static async Task SeedAssignmentExercisesAsync(
        AppDbContext db, Dictionary<string, Lesson> lessons, Dictionary<string, LearningPathNode> nodes,
        int adminId, DateTime now, ILogger logger, CancellationToken ct)
    {
        foreach (var (lessonTitle, simKey) in AssignmentSimKeys)
        {
            if (!lessons.TryGetValue(lessonTitle, out var lesson) || !nodes.TryGetValue(lessonTitle, out var node))
            {
                continue;
            }

            var title = $"Code: {lessonTitle}";
            var existing = await db.Exercises.FirstOrDefaultAsync(
                e => e.LessonId == lesson.Id && e.Title == title && e.DeletedAt == null, ct);
            if (existing is not null)
            {
                // Tự chữa (self-healing): ConfigJson lệch với định nghĩa hiện tại (vd bản cũ bị
                // fallback rỗng vì config task đơn chưa được xử lý) → cập nhật lại — idempotent.
                var freshConfigJson = JsonSerializer.Serialize(BuildAssignmentConfig(lessonTitle, simKey));
                if (existing.ConfigJson != freshConfigJson)
                {
                    existing.ConfigJson = freshConfigJson;
                    await db.SaveChangesAsync(ct);
                    logger.LogInformation("SeedGrokking: Exercises tự chữa ConfigJson {Title} (Id={Id})", title, existing.Id);
                }

                continue;
            }

            var config = BuildAssignmentConfig(lessonTitle, simKey);
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
            logger.LogInformation("SeedGrokking: Exercises thêm {Title} (code, simKey={Key})", title, simKey);
        }
    }

    private static object? BuildAssignmentConfig(string lessonTitle, string simKey)
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
                    if (obj["tasks"] is JsonArray tasks)
                    {
                        return ConvertTasks(tasks, lessonTitle);
                    }

                    // Config là 1 TASK ĐƠN (Assignment 3/4: { id, description, initialCode,
                    // entryFunction, testCases, hints }) → bọc thành mảng 1 phần tử để FE
                    // LessonStepCodeLab + máy chủ chấm (Jint) xử lý thống nhất.
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

            // Giữ đầy đủ task nguồn (description/initialCode/entryFunction/hints) — đúng CodeLabTask FE
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

    // ── Final-Quizz (20 câu trắc nghiệm tổng hợp — 5 câu/module) ──

    /// <summary>Quiz tổng hợp nguồn của từng module — mỗi module lấy 5 câu đại diện → 40 câu bao quát 8 module.</summary>
    private static readonly (string QuizTitle, int Take)[] FinalQuizzModuleQuizzes =
    [
        ("Trắc nghiệm Tổng Hợp: Array & Linked List", 5),        // Module 1: Array & Linked List
        ("Trắc nghiệm Tổng Hợp: Stack & Queue", 5),              // Module 2: Stack & Queue (LIFO & FIFO)
        ("Trắc nghiệm Tổng Hợp: Hash Map & Hash Set", 5),        // Module 3: Hash Map & Hash Set
        ("Trắc nghiệm Tổng Hợp: Tree & Binary Search Tree", 5),  // Module 4: Tree & BST
        ("Trắc nghiệm Tổng Hợp: BST & AVL", 5),                  // Module 5: BST & AVL
        ("Trắc nghiệm Tổng Hợp: Heap & Priority Queue", 5),      // Module 6: Heap & Priority Queue
        ("Trắc nghiệm Tổng Hợp: Đồ thị", 5),                     // Module 7: Đồ thị
        ("Trắc nghiệm Tổng Hợp: Trie & Set/Map", 5),             // Module 8: Trie & Set/Map
    ];

    private static async Task SeedFinalQuizzAsync(
        AppDbContext db, LearningPath path, LearningPathNode practiceNode,
        Dictionary<string, Lesson> lessons, Dictionary<string, List<SourceQuestion>> quizByTitle,
        int adminId, DateTime now, ILogger logger, CancellationToken ct)
    {
        const string title = "Final-Quizz: Grokking Data Structures";
        var existing = await db.Exercises.FirstOrDefaultAsync(
            e => e.NodeId == practiceNode.Id && e.Title == title && e.DeletedAt == null, ct);
        if (existing is not null)
        {
            return;
        }

        var picked = new List<Question>();
        var sort = 1;
        foreach (var (quizTitle, take) in FinalQuizzModuleQuizzes)
        {
            if (!quizByTitle.TryGetValue(quizTitle, out var questions))
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
            logger.LogWarning("SeedGrokking: Final-Quizz bỏ qua — không có câu hỏi nguồn");
            return;
        }

        db.Exercises.Add(new Exercise
        {
            // LessonId NOT NULL → gắn bài học cuối cùng của khóa (tương tự final test cũ);
            // FE lấy exercise theo NodeId (node Final-Quizz).
            LessonId = lessons["Bài 8: Binary Search Tree"].Id,
            NodeId = practiceNode.Id,
            Stage = StageQuiz,
            Title = title,
            Description = $"Final-Quizz {path.Title} — {picked.Count} câu tổng hợp bao quát 8 module (mỗi module 5 câu), pass ≥ 70%.",
            Type = ExerciseType.Mcq,
            MaxScore = picked.Sum(q => q.Points),
            Status = ExerciseStatus.Active,
            CreatedBy = adminId,
            CreatedAt = now,
            Questions = picked
        });
        await db.SaveChangesAsync(ct);
        logger.LogInformation("SeedGrokking: Final-Quizz thêm {Title} ({Count} câu)", title, picked.Count);
    }

    // ── Kiểm tra cuối lộ trình (3 bài code theo phong cách Assignment — Monaco + testcase) ──

    private static async Task SeedFinalCodingTestAsync(
        AppDbContext db, LearningPath path, LearningPathNode finalNode,
        Dictionary<string, Lesson> lessons, int adminId, DateTime now, ILogger logger, CancellationToken ct)
    {
        const string title = "Kiểm tra cuối: Grokking Data Structures";
        var existing = await db.Exercises.FirstOrDefaultAsync(
            e => e.NodeId == finalNode.Id && e.Title == title && e.DeletedAt == null, ct);

        var tasks = BuildFinalCodingTasks();
        var configJson = JsonSerializer.Serialize(tasks);
        var lessonId = lessons["Bài 8: Binary Search Tree"].Id;

        if (existing is null)
        {
            var exercise = new Exercise
            {
                LessonId = lessonId,
                NodeId = finalNode.Id,
                Stage = StageCode,
                Title = title,
                Description = $"Kiểm tra cuối {path.Title} — 3 bài code tổng hợp 4 module (Monaco + testcase, pass tất cả test để hoàn thành).",
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
            logger.LogInformation("SeedGrokking: kiểm tra cuối thêm {Title} (Id={Id}, {Count} bài code)", title, exercise.Id, tasks.Count);
            return;
        }

        // DB cũ: final test là MCQ 5 câu → CHUYỂN sang CODE 3 bài ngay trên exercise cũ
        // (giữ nguyên Id để FinalTestId + submissions không vỡ) — idempotent (lần sau Type=Code → bỏ qua).
        if (existing.Type == ExerciseType.Mcq)
        {
            db.Questions.RemoveRange(db.Questions.Where(q => q.ExerciseId == existing.Id));
            existing.Type = ExerciseType.Code;
            existing.Stage = StageCode;
            existing.ConfigJson = configJson;
            existing.MaxScore = 100;
            existing.Description = $"Kiểm tra cuối {path.Title} — 3 bài code tổng hợp 4 module (Monaco + testcase, pass tất cả test để hoàn thành).";
            await db.SaveChangesAsync(ct);
            logger.LogInformation("SeedGrokking: kiểm tra cuối chuyển MCQ → CODE ({Title}, Id={Id}, {Count} bài code)", title, existing.Id, tasks.Count);
        }
        else if (existing.ConfigJson != configJson)
        {
            // Tự chữa (self-healing): ConfigJson lệch với định nghĩa hiện tại (vd đổi title/nội dung
            // bài code) → cập nhật lại — idempotent (không đổi gì khi khớp).
            existing.ConfigJson = configJson;
            await db.SaveChangesAsync(ct);
            logger.LogInformation("SeedGrokking: kiểm tra cuối cập nhật ConfigJson ({Title}, Id={Id})", title, existing.Id);
        }
    }

    /// <summary>3 bài code kiểm tra cuối — mỗi bài tượng trưng cho nội dung các module;
    /// gộp chung phủ hết 4 module của khóa (Module 1: Linked List · Module 2: Stack ·
    /// Module 3+4: Hash Map & BST). Format khớp CodeLabTask[] mà FE LessonStepCodeLab render
    /// (Monaco + testcase — giống Assignment).</summary>
    private static List<object> BuildFinalCodingTasks()
    {
        return
        [
            new
            {
                id = "final-1-reverse-linked-list",
                title = "Đảo ngược danh sách liên kết (Linked List)",
                description = "MODULE 1 — Cấu trúc Dữ liệu Tuyến tính (Linked List).\n\n" +
                    "Viết hàm `reverseLinkedList(arr)` nhận MẢNG các số nguyên, dựng danh sách liên kết " +
                    "(node có `val` và `next`, node cuối có `next = null`) rồi ĐẢO NGƯỢC danh sách và trả về node đầu.\n\n" +
                    "Gợi ý: đảo ngược bằng 3 con trỏ prev/current/next — O(N) thời gian, O(1) không gian.\n\n" +
                    "Ví dụ: `reverseLinkedList([1,2,3,4,5])` → node đầu của chuỗi `5→4→3→2→1→null`.",
                initialCode = """
                    function ListNode(val, next = null) {
                      this.val = val;
                      this.next = next;
                    }

                    function reverseLinkedList(arr) {
                      // Dựng danh sách liên kết từ mảng
                      let head = null;
                      for (let i = arr.length - 1; i >= 0; i--) {
                        head = new ListNode(arr[i], head);
                      }
                      // TODO: đảo ngược 3 con trỏ, trả về node đầu của danh sách mới
                      return head;
                    }
                    """,
                solution = string.Empty,
                entryFunction = "reverseLinkedList",
                hints = new[]
                {
                    "Dựng list trước: duyệt mảng từ cuối lên đầu rồi tạo `new ListNode(arr[i], head)`.",
                    "Đảo ngược: giữ `prev = null; cur = head;`, mỗi bước lưu `next = cur.next`, trỏ `cur.next = prev`, rồi trượt cả 3 con trỏ.",
                    "Mảng rỗng `[]` → trả về `null`; mảng 1 phần tử → trả về chính node đó.",
                },
                testCases = new object[]
                {
                    new { name = "reverseLinkedList: ví dụ mẫu", input = "[[1,2,3,4,5]]", expectedOutput = "{\"val\":5,\"next\":{\"val\":4,\"next\":{\"val\":3,\"next\":{\"val\":2,\"next\":{\"val\":1,\"next\":null}}}}}", isHidden = false },
                    new { name = "reverseLinkedList: rỗng", input = "[[]]", expectedOutput = "null", isHidden = false },
                    new { name = "reverseLinkedList: 1 phần tử", input = "[[42]]", expectedOutput = "{\"val\":42,\"next\":null}", isHidden = false },
                    new { name = "reverseLinkedList: 3 phần tử", input = "[[1,2,3]]", expectedOutput = "{\"val\":3,\"next\":{\"val\":2,\"next\":{\"val\":1,\"next\":null}}}", isHidden = true },
                    new { name = "reverseLinkedList: 4 phần tử", input = "[[10,20,30,40]]", expectedOutput = "{\"val\":40,\"next\":{\"val\":30,\"next\":{\"val\":20,\"next\":{\"val\":10,\"next\":null}}}}", isHidden = true },
                }
            },
            new
            {
                id = "final-2-valid-parentheses",
                title = "Kiểm tra dấu ngoặc hợp lệ (Stack)",
                description = "MODULE 2 — Quản lý luồng dữ liệu (Stack — LIFO).\n\n" +
                    "Viết hàm `isValid(s)` kiểm tra chuỗi chỉ gồm `()[]{}` có hợp lệ không: ngoặc mở phải " +
                    "được đóng đúng loại và đúng thứ tự (mở trước — đóng sau). Dùng Stack.\n\n" +
                    "Gợi ý: gặp ngoặc mở thì push; gặp ngoặc đóng thì so khớp phần tử trên đỉnh stack (pop).\n\n" +
                    "Ví dụ: `isValid(\"()[]{}\")` → `true`; `isValid(\"(]\")` → `false`.",
                initialCode = """
                    function isValid(s) {
                      // TODO: dùng Stack (LIFO) kiểm tra dấu ngoặc hợp lệ
                      const stack = [];
                      const pairs = { ')': '(', ']': '[', '}': '{' };
                      for (const ch of s) {
                        // TODO: ngoặc mở → push; ngoặc đóng → so khớp đỉnh stack
                      }
                      return false;
                    }
                    """,
                solution = string.Empty,
                entryFunction = "isValid",
                hints = new[]
                {
                    "Ngoặc mở `(` `[` `{` → push vào stack; ngoặc đóng `)` `]` `}` → pop và so khớp với cặp tương ứng.",
                    "Khi gặp ngoặc đóng mà stack rỗng hoặc đỉnh stack không khớp → trả về `false` ngay.",
                    "Kết thúc: stack phải rỗng (không còn ngoặc mở lẻ) thì mới hợp lệ.",
                },
                testCases = new object[]
                {
                    new { name = "isValid: ví dụ mẫu", input = "[\"()[]{}\"]", expectedOutput = "true", isHidden = false },
                    new { name = "isValid: đơn giản", input = "[\"()\"]", expectedOutput = "true", isHidden = false },
                    new { name = "isValid: sai cặp", input = "[\"(]\"]", expectedOutput = "false", isHidden = false },
                    new { name = "isValid: lồng nhau", input = "[\"([{}])\"]", expectedOutput = "true", isHidden = true },
                    new { name = "isValid: mở lẻ", input = "[\"((((\"]", expectedOutput = "false", isHidden = true },
                }
            },
            new
            {
                id = "final-3-bst-two-sum",
                title = "Tìm cặp tổng K trong BST (Hash Map + Cây)",
                description = "MODULE 3 + MODULE 4 — Tra cứu tốc độ cao (Hash Map/Hash Set) & Cây phân cấp (Binary Search Tree).\n\n" +
                    "Viết hàm `findTarget(root, k)` kiểm tra trong cây BST có tồn tại HAI node phân biệt " +
                    "sao cho tổng giá trị bằng `k` hay không. Cây nhập dạng node `{ val, left, right }` (`null` = rỗng).\n\n" +
                    "Gợi ý: kết hợp duyệt cây (đệ quy) + Hash Set lưu giá trị đã gặp — với mỗi node, kiểm tra " +
                    "xem `k - node.val` đã có trong tập chưa; nếu có → `true`, nếu chưa → thêm `node.val` và tiếp tục xuống 2 nhánh.\n\n" +
                    "Ví dụ: cây `5(3,6)`, `k = 9` → `true` (3 + 6); `k = 2` → `false`.",
                initialCode = """
                    function TreeNode(val, left = null, right = null) {
                      this.val = val;
                      this.left = left;
                      this.right = right;
                    }

                    function findTarget(root, k) {
                      // TODO: duyệt cây + Hash Set lưu giá trị đã gặp
                      const seen = new Set();
                      return false;
                    }
                    """,
                solution = string.Empty,
                entryFunction = "findTarget",
                hints = new[]
                {
                    "Mỗi node chỉ được dùng MỘT lần — kiểm tra `seen.has(k - node.val)` TRƯỚC khi thêm `node.val` vào set.",
                    "Base case: node `null` → `false` (hết nhánh).",
                    "Độ phức tạp: O(N) thời gian, O(N) không gian cho Hash Set — nhanh hơn cách duyệt lồng nhau O(N²).",
                },
                testCases = new object[]
                {
                    new { name = "findTarget: ví dụ mẫu", input = "[{\"val\":5,\"left\":{\"val\":3,\"left\":null,\"right\":null},\"right\":{\"val\":6,\"left\":null,\"right\":null}},9]", expectedOutput = "true", isHidden = false },
                    new { name = "findTarget: không có cặp", input = "[{\"val\":5,\"left\":{\"val\":3,\"left\":null,\"right\":null},\"right\":{\"val\":6,\"left\":null,\"right\":null}},2]", expectedOutput = "false", isHidden = false },
                    new { name = "findTarget: cây rỗng", input = "[null,1]", expectedOutput = "false", isHidden = false },
                    new { name = "findTarget: cây lớn", input = "[{\"val\":8,\"left\":{\"val\":4,\"left\":{\"val\":2,\"left\":null,\"right\":null},\"right\":{\"val\":6,\"left\":null,\"right\":null}},\"right\":{\"val\":12,\"left\":null,\"right\":{\"val\":14,\"left\":null,\"right\":null}}},20]", expectedOutput = "true", isHidden = true },
                    new { name = "findTarget: không dùng lại cùng node", input = "[{\"val\":1,\"left\":null,\"right\":null},2]", expectedOutput = "false", isHidden = true },
                }
            },
        ];
    }

    // ── Rich sanitizer (chỉ dùng cho SEED content — giữ hình minh họa) ──

    private static Ganss.Xss.HtmlSanitizer CreateRichSanitizer()
    {
        var sanitizer = new Ganss.Xss.HtmlSanitizer();
        sanitizer.AllowedTags.Clear();
        foreach (var tag in new[]
                 {
                     "h1", "h2", "h3", "h4", "p", "strong", "em", "ul", "ol", "li", "pre", "code",
                     "blockquote", "br", "hr", "div", "span", "table", "thead", "tbody", "tr", "th", "td",
                     "figure", "figcaption", "b", "i", "u", "s", "sub", "sup", "mark"
                 })
        {
            sanitizer.AllowedTags.Add(tag);
        }

        sanitizer.AllowedAttributes.Clear();
        foreach (var attr in new[] { "style", "class", "align", "colspan", "rowspan", "width", "height", "bgcolor", "border" })
        {
            sanitizer.AllowedAttributes.Add(attr);
        }

        sanitizer.AllowedCssProperties.Clear();
        foreach (var prop in new[]
                 {
                     "display", "flex", "gap", "align-items", "justify-content", "margin", "padding",
                     "width", "height", "min-width", "min-height", "max-width", "max-height", "background",
                     "background-color", "color", "border", "border-radius", "font-size", "font-weight",
                     "text-align", "position", "top", "bottom", "left", "right", "overflow", "box-shadow",
                     "transform", "line-height", "white-space", "cursor", "opacity", "z-index", "box-sizing"
                 })
        {
            sanitizer.AllowedCssProperties.Add(prop);
        }

        sanitizer.AllowedSchemes.Clear();
        foreach (var scheme in new[] { "http", "https", "mailto", "data" })
        {
            sanitizer.AllowedSchemes.Add(scheme);
        }

        return sanitizer;
    }
}
