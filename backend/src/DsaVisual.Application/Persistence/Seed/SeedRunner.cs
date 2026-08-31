using System.Net;
using System.Text;
using System.Text.Json;
using DsaVisual.Application.Common;
using DsaVisual.Application.Persistence.Entities;
using Ganss.Xss;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Persistence.Seed;

/// <summary>
/// Seeder THẬT chạy qua AppDbContext (SDD §7.5) — idempotent: kiểm tra tồn tại trước khi chèn.
/// Hook từ Program.cs: <c>dotnet run --project src/DsaVisual.Api -- --seed</c> (sau Migrate()).
///
/// Phạm vi: 3 Users (hash PBKDF2 thật), 5 Topics, 8 Lessons (Bubble Sort, Binary Search, Stack,
/// Linked List, BST, AVL, Hash Table, BFS) + LessonSimulations + Exercises/Questions
/// (MCQ từ content-drafts quiz.json + SIMULATION_LAB + CODE với test ẩn) + LearningPaths/Nodes +
/// DailyQuests + ShopItems + Settings. KHÔNG seed ExerciseSubmissions.
///
/// Nội dung bài học lấy từ source/VisualizationDSA3/plan/content-drafts/v2/lesson-XX/ (content.md →
/// ContentHtml sanitize; quiz.json → Questions). Bài AVL không có bài nguồn riêng (lesson-17 chỉ nhắc
/// AVL ngắn gọn) → nội dung + câu hỏi tự soạn theo SDD §4.7 (xem README).
/// </summary>
public static class SeedRunner
{
    // ── Nguồn nội dung ──────────────────────────────────────────

    private const string ContentDraftsRelative = "source\\VisualizationDSA3\\plan\\content-drafts\\v2";

    /// <summary>Chỉ số câu hỏi (0-based) chọn từ quiz.json của bài nguồn cho Exercise MCQ mỗi bài (5-8 câu).</summary>
    private static readonly IReadOnlyDictionary<string, int[]> QuizSelectionByLesson = new Dictionary<string, int[]>
    {
        ["Bubble Sort"] = [0, 3, 4, 5, 7],                    // lesson-09: 5 câu (lọc theo Bubble Sort)
        ["Binary Search"] = [1, 2, 4, 5, 6, 7, 8, 9],         // lesson-10: 8 câu
        ["Stack"] = [0, 1, 2, 3, 4, 5, 6, 7],                 // lesson-06: 8 câu
        ["Linked List"] = [0, 1, 2, 3, 4, 6, 8, 9],           // lesson-05: 8 câu
        ["BST"] = [0, 1, 2, 3, 4, 5, 8, 9],                   // lesson-17: 8 câu
        ["Hash Table"] = [0, 1, 2, 3, 4, 5, 7, 9],            // lesson-04: 8 câu
        ["BFS"] = [0, 1, 3, 4, 5, 6, 7, 8],                   // lesson-20: 8 câu
        // "AVL" — không có quiz nguồn riêng → AuthoredQuizQuestions["AVL"]
    };

    // ── Entry point ─────────────────────────────────────────────

    /// <summary>Chạy toàn bộ seed idempotent. Gọi SAU <c>db.Database.MigrateAsync()</c>.</summary>
    public static async Task SeedAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        var now = clock.UtcNow;

        // Atomicity (audit bề mặt #3): bọc TOÀN BỘ seed trong 1 transaction — crash giữa chừng
        // không để lại dữ liệu lửng (rollback toàn bộ). InMemory (unit test) không hỗ trợ
        // transaction → bỏ qua, giữ hành vi cũ.
        await using var tx = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(ct)
            : null;

        var adminId = await SeedUsersAsync(db, now, logger, ct);
        var topics = await SeedTopicsAsync(db, adminId, now, logger, ct);
        var lessons = await SeedLessonsAsync(db, topics, adminId, now, logger, ct);
        await SeedLessonSimulationsAsync(db, lessons, logger, ct);
        // H-FINAL1: seed LearningPaths/Nodes TRƯỚC exercises → exercise lesson được gán NodeId/Stage
        // (Ladder filter GET /exercises?nodeId&stage — Exercise.NodeId/Stage, SDD §7.3.9).
        await SeedLearningPathsAsync(db, topics, lessons, adminId, now, logger, ct);
        await SeedExercisesAsync(db, lessons, adminId, now, logger, ct);
        // SeedGrokking: import khóa "Grokking Data Structures" (backend/seed-data/grokking-course.json) —
        // 4 topics + 16 lessons + path lớn + exercises; Ẩn 5 path cũ (IsActive=false).
        await SeedGrokkingData.SeedAsync(db, adminId, now, logger, ct);
        // SeedGrokkingAlgorithms: import khóa "Grokking Algorithms" (backend/seed-data/grokking-algorithms.json) —
        // 8 topics + 32 lessons + path lớn + exercises + final test (lộ trình thứ 2, SortOrder=2).
        await SeedGrokkingAlgorithmsData.SeedAsync(db, adminId, now, logger, ct);
        await SeedQuestsAsync(db, logger, ct);
        await SeedShopItemsAsync(db, logger, ct);
        await SeedSettingsAsync(db, adminId, now, logger, ct);

        // Reconcile tree and class assignments trước khi seed activity
        await MigratePathTreeAndClassAssignmentsAsync(db, logger, ct);

        // Seed hoạt động người dùng demo (nếu có lỗi dữ liệu cũ thì bỏ qua an toàn)
        try
        {
            await SeedDemoActivity.SeedAsync(db, clock, logger, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "SeedDemoActivity: Bỏ qua hoạt động giả lập học sinh cũ");
        }

        if (tx is not null)
        {
            await tx.CommitAsync(ct);
        }

        logger.LogInformation(
            "Seed hoàn tất: Users={Users}, Topics={Topics}, Lessons={Lessons}, Exercises={Exercises}, Questions={Questions}, LearningPaths={Paths}, DailyQuests={Quests}, ShopItems={Items}, Settings={Settings}",
            await db.Users.CountAsync(ct), await db.Topics.CountAsync(ct), await db.Lessons.CountAsync(ct),
            await db.Exercises.CountAsync(ct), await db.Questions.CountAsync(ct),
            await db.LearningPaths.CountAsync(ct), await db.DailyQuests.CountAsync(ct),
            await db.ShopItems.CountAsync(ct), await db.Settings.CountAsync(ct));
    }

    public static async Task FixMismatchedQuestionsAsync(AppDbContext db, CancellationToken ct = default)
    {
        foreach (var (title, questions) in AuthoredQuestionsByLesson)
        {
            var quizTitle = $"Quiz: {title}";
            var exercises = await db.Exercises.Where(e => e.Title == quizTitle && e.DeletedAt == null).ToListAsync(ct);
            foreach (var ex in exercises)
            {
                var curQuestions = await db.Questions.Where(q => q.ExerciseId == ex.Id).ToListAsync(ct);
                var isWrong = curQuestions.Any(q => q.Content.Contains("AVL", StringComparison.OrdinalIgnoreCase)) && !title.Contains("AVL", StringComparison.OrdinalIgnoreCase);
                if (isWrong || curQuestions.Count == 0)
                {
                    db.Questions.RemoveRange(curQuestions);
                    var sort = 1;
                    foreach (var (text, opts, correct, exp) in questions)
                    {
                        db.Questions.Add(new Question
                        {
                            ExerciseId = ex.Id,
                            Type = QuestionType.Single,
                            Content = text,
                            OptionsJson = System.Text.Json.JsonSerializer.Serialize(opts),
                            AnswerJson = $"[{correct}]",
                            Explanation = exp,
                            Points = 1,
                            SortOrder = sort++
                        });
                    }
                    ex.MaxScore = questions.Count;
                    ex.Description = $"Trắc nghiệm kiến thức {title} — {questions.Count} câu, giải thích tiếng Việt sau khi nộp.";
                }
            }
        }
        await db.SaveChangesAsync(ct);
    }

    public static async Task AutoRepairOrphanTheoryNodesAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        var orphanNodes = await db.LearningPathNodes
            .Where(n => n.ItemType == PathItemType.Theory && n.LessonId == null)
            .ToListAsync(ct);

        if (orphanNodes.Count > 0)
        {
            var defaultTopicId = await db.Topics.Select(t => t.Id).FirstOrDefaultAsync(ct);
            if (defaultTopicId == 0) defaultTopicId = 1;

            foreach (var node in orphanNodes)
            {
                var path = await db.LearningPaths.AsNoTracking().FirstOrDefaultAsync(p => p.Id == node.PathId, ct);
                var lesson = new Lesson
                {
                    TopicId = path?.TopicId ?? defaultTopicId,
                    Title = !string.IsNullOrWhiteSpace(node.Title) ? node.Title : "Bài học mới",
                    Description = node.Description,
                    ContentHtml = "",
                    Status = LessonStatus.Active,
                    CreatedBy = path?.CreatedBy ?? 1,
                    CreatedAt = DateTime.UtcNow
                };
                db.Lessons.Add(lesson);
                await db.SaveChangesAsync(ct);

                node.LessonId = lesson.Id;
                logger.LogInformation("Auto-repaired orphan theory node {NodeId} -> created Lesson {LessonId}", node.Id, lesson.Id);
            }
            await db.SaveChangesAsync(ct);
        }
    }

    public static async Task MigratePathTreeAndClassAssignmentsAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        // 1. Reconcile LearningPath.Visibility
        var paths = await db.LearningPaths.ToListAsync(ct);
        foreach (var path in paths)
        {
            if (path.Status == LearningPathStatus.Active && path.Visibility == PathVisibility.Private)
            {
                path.Visibility = PathVisibility.Public;
            }
            else if (path.Status == LearningPathStatus.ClassOnly && path.Visibility == PathVisibility.Private)
            {
                path.Visibility = PathVisibility.ClassOnly;
            }
        }

        // 2. Reconcile LearningPathNodes ItemType
        var nodes = await db.LearningPathNodes.ToListAsync(ct);
        foreach (var node in nodes)
        {
            if (node.LessonId != null && node.ItemType == PathItemType.Folder)
            {
                node.ItemType = PathItemType.Theory;
            }
            else if ((node.FinalTestId != null || node.Title.Contains("Kiểm tra") || node.Title.Contains("Quiz")) && node.ItemType == PathItemType.Folder)
            {
                node.ItemType = PathItemType.Quiz;
            }
            else if ((node.LabExerciseId != null || node.Title.Contains("Lab") || node.Title.Contains("Thực hành")) && node.ItemType == PathItemType.Folder)
            {
                node.ItemType = PathItemType.Lab;
            }
        }

        // 3. Reconcile ClassAssignments with PathItemId
        var classes = await db.Classes.Where(c => c.LearningPathId != null && c.DeletedAt == null).ToListAsync(ct);
        foreach (var cls in classes)
        {
            var classAssignments = await db.ClassAssignments.Where(a => a.ClassId == cls.Id && a.PathItemId == null).ToListAsync(ct);
            if (classAssignments.Count == 0) continue;

            var pathNodes = await db.LearningPathNodes.Where(n => n.PathId == cls.LearningPathId!.Value).ToListAsync(ct);
            foreach (var assign in classAssignments)
            {
                if (assign.LessonId is { } lid)
                {
                    var match = pathNodes.FirstOrDefault(n => n.LessonId == lid);
                    if (match is not null)
                    {
                        assign.PathItemId = match.Id;
                    }
                }
                else if (assign.ExerciseId is { } eid)
                {
                    var match = pathNodes.FirstOrDefault(n => n.FinalTestId == eid || n.LabExerciseId == eid);
                    if (match is not null)
                    {
                        assign.PathItemId = match.Id;
                    }
                }
            }
        }

        await db.SaveChangesAsync(ct);
    }

    // ── 1. Users ────────────────────────────────────────────────

    private static async Task<int> SeedUsersAsync(AppDbContext db, DateTime now, ILogger logger, CancellationToken ct)
    {
        var adminId = 0;
        foreach (var seed in SeedData.Users)
        {
            var email = seed.Email.ToLowerInvariant();
            var exists = await db.Users.AnyAsync(u => u.Email == email, ct);
            if (exists)
            {
                var user = await db.Users.FirstAsync(u => u.Email == email, ct);
                user.PasswordHash = PasswordHasher.Hash(seed.DevPassword);
                user.IsActive = true;
                user.DeletedAt = null;
                if (user.IsPrimaryAdmin)
                {
                    adminId = user.Id;
                }

                // Teacher demo luôn có đủ dữ liệu để trình diễn Teacher Studio/Shop trên máy dev.
                if (email == "teacher@demo.local")
                {
                    user.Gems = Math.Max(user.Gems, 1000);
                    user.AvatarUrl ??= "/assets/avatars/cyber-hacker.svg";
                }
                user.UpdatedAt = now;
                await db.SaveChangesAsync(ct);

                logger.LogInformation("Seed: Users cập nhật mật khẩu DEV & trạng thái {Email}", email);
                continue;
            }

            var entity = new User
            {
                Email = email,
                PasswordHash = PasswordHasher.Hash(seed.DevPassword),
                DisplayName = seed.DisplayName,
                Role = (UserRole)seed.Role,
                IsActive = true,
                IsPrimaryAdmin = seed.IsPrimaryAdmin,
                Hearts = 10,
                HeartsMax = 10,
                LastHeartAt = now,
                Gems = seed.Email == "teacher@demo.local" ? 1000 : 0,
                AvatarUrl = seed.Email == "teacher@demo.local" ? "/assets/avatars/cyber-hacker.svg" : null,
                CreatedAt = now
            };
            db.Users.Add(entity);
            await db.SaveChangesAsync(ct);   // lấy Id ngay để dùng làm CreatedBy
            if (entity.IsPrimaryAdmin)
            {
                adminId = entity.Id;
            }

            logger.LogInformation("Seed: Users thêm {Email} (Id={Id}, Role={Role})", email, entity.Id, entity.Role);
        }

        if (adminId == 0)
        {
            throw new InvalidOperationException("Seed: không tìm thấy admin chính (admin@system.local)");
        }

        return adminId;
    }

    // ── 2. Topics ───────────────────────────────────────────────

    private static async Task<Dictionary<string, Topic>> SeedTopicsAsync(AppDbContext db, int adminId, DateTime now, ILogger logger, CancellationToken ct)
    {
        var result = new Dictionary<string, Topic>(StringComparer.Ordinal);
        foreach (var seed in SeedData.Topics)
        {
            var topic = await db.Topics.FirstOrDefaultAsync(t => t.ParentId == null && t.Name == seed.Name, ct);
            if (topic is not null)
            {
                logger.LogInformation("Seed: Topics bỏ qua (đã tồn tại) {Name}", seed.Name);
            }
            else
            {
                topic = new Topic
                {
                    Name = seed.Name,
                    Description = seed.Description,
                    SortOrder = seed.SortOrder,
                    CreatedBy = adminId,
                    CreatedAt = now
                };
                db.Topics.Add(topic);
                await db.SaveChangesAsync(ct);
                logger.LogInformation("Seed: Topics thêm {Name} (Id={Id})", seed.Name, topic.Id);
            }

            result[seed.Name] = topic;
        }

        return result;
    }

    // ── 3. Lessons ──────────────────────────────────────────────

    private static async Task<Dictionary<string, Lesson>> SeedLessonsAsync(
        AppDbContext db, Dictionary<string, Topic> topics, int adminId, DateTime now, ILogger logger, CancellationToken ct)
    {
        var result = new Dictionary<string, Lesson>(StringComparer.Ordinal);
        var sanitizer = CreateSanitizer();

        foreach (var seed in SeedData.Lessons)
        {
            var topic = topics[seed.TopicId switch
            {
                1 => "Sắp xếp & Tìm kiếm",
                2 => "CTDL tuyến tính",
                3 => "Cây",
                4 => "Bảng băm",
                _ => "Đồ thị"
            }];

            var lesson = await db.Lessons.FirstOrDefaultAsync(l => l.TopicId == topic.Id && l.Title == seed.Title, ct);
            if (lesson is not null)
            {
                logger.LogInformation("Seed: Lessons bỏ qua (đã tồn tại) {Title}", seed.Title);
                result[seed.Title] = lesson;
                continue;
            }

            var markdown = seed.SourceLesson is null
                ? AuthoredContent.AvlMarkdown
                : ReadSourceFile(seed.SourceLesson, "content.md") ?? AuthoredContent.GenericMarkdown(seed.Title);

            lesson = new Lesson
            {
                TopicId = topic.Id,
                Title = seed.Title,
                Description = seed.Description,
                ContentHtml = sanitizer.Sanitize(MarkdownToHtml(markdown)),
                SortOrder = seed.SortOrder,
                Status = LessonStatus.Active,
                CreatedBy = adminId,
                CreatedAt = now
            };
            db.Lessons.Add(lesson);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Seed: Lessons thêm {Title} (Id={Id}, Topic={Topic})", seed.Title, lesson.Id, topic.Name);
            result[seed.Title] = lesson;
        }

        return result;
    }

    // ── 4. LessonSimulations ────────────────────────────────────

    private static async Task SeedLessonSimulationsAsync(AppDbContext db, Dictionary<string, Lesson> lessons, ILogger logger, CancellationToken ct)
    {
        foreach (var seed in SeedData.Lessons)
        {
            var lesson = lessons[seed.Title];
            var existing = await db.LessonSimulations
                .Where(s => s.LessonId == lesson.Id)
                .Select(s => s.SimulationKey)
                .ToListAsync(ct);

            var order = 1;
            foreach (var key in seed.SimulationKeys)
            {
                if (existing.Contains(key))
                {
                    logger.LogInformation("Seed: LessonSimulations bỏ qua (đã tồn tại) {Lesson}/{Key}", seed.Title, key);
                    continue;
                }

                db.LessonSimulations.Add(new LessonSimulation
                {
                    LessonId = lesson.Id,
                    SimulationKey = key,
                    Title = $"{seed.Title} — mô phỏng {key}",
                    SortOrder = order
                });
                order++;
            }
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seed: LessonSimulations thêm xong (bỏ qua các cặp LessonId+SimulationKey đã tồn tại)");
    }

    // ── 5. Exercises / Questions ────────────────────────────────
    // Stage exercise trên Ladder (Exercise.Stage — 1=QUIZ, 2=LAB, 3=CODE, SDD §7.3.9).
    private const int StageQuiz = 1;
    private const int StageLab = 2;
    private const int StageCode = 3;

    private static async Task SeedExercisesAsync(
        AppDbContext db, Dictionary<string, Lesson> lessons, int adminId, DateTime now, ILogger logger, CancellationToken ct)
    {
        foreach (var seed in SeedData.Lessons)
        {
            var lesson = lessons[seed.Title];

            // H-FINAL1: node "Học: {lesson}" của learning path — mọi exercise lesson gắn NodeId + Stage
            // để GET /exercises?nodeId&stage (Ladder) trả đúng item (trước đây NodeId=null → stage rỗng).
            var node = await db.LearningPathNodes.OrderBy(n => n.Id).FirstOrDefaultAsync(n => n.LessonId == lesson.Id, ct);
            if (node is null)
            {
                logger.LogWarning(
                    "Seed: Exercises bỏ qua {Title} — chưa có LearningPathNode gắn LessonId={LessonId} (path chưa seed?)",
                    lesson.Title, lesson.Id);
                continue;
            }

            await SeedMcqExerciseAsync(db, lesson, node, seed, adminId, now, logger, ct);
            await SeedLabExerciseAsync(db, lesson, node, seed, adminId, now, logger, ct);
            await SeedCodeExerciseAsync(db, lesson, node, seed, adminId, now, logger, ct);
        }
    }

    private static async Task SeedMcqExerciseAsync(
        AppDbContext db, Lesson lesson, LearningPathNode node, SeedData.SeedLesson seed,
        int adminId, DateTime now, ILogger logger, CancellationToken ct)
    {
        const string titlePrefix = "Quiz: ";
        var title = titlePrefix + lesson.Title;
        var existing = await db.Exercises.FirstOrDefaultAsync(
            e => e.LessonId == lesson.Id && e.Title == title && e.DeletedAt == null, ct);
        if (existing is not null)
        {
            var correctQuestions = LoadQuizQuestions(lesson.Title, seed.SourceLesson);
            var currentQuestions = await db.Questions.Where(q => q.ExerciseId == existing.Id).ToListAsync(ct);
            var hasMismatch = currentQuestions.Any(q => q.Content.Contains("AVL", StringComparison.OrdinalIgnoreCase)) && !lesson.Title.Contains("AVL", StringComparison.OrdinalIgnoreCase);
            if (hasMismatch || currentQuestions.Count == 0)
            {
                db.Questions.RemoveRange(currentQuestions);
                foreach (var q in correctQuestions)
                {
                    q.ExerciseId = existing.Id;
                    db.Questions.Add(q);
                }
                existing.MaxScore = correctQuestions.Sum(q => q.Points);
                existing.Description = $"Trắc nghiệm kiến thức {lesson.Title} — {correctQuestions.Count} câu, giải thích tiếng Việt sau khi nộp.";
                await db.SaveChangesAsync(ct);
                logger.LogInformation("Seed: Cập nhật lại câu hỏi chuẩn cho {Title}", title);
            }

            // H-FINAL1 backfill: DB cũ seed trước fix có NodeId=null → gán lại (idempotent, không nhân đôi)
            if (existing.NodeId != node.Id || existing.Stage != StageQuiz)
            {
                existing.NodeId = node.Id;
                existing.Stage = StageQuiz;
                await db.SaveChangesAsync(ct);
                logger.LogInformation("Seed: Exercises backfill NodeId/Stage {Title} (Node={NodeId}, Stage={Stage})", title, existing.NodeId, existing.Stage);
            }
            else
            {
                logger.LogInformation("Seed: Exercises bỏ qua (đã tồn tại) {Title}", title);
            }

            return;
        }

        var questions = LoadQuizQuestions(lesson.Title, seed.SourceLesson);

        var exercise = new Exercise
        {
            LessonId = lesson.Id,
            NodeId = node.Id,
            Stage = StageQuiz,
            Title = title,
            Description = $"Trắc nghiệm kiến thức {lesson.Title} — {questions.Count} câu, giải thích tiếng Việt sau khi nộp.",
            Type = ExerciseType.Mcq,
            MaxScore = questions.Sum(q => q.Points),
            Status = ExerciseStatus.Active,
            CreatedBy = adminId,
            CreatedAt = now,
            Questions = questions
        };
        db.Exercises.Add(exercise);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seed: Exercises thêm {Title} (Id={Id}, {Count} câu hỏi, Node={NodeId}, Stage={Stage})", title, exercise.Id, questions.Count, exercise.NodeId, exercise.Stage);
    }

    private static async Task SeedLabExerciseAsync(
        AppDbContext db, Lesson lesson, LearningPathNode node, SeedData.SeedLesson seed,
        int adminId, DateTime now, ILogger logger, CancellationToken ct)
    {
        const string titlePrefix = "Lab: ";
        var title = titlePrefix + lesson.Title;
        var existing = await db.Exercises.FirstOrDefaultAsync(
            e => e.LessonId == lesson.Id && e.Title == title && e.DeletedAt == null, ct);
        if (existing is not null)
        {
            // H-FINAL1 backfill: DB cũ seed trước fix có NodeId=null → gán lại (idempotent, không nhân đôi)
            if (existing.NodeId != node.Id || existing.Stage != StageLab)
            {
                existing.NodeId = node.Id;
                existing.Stage = StageLab;
                await db.SaveChangesAsync(ct);
                logger.LogInformation("Seed: Exercises backfill NodeId/Stage {Title} (Node={NodeId}, Stage={Stage})", title, existing.NodeId, existing.Stage);
            }
            else
            {
                logger.LogInformation("Seed: Exercises bỏ qua (đã tồn tại) {Title}", title);
            }

            return;
        }

        var simulationKey = seed.SimulationKeys[0];
        var maxSteps = LabMaxStepsByLesson[lesson.Title];
        var exercise = new Exercise
        {
            LessonId = lesson.Id,
            NodeId = node.Id,
            Stage = StageLab,
            Title = title,
            Description = $"Mô phỏng {simulationKey} từng bước trên canvas — đạt trạng thái cuối chuẩn trong tối đa {maxSteps} bước (chấm theo trạng thái cuối, SDD §4.16/API_REFERENCE §4.16).",
            Type = ExerciseType.SimulationLab,
            ConfigJson = JsonSerializer.Serialize(new { simulationKey, maxSteps }),
            MaxScore = 10,
            Status = ExerciseStatus.Active,
            CreatedBy = adminId,
            CreatedAt = now
        };
        db.Exercises.Add(exercise);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seed: Exercises thêm {Title} (Id={Id}, key={Key}, Node={NodeId}, Stage={Stage})", title, exercise.Id, simulationKey, exercise.NodeId, exercise.Stage);
    }

    private static async Task SeedCodeExerciseAsync(
        AppDbContext db, Lesson lesson, LearningPathNode node, SeedData.SeedLesson seed,
        int adminId, DateTime now, ILogger logger, CancellationToken ct)
    {
        const string titlePrefix = "Code: ";
        var title = titlePrefix + lesson.Title;
        var existing = await db.Exercises.FirstOrDefaultAsync(
            e => e.LessonId == lesson.Id && e.Title == title && e.DeletedAt == null, ct);
        if (existing is not null)
        {
            // H-FINAL1 backfill: DB cũ seed trước fix có NodeId=null → gán lại (idempotent, không nhân đôi)
            if (existing.NodeId != node.Id || existing.Stage != StageCode)
            {
                existing.NodeId = node.Id;
                existing.Stage = StageCode;
                await db.SaveChangesAsync(ct);
                logger.LogInformation("Seed: Exercises backfill NodeId/Stage {Title} (Node={NodeId}, Stage={Stage})", title, existing.NodeId, existing.Stage);
            }
            else
            {
                logger.LogInformation("Seed: Exercises bỏ qua (đã tồn tại) {Title}", title);
            }

            return;
        }

        var testCases = CodeTestCasesByLesson[lesson.Title];
        var exercise = new Exercise
        {
            LessonId = lesson.Id,
            NodeId = node.Id,
            Stage = StageCode,
            Title = title,
            Description = $"Thử thách lập trình {lesson.Title} — {testCases.Count} test ẩn, pass ≥ 70% để đạt bậc (sandbox client chấm, ADR-012).",
            Type = ExerciseType.Code,
            ConfigJson = JsonSerializer.Serialize(testCases.Config),
            MaxScore = 100,
            Status = ExerciseStatus.Active,
            CreatedBy = adminId,
            CreatedAt = now
        };
        db.Exercises.Add(exercise);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seed: Exercises thêm {Title} (Id={Id}, {Count} test ẩn, Node={NodeId}, Stage={Stage})", title, exercise.Id, testCases.Count, exercise.NodeId, exercise.Stage);
    }

    // ── 6. LearningPaths / Nodes + final test ───────────────────

    private static async Task SeedLearningPathsAsync(
        AppDbContext db, Dictionary<string, Topic> topics, Dictionary<string, Lesson> lessons,
        int adminId, DateTime now, ILogger logger, CancellationToken ct)
    {
        // 5 path: mỗi topic 1 path; node bài học + node luyện tập tổng hợp + node kiểm tra cuối.
        var pathSpecs = new (string PathTitle, string TopicName, string[] LessonTitles)[]
        {
            ("Lộ trình Sắp xếp & Tìm kiếm", "Sắp xếp & Tìm kiếm", ["Bubble Sort", "Binary Search"]),
            ("Lộ trình CTDL tuyến tính", "CTDL tuyến tính", ["Stack", "Linked List"]),
            ("Lộ trình Cây", "Cây", ["BST", "AVL"]),
            ("Lộ trình Bảng băm", "Bảng băm", ["Hash Table"]),
            ("Lộ trình Đồ thị", "Đồ thị", ["BFS"]),
        };

        foreach (var spec in pathSpecs)
        {
            var path = await db.LearningPaths.FirstOrDefaultAsync(p => p.Title == spec.PathTitle, ct);
            if (path is null)
            {
                path = new LearningPath
                {
                    Title = spec.PathTitle,
                    Description = $"Lộ trình học chủ đề {spec.TopicName} — từ bài học đến luyện tập tổng hợp và kiểm tra cuối.",
                    TopicId = topics[spec.TopicName].Id,
                    SortOrder = topics[spec.TopicName].SortOrder,
                    IsActive = true,
                    CreatedBy = adminId
                };
                db.LearningPaths.Add(path);
                await db.SaveChangesAsync(ct);
                logger.LogInformation("Seed: LearningPaths thêm {Title} (Id={Id})", spec.PathTitle, path.Id);
            }
            else
            {
                logger.LogInformation("Seed: LearningPaths bỏ qua (đã tồn tại) {Title}", spec.PathTitle);
            }

            var pathLessonIds = spec.LessonTitles.Select(t => lessons[t].Id).ToList();
            var sourceByTitle = SeedData.Lessons.ToDictionary(l => l.Title, l => l.SourceLesson);

            // Node bài học
            var sortOrder = 1;
            foreach (var lessonTitle in spec.LessonTitles)
            {
                var lesson = lessons[lessonTitle];
                var nodeTitle = $"Học: {lesson.Title}";
                var exists = await db.LearningPathNodes.AnyAsync(n => n.PathId == path.Id && n.Title == nodeTitle, ct);
                if (!exists)
                {
                    db.LearningPathNodes.Add(new LearningPathNode
                    {
                        PathId = path.Id,
                        Title = nodeTitle,
                        LessonId = lesson.Id,
                        SortOrder = sortOrder
                    });
                    logger.LogInformation("Seed: LearningPathNodes thêm {Node} (path={Path})", nodeTitle, spec.PathTitle);
                }
                else
                {
                    logger.LogInformation("Seed: LearningPathNodes bỏ qua (đã tồn tại) {Node}", nodeTitle);
                }

                sortOrder++;
            }

            // Node luyện tập tổng hợp (LessonId = null — đề trộn runtime theo seed, SDD §7.3.25 D-3)
            const string practiceNodeTitle = "Luyện tập tổng hợp";
            var practiceExists = await db.LearningPathNodes.AnyAsync(n => n.PathId == path.Id && n.Title == practiceNodeTitle, ct);
            if (!practiceExists)
            {
                db.LearningPathNodes.Add(new LearningPathNode
                {
                    PathId = path.Id,
                    Title = practiceNodeTitle,
                    LessonId = null,
                    SortOrder = sortOrder
                });
                logger.LogInformation("Seed: LearningPathNodes thêm {Node} (path={Path})", practiceNodeTitle, spec.PathTitle);
            }

            sortOrder++;

            // Node kiểm tra cuối + final-test exercise (FinalTestId ↔ Exercise.NodeId — chèn tuần tự tránh FK vòng)
            const string finalNodeTitle = "Kiểm tra cuối lộ trình";
            var finalNode = await db.LearningPathNodes.FirstOrDefaultAsync(n => n.PathId == path.Id && n.Title == finalNodeTitle, ct);
            if (finalNode is null)
            {
                finalNode = new LearningPathNode
                {
                    PathId = path.Id,
                    Title = finalNodeTitle,
                    LessonId = null,
                    SortOrder = sortOrder
                };
                db.LearningPathNodes.Add(finalNode);
                await db.SaveChangesAsync(ct);
                logger.LogInformation("Seed: LearningPathNodes thêm {Node} (path={Path})", finalNodeTitle, spec.PathTitle);
            }

            var finalTestTitle = $"Kiểm tra cuối: {spec.PathTitle}";
            var lastLessonId = pathLessonIds[^1];
            var finalExercise = await db.Exercises.FirstOrDefaultAsync(
                e => e.LessonId == lastLessonId && e.Title == finalTestTitle && e.DeletedAt == null, ct);
            if (finalExercise is null)
            {
                var questions = new List<Question>();
                var qSort = 1;
                foreach (var lessonTitle in spec.LessonTitles)
                {
                    // Lấy 2 câu đầu của mỗi bài trong path làm đề final test (MCQ trộn)
                    foreach (var q in LoadQuizQuestions(lessonTitle, sourceByTitle[lessonTitle], takeFirst: 2))
                    {
                        q.SortOrder = qSort++;
                        questions.Add(q);
                        if (questions.Count >= 5)
                        {
                            break;
                        }
                    }

                    if (questions.Count >= 5)
                    {
                        break;
                    }
                }

                finalExercise = new Exercise
                {
                    LessonId = lastLessonId,      // FK NOT NULL — gắn bài học cuối path
                    NodeId = finalNode.Id,
                    Stage = StageQuiz,                 // 1 = QUIZ
                    Title = finalTestTitle,
                    Description = $"Kiểm tra cuối lộ trình {spec.PathTitle} — {questions.Count} câu trộn từ các bài học trong path.",
                    Type = ExerciseType.Mcq,
                    MaxScore = questions.Sum(q => q.Points),
                    Status = ExerciseStatus.Active,
                    CreatedBy = adminId,
                    CreatedAt = now,
                    Questions = questions
                };
                db.Exercises.Add(finalExercise);
                await db.SaveChangesAsync(ct);

                finalNode.FinalTestId = finalExercise.Id;
                await db.SaveChangesAsync(ct);
                logger.LogInformation("Seed: final test thêm {Title} (Id={Id}, Node={NodeId})", finalTestTitle, finalExercise.Id, finalNode.Id);
            }
            else
            {
                logger.LogInformation("Seed: final test bỏ qua (đã tồn tại) {Title}", finalTestTitle);
            }
        }
    }

    // ── 7. DailyQuests ──────────────────────────────────────────

    private static async Task SeedQuestsAsync(AppDbContext db, ILogger logger, CancellationToken ct)
    {
        foreach (var quest in SeedData.Quests)
        {
            var exists = await db.DailyQuests.AnyAsync(q => q.QuestKey == quest.QuestKey, ct);
            if (exists)
            {
                logger.LogInformation("Seed: DailyQuests bỏ qua (đã tồn tại) {QuestKey}", quest.QuestKey);
                continue;
            }

            db.DailyQuests.Add(new DailyQuest
            {
                QuestKey = quest.QuestKey,
                Title = quest.Title,
                Type = quest.Type,
                ConditionJson = quest.ConditionJson,
                RewardJson = quest.RewardJson,
                PoolEnabled = true
            });
            logger.LogInformation("Seed: DailyQuests thêm {QuestKey}", quest.QuestKey);
        }

        await db.SaveChangesAsync(ct);
    }

    // ── 8. ShopItems ────────────────────────────────────────────

    private static async Task SeedShopItemsAsync(AppDbContext db, ILogger logger, CancellationToken ct)
    {
        foreach (var item in SeedData.ShopItems)
        {
            var existing = await db.ShopItems.FirstOrDefaultAsync(i => i.ItemKey == item.ItemKey, ct);
            if (existing is not null)
            {
                if (existing.Type != item.Type)
                {
                    existing.Type = item.Type;
                    logger.LogInformation("Seed: ShopItems cập nhật Type cho {ItemKey} -> {Type}", item.ItemKey, item.Type);
                }
                continue;
            }

            db.ShopItems.Add(new ShopItem
            {
                ItemKey = item.ItemKey,
                Name = item.Name,
                PriceGems = item.PriceGems,
                MaxStack = item.MaxStack,
                Type = item.Type,
                DurationHours = item.DurationHours
            });
            logger.LogInformation("Seed: ShopItems thêm {ItemKey}", item.ItemKey);
        }

        await db.SaveChangesAsync(ct);
    }

    // ── 9. Settings ─────────────────────────────────────────────

    private static async Task SeedSettingsAsync(AppDbContext db, int adminId, DateTime now, ILogger logger, CancellationToken ct)
    {
        foreach (var setting in SeedData.Settings)
        {
            var exists = await db.Settings.AnyAsync(s => s.Key == setting.Key, ct);
            if (exists)
            {
                logger.LogInformation("Seed: Settings bỏ qua (đã tồn tại) {Key}", setting.Key);
                continue;
            }

            db.Settings.Add(new Setting
            {
                Key = setting.Key,
                Value = setting.Value,
                Description = setting.Description,
                UpdatedAt = now,
                UpdatedBy = adminId
            });
            logger.LogInformation("Seed: Settings thêm {Key}", setting.Key);
        }

        await db.SaveChangesAsync(ct);
    }

    // ── Quiz loading ────────────────────────────────────────────

    /// <summary>Đọc quiz.json bài nguồn → danh sách Question SINGLE (map SDD §7.3.3).</summary>
    private static List<Question> LoadQuizQuestions(string lessonTitle, string? sourceLesson, int takeFirst = 0)
    {
        var authored = AuthoredQuestionsByLesson.TryGetValue(lessonTitle, out var authoredQuestions);
        var selected = new List<(string Text, List<string> Options, int Correct, string? Explanation)>();

        if (authored)
        {
            selected = authoredQuestions!;
        }
        else if (sourceLesson is not null)
        {
            var json = ReadSourceFile(sourceLesson, "quiz.json");
            if (json is not null)
            {
                var indexes = QuizSelectionByLesson.GetValueOrDefault(lessonTitle, []);
                selected = ParseQuizJson(json, indexes);
            }
        }

        if (selected.Count == 0)
        {
            if (AuthoredQuestionsByLesson.TryGetValue(lessonTitle, out var authoredFallback))
            {
                selected = authoredFallback;
            }
            else
            {
                selected = AuthoredQuestionsByLesson["Bubble Sort"];
            }
        }

        if (takeFirst > 0)
        {
            selected = selected.Take(takeFirst).ToList();
        }

        var result = new List<Question>();
        for (var i = 0; i < selected.Count; i++)
        {
            result.Add(new Question
            {
                Type = QuestionType.Single,          // quiz.json dùng correctIndex đơn → SINGLE (SDD §7.3.3)
                Content = selected[i].Text,
                OptionsJson = JsonSerializer.Serialize(selected[i].Options),
                AnswerJson = $"[{selected[i].Correct}]",
                Explanation = selected[i].Explanation,
                Points = 1,
                SortOrder = i + 1
            });
        }

        return result;
    }

    private static List<(string Text, List<string> Options, int Correct, string? Explanation)> ParseQuizJson(string json, int[] indexes)
    {
        var result = new List<(string, List<string>, int, string?)>();
        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("questions", out var questions))
        {
            return result;
        }

        var count = questions.GetArrayLength();
        foreach (var i in indexes)
        {
            if (i < 0 || i >= count)
            {
                continue;
            }

            var q = questions[i];
            var text = q.TryGetProperty("questionText", out var t) ? t.GetString() : null;
            var options = q.TryGetProperty("options", out var o) && o.ValueKind == JsonValueKind.Array
                ? o.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToList()
                : [];
            var correct = q.TryGetProperty("correctIndex", out var c) && c.TryGetInt32(out var ci) ? ci : -1;
            var explanation = q.TryGetProperty("explanation", out var e) ? e.GetString() : null;

            if (!string.IsNullOrWhiteSpace(text) && options.Count > 0 && correct >= 0 && correct < options.Count)
            {
                result.Add((text!, options, correct, explanation));
            }
        }

        return result;
    }

    // ── Source files ────────────────────────────────────────────

    private static readonly string[] ContentDraftCandidates =
    [
        "trees\\admin-content-tools\\source\\VisualizationDSA3\\plan\\content-drafts\\v2",
        "trees\\admin-content-tools\\source\\VisualizationDSA\\plan\\content-drafts\\v2",
        "source\\VisualizationDSA3\\plan\\content-drafts\\v2",
        "source\\VisualizationDSA\\plan\\content-drafts\\v2",
        "plan\\content-drafts\\v2"
    ];

    /// <summary>Tìm thư mục content-drafts bằng cách đi lên từ thư mục chạy.</summary>
    private static string? FindContentDraftsRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            foreach (var candidateRel in ContentDraftCandidates)
            {
                var candidate = Path.Combine(dir.FullName, candidateRel);
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }
            }

            dir = dir.Parent;
        }

        return null;
    }

    private static string? ReadSourceFile(string lessonDir, string fileName)
    {
        var root = FindContentDraftsRoot();
        if (root is null)
        {
            return null;
        }

        var path = Path.Combine(root, lessonDir, fileName);
        if (!File.Exists(path))
        {
            return null;
        }

        return File.ReadAllText(path, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    // ── Markdown → HTML ─────────────────────────────────────────

    /// <summary>Chuyển Markdown ĐƠN GIẢN sang HTML (h2/h3, strong, em, ul/li, pre/code, p) — đủ cho content-drafts; không dùng thư viện.</summary>
    internal static string MarkdownToHtml(string markdown)
    {
        var sb = new StringBuilder();
        var lines = markdown.Replace("\r\n", "\n").Split('\n');
        var inCode = false;
        var inList = false;

        foreach (var raw in lines)
        {
            var line = raw.TrimEnd();
            var trimmed = line.Trim();

            if (trimmed.StartsWith("```", StringComparison.Ordinal))
            {
                if (inCode)
                {
                    sb.AppendLine("</code></pre>");
                    inCode = false;
                }
                else
                {
                    sb.AppendLine("<pre><code>");
                    inCode = true;
                }

                continue;
            }

            if (inCode)
            {
                sb.AppendLine(WebUtility.HtmlEncode(line));
                continue;
            }

            if (trimmed.Length == 0)
            {
                if (inList)
                {
                    sb.AppendLine("</ul>");
                    inList = false;
                }

                continue;
            }

            if (trimmed.StartsWith("### ", StringComparison.Ordinal))
            {
                CloseList(sb, ref inList);
                sb.Append("<h3>").Append(InlineHtml(trimmed[4..])).AppendLine("</h3>");
                continue;
            }

            if (trimmed.StartsWith("## ", StringComparison.Ordinal))
            {
                CloseList(sb, ref inList);
                sb.Append("<h3>").Append(InlineHtml(trimmed[3..])).AppendLine("</h3>");
                continue;
            }

            if (trimmed.StartsWith("# ", StringComparison.Ordinal))
            {
                CloseList(sb, ref inList);
                sb.Append("<h2>").Append(InlineHtml(trimmed[2..])).AppendLine("</h2>");
                continue;
            }

            if (trimmed.StartsWith("- ", StringComparison.Ordinal) || trimmed.StartsWith("* ", StringComparison.Ordinal))
            {
                if (!inList)
                {
                    sb.AppendLine("<ul>");
                    inList = true;
                }

                sb.Append("<li>").Append(InlineHtml(trimmed[2..])).AppendLine("</li>");
                continue;
            }

            if (trimmed.StartsWith("|", StringComparison.Ordinal))
            {
                // Bảng markdown: bỏ dòng phân tách (:---), render dòng dữ liệu thành đoạn văn
                CloseList(sb, ref inList);
                if (!trimmed.Contains("---", StringComparison.Ordinal))
                {
                    var cells = trimmed.Trim('|').Split('|').Select(c => InlineHtml(c.Trim()));
                    sb.Append("<p>").Append(string.Join(" · ", cells)).AppendLine("</p>");
                }

                continue;
            }

            CloseList(sb, ref inList);
            sb.Append("<p>").Append(InlineHtml(trimmed)).AppendLine("</p>");
        }

        CloseList(sb, ref inList);
        if (inCode)
        {
            sb.AppendLine("</code></pre>");
        }

        return sb.ToString();
    }

    private static void CloseList(StringBuilder sb, ref bool inList)
    {
        if (inList)
        {
            sb.AppendLine("</ul>");
            inList = false;
        }
    }

    /// <summary>Inline: **bold**, *italic*, `code` — sanitize cuối pipeline.</summary>
    private static string InlineHtml(string text)
    {
        var encoded = WebUtility.HtmlEncode(text);
        var builder = new StringBuilder(encoded.Length + 32);

        // **bold**
        int i = 0;
        while (i < encoded.Length)
        {
            var rest = encoded[i..];

            if (rest.StartsWith("**", StringComparison.Ordinal))
            {
                var end = rest.IndexOf("**", 2, StringComparison.Ordinal);
                if (end > 0)
                {
                    builder.Append("<strong>").Append(rest[2..end]).Append("</strong>");
                    i += end + 2;
                    continue;
                }
            }

            if (rest.StartsWith("`", StringComparison.Ordinal))
            {
                var end = rest.IndexOf('`', 1);
                if (end > 0)
                {
                    builder.Append("<code>").Append(rest[1..end]).Append("</code>");
                    i += end + 1;
                    continue;
                }
            }

            if (rest.StartsWith("*", StringComparison.Ordinal))
            {
                var end = rest.IndexOf('*', 1);
                if (end > 0)
                {
                    builder.Append("<em>").Append(rest[1..end]).Append("</em>");
                    i += end + 1;
                    continue;
                }
            }

            // [text](url) → text (sanitizer không cho phép <a>)
            if (rest.StartsWith("[", StringComparison.Ordinal))
            {
                var close = rest.IndexOf(']', 1);
                var paren = close > 0 && close + 1 < rest.Length && rest[close + 1] == '(' ? rest.IndexOf(')', close) : -1;
                if (close > 0 && paren > close)
                {
                    builder.Append(rest[1..close]);
                    i += paren + 1;
                    continue;
                }
            }

            builder.Append(encoded[i]);
            i++;
        }

        return builder.ToString();
    }

    private static IHtmlSanitizer CreateSanitizer()
    {
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedTags.Add("h1");
        sanitizer.AllowedTags.Add("h2");
        sanitizer.AllowedTags.Add("h3");
        sanitizer.AllowedTags.Add("p");
        sanitizer.AllowedTags.Add("strong");
        sanitizer.AllowedTags.Add("em");
        sanitizer.AllowedTags.Add("ul");
        sanitizer.AllowedTags.Add("ol");
        sanitizer.AllowedTags.Add("li");
        sanitizer.AllowedTags.Add("pre");
        sanitizer.AllowedTags.Add("code");
        sanitizer.AllowedTags.Add("blockquote");
        sanitizer.AllowedTags.Add("br");
        return sanitizer;
    }

    // ── Cấu hình LAB / CODE ─────────────────────────────────────

    private static readonly IReadOnlyDictionary<string, int> LabMaxStepsByLesson = new Dictionary<string, int>
    {
        ["Bubble Sort"] = 12,
        ["Binary Search"] = 4,
        ["Stack"] = 6,
        ["Linked List"] = 6,
        ["BST"] = 6,
        ["AVL"] = 8,
        ["Hash Table"] = 6,
        ["BFS"] = 10,
    };

    private sealed record CodeSpec(int Count, object Config);

    private static readonly IReadOnlyDictionary<string, CodeSpec> CodeTestCasesByLesson = BuildCodeTestCases();

    private static IReadOnlyDictionary<string, CodeSpec> BuildCodeTestCases()
    {
        return new Dictionary<string, CodeSpec>
        {
            ["Bubble Sort"] = Code(
                "function bubbleSort(arr) → trả mảng đã sắp xếp tăng dần (in-place)",
                ("mảng đã sắp xếp", new[] { 5, 3, 8, 4, 2 }, new[] { 2, 3, 4, 5, 8 }),
                ("mảng đã sắp xếp sẵn", new[] { 1, 2, 3, 4, 5 }, new[] { 1, 2, 3, 4, 5 }),
                ("mảng đảo ngược hoàn toàn", new[] { 5, 4, 3, 2, 1 }, new[] { 1, 2, 3, 4, 5 }),
                ("một phần tử", new[] { 1 }, new[] { 1 }),
                ("mảng rỗng", Array.Empty<int>(), Array.Empty<int>()),
                ("phần tử trùng nhau", new[] { 3, 3, 3 }, new[] { 3, 3, 3 }),
                ("số âm", new[] { -5, 0, -2, 7 }, new[] { -5, -2, 0, 7 }),
                ("10 phần tử đảo ngược", new[] { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 }, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }),
                ("đan xen trùng", new[] { 2, 1, 2, 1, 2 }, new[] { 1, 1, 2, 2, 2 }),
                ("7 phần tử rời rạc", new[] { 64, 34, 25, 12, 22, 11, 90 }, new[] { 11, 12, 22, 25, 34, 64, 90 }),
                ("0 và 1", new[] { 0, 0, 0, 1 }, new[] { 0, 0, 0, 1 })),
            ["Binary Search"] = Code(
                "function binarySearch(arr, target) → chỉ số tìm thấy (0-based) hoặc -1; mảng đã sắp xếp tăng dần",
                ("mục tiêu ở giữa", new object[] { new[] { 2, 5, 8, 12, 16, 23, 38, 56, 72, 91 }, 23 }, 5),
                ("mục tiêu lẻ giữa", new object[] { new[] { 1, 3, 5, 7, 9, 11 }, 9 }, 4),
                ("không tồn tại", new object[] { new[] { 1, 3, 5, 7, 9, 11 }, 4 }, -1),
                ("một phần tử thấy", new object[] { new[] { 1 }, 1 }, 0),
                ("mảng rỗng", new object[] { Array.Empty<int>(), 5 }, -1),
                ("mục tiêu đầu mảng", new object[] { new[] { 1, 2, 3, 4, 5 }, 1 }, 0),
                ("mục tiêu cuối mảng", new object[] { new[] { 1, 2, 3, 4, 5 }, 5 }, 4),
                ("10 phần tử — cuối", new object[] { new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 10 }, 9),
                ("không tồn tại giữa khoảng", new object[] { new[] { 10, 20, 30, 40, 50, 60 }, 35 }, -1),
                ("số âm", new object[] { new[] { -10, -5, 0, 3, 8 }, -5 }, 1),
                ("mảng 1000 phần tử", new object[] { Enumerable.Range(0, 1000).ToArray(), 777 }, 777)),
            ["Stack"] = Code(
                "class MyStack { push(x); pop(); peek(); } — pop/peek trên stack rỗng trả null; test là dãy kết quả của các thao tác trả giá trị",
                ("LIFO cơ bản", new[] { ("push", 1), ("push", 2), ("push", 3), ("peek", 0), ("pop", 0), ("pop", 0), ("peek", 0) }, new object[] { 3, 3, 2, 1 }),
                ("pop hết stack", new[] { ("push", 1), ("push", 2), ("push", 3), ("push", 4), ("pop", 0), ("pop", 0), ("pop", 0), ("pop", 0) }, new object[] { 4, 3, 2, 1 }),
                ("pop trên stack rỗng", new[] { ("push", 5), ("pop", 0), ("pop", 0) }, new object?[] { 5, null }),
                ("peek không xóa", new[] { ("push", 7), ("peek", 0), ("peek", 0), ("pop", 0) }, new object[] { 7, 7, 7 }),
                ("xen kẽ push/pop", new[] { ("push", 1), ("pop", 0), ("push", 2), ("pop", 0), ("push", 3), ("pop", 0) }, new object[] { 1, 2, 3 }),
                ("đảo thứ tự", new[] { ("push", 10), ("push", 20), ("peek", 0), ("pop", 0), ("peek", 0) }, new object[] { 20, 20, 10 }),
                ("chuỗi 100 phần tử", Enumerable.Range(1, 100).Select(i => ("push", i)).Append(("peek", 0)).ToArray(), new object[] { 100 }),
                ("hai chiều lồng nhau", new[] { ("push", 42), ("peek", 0), ("push", 43), ("peek", 0), ("pop", 0), ("pop", 0) }, new object[] { 42, 43, 43, 42 }),
                ("pop về rỗng rồi push lại", new[] { ("push", 1), ("push", 2), ("pop", 0), ("pop", 0), ("push", 9), ("peek", 0) }, new object[] { 2, 1, 9 }),
                ("giá trị trùng", new[] { ("push", 5), ("push", 5), ("pop", 0), ("peek", 0) }, new object[] { 5, 5 }),
                ("số âm", new[] { ("push", -1), ("push", -2), ("peek", 0) }, new object[] { -2 })),
            ["Linked List"] = Code(
                "class MyLinkedList { insertHead(val); insertTail(val); delete(val); toArray(); } — toArray() trả mảng theo thứ tự Head→Tail",
                ("chèn đầu 3,2,1", new[] { ("insertHead", 3), ("insertHead", 2), ("insertHead", 1) }, new[] { 1, 2, 3 }),
                ("chèn cuối 1,2,3", new[] { ("insertTail", 1), ("insertTail", 2), ("insertTail", 3) }, new[] { 1, 2, 3 }),
                ("hỗn hợp đầu/cuối", new[] { ("insertHead", 1), ("insertTail", 3), ("insertHead", 0), ("insertTail", 4) }, new[] { 0, 1, 3, 4 }),
                ("xóa giữa", new[] { ("insertTail", 1), ("insertTail", 2), ("insertTail", 3), ("delete", 2) }, new[] { 1, 3 }),
                ("xóa đầu", new[] { ("insertTail", 1), ("insertTail", 2), ("delete", 1) }, new[] { 2 }),
                ("xóa cuối", new[] { ("insertTail", 1), ("insertTail", 2), ("delete", 2) }, new[] { 1 }),
                ("xóa phần tử không tồn tại", new[] { ("insertTail", 1), ("delete", 99) }, new[] { 1 }),
                ("danh sách rỗng", Array.Empty<(string, int)>(), Array.Empty<int>()),
                ("một node rồi xóa sạch", new[] { ("insertTail", 7), ("delete", 7) }, Array.Empty<int>()),
                ("5 node duyệt tuần tự", new[] { ("insertTail", 1), ("insertTail", 2), ("insertTail", 3), ("insertTail", 4), ("insertTail", 5) }, new[] { 1, 2, 3, 4, 5 }),
                ("giá trị trùng", new[] { ("insertTail", 2), ("insertTail", 2), ("delete", 2) }, new[] { 2 })),
            ["BST"] = Code(
                "class BST { insert(val); search(val) → boolean; inorder() → mảng tăng dần }",
                ("chèn + inorder", new[] { 20, 10, 30, 12, 15 }, new[] { 10, 12, 15, 20, 30 }),
                ("chèn tăng dần", new[] { 1, 2, 3, 4, 5 }, new[] { 1, 2, 3, 4, 5 }),
                ("chèn giảm dần", new[] { 5, 4, 3, 2, 1 }, new[] { 1, 2, 3, 4, 5 }),
                ("tìm thấy", new[] { 20, 10, 30 }, 15, false),
                ("tìm thấy ở gốc", new[] { 20, 10, 30 }, 20, true),
                ("tìm không thấy", new[] { 20, 10, 30 }, 25, false),
                ("trùng lặp bỏ qua", new[] { 10, 10, 20 }, new[] { 10, 20 }),
                ("10 node rời rạc", new[] { 50, 25, 75, 10, 30, 60, 90, 5, 40, 80 }, new[] { 5, 10, 25, 30, 40, 50, 60, 75, 80, 90 }),
                ("tìm lá", new[] { 50, 25, 75 }, 75, true),
                ("tìm giá trị âm", new[] { -5, -10, 0, 7 }, -10, true),
                ("inorder mảng một phần tử", new[] { 42 }, new[] { 42 })),
            ["AVL"] = Code(
                "class AVLTree { insert(val); height() → số mức (gốc = 1); inorder() } — cây luôn cân bằng sau mỗi insert",
                ("xoay trái LL: chèn 10,20,30", new[] { 10, 20, 30 }, 2, new[] { 10, 20, 30 }),
                ("xoay phải RR: chèn 30,20,10", new[] { 30, 20, 10 }, 2, new[] { 10, 20, 30 }),
                ("chèn 4 node", new[] { 10, 20, 30, 40 }, 3, new[] { 10, 20, 30, 40 }),
                ("7 node liên tiếp", new[] { 10, 20, 30, 40, 50, 60, 70 }, 3, new[] { 10, 20, 30, 40, 50, 60, 70 }),
                ("5 node tăng dần", new[] { 1, 2, 3, 4, 5 }, 3, new[] { 1, 2, 3, 4, 5 }),
                ("5 node giảm dần", new[] { 5, 4, 3, 2, 1 }, 3, new[] { 1, 2, 3, 4, 5 }),
                ("LR: chèn 30,10,20", new[] { 30, 10, 20 }, 2, new[] { 10, 20, 30 }),
                ("RL: chèn 10,30,20", new[] { 10, 30, 20 }, 2, new[] { 10, 20, 30 }),
                ("8 node hỗn hợp", new[] { 50, 25, 75, 10, 30, 60, 90, 5 }, 4, new[] { 5, 10, 25, 30, 50, 60, 75, 90 }),
                ("15 node liên tiếp", Enumerable.Range(1, 15).ToArray(), 4, Enumerable.Range(1, 15).ToArray()),
                ("100 node giữ chiều cao thấp", Enumerable.Range(1, 100).ToArray(), 9, Enumerable.Range(1, 100).ToArray())),
            ["Hash Table"] = Code(
                "class MyHashMap { put(k,v); get(k) → v hoặc null; contains(k) → boolean; remove(k) }",
                ("put/get cơ bản", new[] { ("put", "a", 1), ("get", "a", 0), ("contains", "a", 0) }, new object[] { 1, true }),
                ("ghi đè giá trị", new[] { ("put", "a", 1), ("put", "a", 2), ("get", "a", 0) }, new object[] { 2 }),
                ("get key không tồn tại", new[] { ("get", "missing", 0) }, new object?[] { null }),
                ("contains không tồn tại", new[] { ("contains", "missing", 0) }, new object[] { false }),
                ("remove rồi get", new[] { ("put", "a", 1), ("remove", "a", 0), ("get", "a", 0), ("contains", "a", 0) }, new object?[] { null, false }),
                ("100 key số nguyên", Enumerable.Range(1, 100).Select(i => ("put", i.ToString(), i)).Append(("get", "50", 0)).ToArray(), new object[] { 50 }),
                ("va chạm chuỗi", new[] { ("put", "abc", 1), ("put", "bca", 2), ("get", "abc", 0), ("get", "bca", 0) }, new object[] { 1, 2 }),
                ("remove không tồn tại", new[] { ("remove", "void", 0), ("contains", "void", 0) }, new object[] { false }),
                ("key số nguyên mixed", new[] { ("put", "5", 50), ("get", "5", 0) }, new object[] { 50 }),
                ("bảng rỗng", new[] { ("contains", "x", 0) }, new object[] { false }),
                ("nhiều key khác nhau", new[] { ("put", "k1", 1), ("put", "k2", 2), ("put", "k3", 3), ("get", "k2", 0) }, new object[] { 2 })),
            ["BFS"] = Code(
                "function bfs(graph, start) → mảng thứ tự duyệt theo tầng (adjacency list, khóa chuỗi hoặc số)",
                ("đồ thị 4 đỉnh", new Dictionary<string, object> { ["0"] = new[] { 1, 2 }, ["1"] = new[] { 3 }, ["2"] = new[] { 3 }, ["3"] = Array.Empty<int>() }, "0", new[] { "0", "1", "2", "3" }),
                ("đồ thị chữ cái", new Dictionary<string, object> { ["A"] = new[] { "B", "C" }, ["B"] = new[] { "D", "E" }, ["C"] = new[] { "F" }, ["D"] = Array.Empty<string>(), ["E"] = Array.Empty<string>(), ["F"] = Array.Empty<string>() }, "A", new[] { "A", "B", "C", "D", "E", "F" }),
                ("một đỉnh", new Dictionary<string, object> { ["0"] = Array.Empty<int>() }, "0", new[] { "0" }),
                ("đồ thị không liên thông", new Dictionary<string, object> { ["0"] = new[] { 1 }, ["1"] = new[] { 2 }, ["2"] = Array.Empty<int>(), ["3"] = new[] { 4 }, ["4"] = Array.Empty<int>() }, "0", new[] { "0", "1", "2" }),
                ("đồ thị sao", new Dictionary<string, object> { ["0"] = new[] { 1, 2, 3 }, ["1"] = Array.Empty<int>(), ["2"] = Array.Empty<int>(), ["3"] = Array.Empty<int>() }, "0", new[] { "0", "1", "2", "3" }),
                ("đồ thị có chu trình", new Dictionary<string, object> { ["0"] = new[] { 1 }, ["1"] = new[] { 0 } }, "0", new[] { "0", "1" }),
                ("đồ thị 6 đỉnh rẽ nhánh", new Dictionary<string, object> { ["0"] = new[] { 1, 4 }, ["1"] = new[] { 2 }, ["2"] = new[] { 3 }, ["3"] = Array.Empty<int>(), ["4"] = new[] { 5 }, ["5"] = Array.Empty<int>() }, "0", new[] { "0", "1", "4", "2", "5", "3" }),
                ("bắt đầu từ giữa", new Dictionary<string, object> { ["0"] = new[] { 1 }, ["1"] = new[] { 2, 3 }, ["2"] = Array.Empty<int>(), ["3"] = new[] { 4 }, ["4"] = Array.Empty<int>() }, "2", new[] { "2" }),
                ("chuỗi 5 đỉnh", new Dictionary<string, object> { ["0"] = new[] { 1 }, ["1"] = new[] { 2 }, ["2"] = new[] { 3 }, ["3"] = new[] { 4 }, ["4"] = Array.Empty<int>() }, "0", new[] { "0", "1", "2", "3", "4" }),
                ("start không có cạnh đi", new Dictionary<string, object> { ["0"] = new[] { 1 }, ["1"] = Array.Empty<int>() }, "1", new[] { "1" }),
                ("đồ thị rỗng — chỉ start", new Dictionary<string, object>(), "x", new[] { "x" })),
        };
    }

    private static CodeSpec Code(string signature, params object[] testCases) => new(testCases.Length, new { signature, language = "javascript", testCases });

    // ── Authored content (AVL — không có bài nguồn riêng) ───────

    private static readonly IReadOnlyDictionary<string, List<(string Text, List<string> Options, int Correct, string? Explanation)>> AuthoredQuestionsByLesson =
        new Dictionary<string, List<(string, List<string>, int, string?)>>
        {
            ["Bubble Sort"] =
            [
                ("Trong Bubble Sort, sau mỗi lượt duyệt ngoài cùng (outer pass), phần tử nào chắc chắn về đúng vị trí?",
                    ["Phần tử lớn nhất trong đoạn chưa sắp xếp nổi về cuối", "Phần tử nhỏ nhất", "Phần tử ở vị trí giữa", "Không xác định"], 0,
                    "Mỗi lượt so sánh cặp liền kề đẩy giá trị lớn nhất còn lại về vị trí cuối cùng của đoạn."),
                ("Độ phức tạp thời gian trung bình và xấu nhất của Bubble Sort là gì?",
                    ["O(N²)", "O(N log N)", "O(N)", "O(1)"], 0,
                    "Bubble Sort cần 2 vòng lặp lồng nhau nên độ phức tạp trung bình và tệ nhất đều là O(N²)."),
                ("Thuật toán Bubble Sort có tính ổn định (Stable Sort) không?",
                    ["Có, vì chỉ đổi chỗ khi phần tử trước lớn hơn hẳn phần tử sau", "Không bao giờ ổn định", "Tùy thuộc vào dữ liệu", "Chỉ ổn định với số nguyên"], 0,
                    "Các phần tử có giá trị bằng nhau không bị đổi chỗ nên giữ nguyên thứ tự tương đối ban đầu."),
                ("Khi mảng đã được sắp xếp sẵn tăng dần, Bubble Sort có cờ swapped tối ưu chạy trong bao lâu?",
                    ["O(N)", "O(N²)", "O(log N)", "O(N log N)"], 0,
                    "Chỉ cần 1 lượt duyệt kiểm tra không có swap nào xảy ra → dừng ngay trong O(N)."),
                ("Thao tác cơ bản nhất diễn ra liên tục trong Bubble Sort là gì?",
                    ["So sánh cặp liền kề và đổi chỗ (Swap)", "Chia đôi mảng", "Tìm kiếm nhị phân", "Xây dựng cây nhị phân"], 0,
                    "Bubble Sort so sánh arr[j] và arr[j+1], nếu arr[j] > arr[j+1] thì đổi chỗ."),
            ],
            ["Binary Search"] =
            [
                ("Điều kiện tiên quyết bắt buộc để áp dụng Binary Search trên một mảng là gì?",
                    ["Mảng phải được sắp xếp trước", "Mảng phải chứa toàn số dương", "Kích thước mảng phải là lũy thừa của 2", "Mảng không chứa phần tử trùng"], 0,
                    "Binary Search dựa vào tính thứ tự để loại bỏ một nửa không gian tìm kiếm sau mỗi bước."),
                ("Độ phức tạp thời gian của Binary Search trong trường hợp xấu nhất là gì?",
                    ["O(log N)", "O(N)", "O(N²)", "O(1)"], 0,
                    "Sau mỗi phép so sánh, kích thước bài toán giảm đi một nửa: N → N/2 → N/4 ... mất tối đa log2(N) bước."),
                ("Khi không tìm thấy phần tử mục tiêu, hàm Binary Search chuẩn thường trả về giá trị nào?",
                    ["-1", "0", "null", "Vô cùng"], 0,
                    "Quy ước -1 biểu thị chỉ số không hợp lệ trong mảng."),
                ("Công thức an toàn tránh tràn số nguyên khi tính chỉ số giữa (mid) là gì?",
                    ["mid = left + (right - left) / 2", "mid = (left + right) / 2", "mid = right - left", "mid = left * 2"], 0,
                    "Phép cộng left + right có thể vượt quá giới hạn INT_MAX; dùng left + (right - left) / 2 an toàn tuyệt đối."),
            ],
            ["Stack"] =
            [
                ("Ngăn xếp (Stack) hoạt động theo nguyên lý nào?",
                    ["LIFO (Last In First Out - Vào sau Ra trước)", "FIFO (First In First Out)", "Ưu tiên theo giá trị", "Ngẫu nhiên"], 0,
                    "Phần tử được đưa vào cuối cùng sẽ là phần tử đầu tiên được lấy ra."),
                ("Thao tác nào lấy phần tử ra khỏi đỉnh ngăn xếp?",
                    ["pop()", "push()", "peek()", "enqueue()"], 0,
                    "push thêm vào đỉnh, pop lấy ra khỏi đỉnh, peek xem đỉnh mà không lấy ra."),
                ("Ứng dụng nào sau đây thường sử dụng cấu trúc Ngăn xếp (Stack)?",
                    ["Kiểm tra tính hợp lệ của dấu ngoặc và Undo/Redo", "Hàng đợi in ấn", "Tìm đường đi ngắn nhất đồ thị", "Bảng tra cứu từ điển"], 0,
                    "Kiểm tra cặp ngoặc lồng nhau và lịch sử hoàn tác (Undo) là ứng dụng kinh điển của LIFO Stack."),
            ],
            ["Linked List"] =
            [
                ("Ưu điểm nổi bật của Linked List so với Mảng tĩnh (Array) là gì?",
                    ["Chèn/xóa ở đầu danh sách trong O(1) không cần dời các phần tử", "Truy cập ngẫu nhiên theo index trong O(1)", "Tiết kiệm bộ nhớ hơn mảng", "Tự động sắp xếp"], 0,
                    "Linked List chỉ cần thay đổi con trỏ Head trong O(1), trong khi Array phải dịch chuyển toàn bộ N phần tử."),
                ("Mỗi Node trong Danh sách liên kết đơn (Singly Linked List) chứa những gì?",
                    ["Giá trị (Data) và Con trỏ tới Node tiếp theo (Next)", "Chỉ chứa Giá trị", "Con trỏ Next và Con trỏ Prev", "Mảng các phần tử"], 0,
                    "Node đơn gồm vùng chứa dữ liệu (val) và con trỏ next liên kết tới node kế tiếp."),
            ],
            ["BST"] =
            [
                ("Trong Cây nhị phân tìm kiếm (BST), tính chất nào luôn đúng với mọi Node X?",
                    ["Mọi node ở cây con trái < X và mọi node ở cây con phải > X", "Cây con trái luôn có số node bằng cây con phải", "Node lá luôn có giá trị chẵn", "Chiều cao luôn là O(log N)"], 0,
                    "Tính chất cốt lõi: Cây con trái < Gốc < Cây con phải."),
                ("Phép duyệt cây nào trên BST cho ra dãy giá trị theo thứ tự TĂNG DẦN?",
                    ["Duyệt trung thứ tự (In-order: Trái - Gốc - Phải)", "Duyệt tiền thứ tự (Pre-order: Gốc - Trái - Phải)", "Duyệt hậu thứ tự (Post-order)", "Duyệt theo tầng (BFS)"], 0,
                    "In-order duyệt Trái → Gốc → Phải trên BST luôn sinh ra dãy số đã sắp xếp tăng dần."),
            ],
            ["Hash Table"] =
            [
                ("Bảng băm (Hash Table) đạt độ phức tạp thời gian trung bình cho thao tác tra cứu/chèn là bao nhiêu?",
                    ["O(1)", "O(log N)", "O(N)", "O(N²)"], 0,
                    "Hàm băm ánh xạ trực tiếp key sang index mảng, cho phép tra cứu trung bình O(1)."),
                ("Phương pháp xử lý va chạm bằng danh sách liên kết tại mỗi ô gọi là gì?",
                    ["Separate Chaining (Nối chuỗi)", "Linear Probing", "Quadratic Probing", "Double Hashing"], 0,
                    "Chaining lưu các phần tử trùng hash vào một danh sách liên kết tại bucket đó."),
            ],
            ["BFS"] =
            [
                ("Thuật toán Duyệt theo chiều rộng (BFS) sử dụng cấu trúc dữ liệu nào để lưu các đỉnh chờ duyệt?",
                    ["Hàng đợi (Queue - FIFO)", "Ngăn xếp (Stack - LIFO)", "Cây nhị phân", "Bảng băm"], 0,
                    "BFS thăm các đỉnh theo từng lớp sóng lan tỏa (tầng), do đó dùng Queue (FIFO) để bảo đảm thứ tự trước-sau."),
                ("BFS trên đồ thị không trọng số có thể dùng để giải quyết bài toán nào?",
                    ["Tìm đường đi ngắn nhất (ít cạnh nhất) từ đỉnh nguồn", "Tìm cây khung nhỏ nhất", "Sắp xếp topo", "Tìm chu trình Euler"], 0,
                    "Vì BFS duyệt theo từng tầng khoảng cách tăng dần, đường đi đầu tiên tìm thấy tới đích luôn là ngắn nhất."),
            ],
            ["AVL"] =
            [
                ("Hệ số cân bằng (balance factor) của một node trong cây AVL được tính như thế nào?",
                    ["Chiều cao cây con phải trừ chiều cao cây con trái", "Số node con trừ số node phải", "Tổng số node của cây con", "Chiều cao cây con trái cộng chiều cao cây con phải"], 0,
                    "Balance factor = height(right) − height(left); ngoài khoảng [-1, 1] nghĩa là node mất cân bằng."),
                ("AVL yêu cầu hệ số cân bằng của MỌI node nằm trong khoảng nào?",
                    ["[-2, 2]", "[-1, 1]", "[0, 1]", "[−∞, +∞]"], 1,
                    "AVL giữ |balance factor| ≤ 1 ở mọi node; vi phạm ngay sau khi chèn/xóa sẽ kích hoạt xoay."),
                ("Chèn lần lượt 10, 20, 30 vào AVL rỗng. Cần thao tác gì để cây cân bằng lại?",
                    ["Xoay trái quanh 10 (Left Rotation)", "Xoay phải quanh 30", "Xoay kép trái-phải", "Không cần xoay — cây vẫn cân bằng"], 0,
                    "10 → 20 → 30 tạo chuỗi lệch phải (RR): xoay trái quanh 10, 20 trở thành gốc, cây cân bằng."),
                ("Khi nào cần xoay KÉP (LR hoặc RL) để cân bằng AVL?",
                    ["Khi node mất cân bằng và đứa con gây mất cân bằng nằm ngược hướng (trái-phải hoặc phải-trái)", "Khi chèn giá trị trùng lặp", "Khi cây có hơn 10 node", "Khi xóa node lá"], 0,
                    "LR: chèn vào con trái của con phải... Xoay đơn không đủ — cần xoay con trước rồi xoay gốc."),
                ("Chiều cao của cây AVL chứa N node luôn nằm trong khoảng nào?",
                    ["O(N)", "O(N log N)", "O(log N)", "O(1)"], 2,
                    "AVL đảm bảo chiều cao tối đa ≈ 1,44·log2(N) nhờ ràng buộc cân bằng — mọi thao tác O(log N)."),
                ("Độ phức tạp thời gian XẤU NHẤT của insert/search/delete trên AVL là gì?",
                    ["O(N)", "O(log N)", "O(N²)", "O(1)"], 1,
                    "Khác BST thường (có thể lệch thành O(N)), AVL tự xoay giữ chiều cao O(log N) nên xấu nhất vẫn O(log N)."),
                ("Điểm khác biệt cốt lõi giữa AVL và BST thường là gì?",
                    ["AVL chạy nhanh hơn vì dùng hàm băm", "AVL tự xoay sau mỗi chèn/xóa để giữ cân bằng, đảm bảo O(log N); BST thường có thể lệch thành danh sách O(N)", "AVL chỉ lưu số nguyên", "AVL không cho phép xóa node"], 1,
                    "BST chèn dãy đã sắp xếp → cây lệch O(N); AVL phát hiện |bf| > 1 và xoay LL/RR/LR/RL để cân bằng lại ngay."),
            ]
        };

    private static class AuthoredContent
    {
        /// <summary>Nội dung AVL tự soạn (SDD §4.7 — Ý tưởng / Minh họa / Độ phức tạp) vì 40 bài nguồn không có bài AVL riêng.</summary>
        public const string AvlMarkdown =
            "# Cây AVL — BST tự cân bằng\n" +
            "\n" +
            "## 1. Động cơ học\n" +
            "BST chèn dãy đã sắp xếp (10, 20, 30, 40...) sẽ thoái hóa thành danh sách liên kết — mọi thao tác rơi về O(N). Cây AVL (Adelson-Velsky & Landis) khắc phục bằng cách **tự xoay lại sau mỗi lần chèn/xóa** để chiều cao luôn là O(log N) — nền tảng cho từ điển, index database và bộ nhớ đệm cần thao tác nhanh ổn định.\n" +
            "\n" +
            "## 2. Ý tưởng cốt lõi\n" +
            "- **Hệ số cân bằng (balance factor)** của một node: `bf = height(phải) − height(trái)`; AVL yêu cầu `|bf| ≤ 1` ở **mọi node**.\n" +
            "- Sau mỗi chèn/xóa, đi từ node bị ảnh hưởng ngược lên gốc; node nào `|bf| > 1` sẽ được cân bằng bằng **xoay**.\n" +
            "- **Bốn trường hợp xoay**: LL → xoay phải; RR → xoay trái; LR → xoay trái con rồi xoay phải gốc; RL → xoay phải con rồi xoay trái gốc.\n" +
            "- Xoay chỉ nối lại con trỏ — O(1) phép gán, không cấp phát node mới; bất biến BST (trái < node < phải) được giữ nguyên.\n" +
            "\n" +
            "## 3. Minh họa: chèn 10, 20, 30 vào AVL rỗng\n" +
            "1. Chèn 10 làm gốc.\n" +
            "2. Chèn 20 làm con phải của 10 → cân bằng (bf = 1).\n" +
            "3. Chèn 30 làm con phải của 20 → node 10 có bf = 2, mất cân bằng dạng RR.\n" +
            "4. **Xoay trái quanh 10**: 20 lên làm gốc, 10 thành con trái của 20, 30 là con phải — cây cân bằng, chiều cao 2.\n" +
            "\n" +
            "Chèn tiếp 40, 50, 60: mỗi lần vượt ngưỡng, AVL xoay đúng chỗ — kết quả 7 node chỉ cao 3 mức, thay vì 7 mức như BST lệch.\n" +
            "\n" +
            "## 4. Độ phức tạp & so sánh\n" +
            "| Thao tác | BST thường (xấu nhất) | AVL (xấu nhất) |\n" +
            "| :--- | :--- | :--- |\n" +
            "| Insert | O(N) — cây lệch | O(log N) — tự xoay |\n" +
            "| Search | O(N) | O(log N) |\n" +
            "| Delete | O(N) | O(log N) |\n" +
            "- Bộ nhớ: O(N) + hằng số cho chiều cao/node — chi phí xoay thêm rất nhỏ.\n" +
            "- Chiều cao tối đa của AVL ≈ 1,44·log2(N) — chặt hơn Red-Black (2·log2(N)) nhưng xoay nhiều hơn khi chèn; chọn AVL khi **đọc nhiều**, Red-Black khi **ghi nhiều**.\n" +
            "\n" +
            "## 5. Tổng kết\n" +
            "- AVL = BST + ràng buộc `|bf| ≤ 1` + 4 phép xoay (LL/RR/LR/RL) sau mỗi thay đổi.\n" +
            "- Mọi thao tác luôn O(log N) bất kể thứ tự dữ liệu vào — khác BST thường có thể O(N).\n" +
            "- Bẫy thường gặp: quên cập nhật chiều cao khi xoay; xoay sai hướng (nhầm LL với LR); chỉ kiểm tra cân bằng ở gốc thay vì mọi node.\n" +
            "\n" +
            "## 📚 Tham khảo\n" +
            "- Sách: Introduction to Algorithms (CLRS) — chương AVL/Red-Black Trees.\n" +
            "- Coursera: Data Structures and Algorithms Specialization (UC San Diego).";

        /// <summary>Nội dung fallback nếu thiếu file nguồn (không nên xảy ra trong repo này).</summary>
        public static string GenericMarkdown(string lessonTitle) =>
            $"# {lessonTitle}\n\n" +
            "## 1. Ý tưởng\n" +
            $"Bản tóm tắt ngắn về **{lessonTitle}** — khái niệm trung tâm, bất biến và ý nghĩa thực tế của bài học.\n\n" +
            "## 2. Minh họa\n" +
            "- Ví dụ từng bước với dữ liệu nhỏ để quan sát hành vi của thuật toán/cấu trúc.\n" +
            "- Bẫy thường gặp khi cài đặt và cách tránh.\n\n" +
            "## 3. Độ phức tạp\n" +
            "- Thời gian: phụ thuộc bài — xem mô phỏng tương ứng trong bài.\n" +
            "- Bộ nhớ: ghi chú ngắn về chi phí phụ trợ.";
    }
}
