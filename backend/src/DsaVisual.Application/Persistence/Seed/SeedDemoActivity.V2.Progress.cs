using DsaVisual.Application.Common;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Persistence.Seed;

/// <summary>
/// SEED-V2 (PROMPT_K_SEED_PROD_V2) — Task 3: UserProgress + UserNodeProgress + ExerciseSubmissions
/// cho 69 user V2 (68 student @university.edu.vn theo 4 persona + showcase@demo.local). Pattern y hệt
/// V1 SeedDemoActivity.Progress.cs: BuildPlanV2 deterministic (Random cố định <see cref="PlanSeedV2"/>,
/// Index deterministic của V2Users) → 3 method dùng CHUNG 1 kế hoạch nên UserProgress.CompletedAt /
/// UserNodeProgress.PassedAt khớp CHÍNH XÁC ExerciseSubmission.SubmittedAt của bài full-score tương ứng;
/// chạy lại lần 2 → guard bỏ qua hết (0 dòng thêm).
///
/// Kế hoạch theo persona (tổng dự kiến — khớp khoảng mục tiêu Task 3):
///   UserNodeProgress ~602 (Showcase 18 / Hardworking 213 / Average 305 / Slacker 51 / New 15),
///   UserProgress ~377 (8 / 104 / 240 / 20 / 5),
///   ExerciseSubmissions ~254 (18 / 109 / 113 / 14 / 0).
///
/// Rule hệ thống (bắt buộc): UserNodeProgress.Status=2 → CÓ ExerciseSubmission Score == MaxScore của
/// exercise gắn node (NodeScore = MaxScore, Stars = 3, PassedAt = SubmittedAt); không tìm thấy bài full
/// → hạ Status=1 cho an toàn. Node luyện tập tổng hợp KHÔNG có exercise (LessonId=null, FinalTestId=null)
/// → chỉ đạt Status=1 (rule V1). ClassAssignmentId luôn null (task SEED-3 gắn sau).
/// KHÔNG sửa file V1, KHÔNG wire vào SeedDemoActivity.cs (Task 6 sẽ thêm call).
/// </summary>
public static partial class SeedDemoActivity
{
    /// <summary>Seed cố định riêng cho kế hoạch V2 (KHÁC hằng PlanSeed của file V1 — không đổi hằng V1).</summary>
    private const int PlanSeedV2 = 20260814;

    // ── 1. UserProgress V2 ──────────────────────────────────────

    /// <summary>
    /// ~377 dòng UserProgress V2: mỗi user 0-8 lesson (DB chỉ có 8 lesson active); Viewed=1,
    /// SimulationCount ≥ 1, BestScore = max Score bài nộp trong lesson (null nếu chưa nộp),
    /// CompletedAt = SubmittedAt bài nộp full-score ĐẦU TIÊN của lesson (null nếu không có),
    /// UpdatedAt = max SubmittedAt (hoặc mốc gần đây nếu lesson chỉ xem). Guard (UserId, LessonId).
    /// </summary>
    public static async Task SeedUserProgressV2Async(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        var now = NowUtc7(clock);
        var (users, lessons, exercises, nodes) = await LoadV2ContextAsync(db, ct);
        var plan = BuildPlanV2(users, lessons, exercises, nodes, now);

        var added = 0;
        var skipped = 0;
        foreach (var student in plan)
        {
            foreach (var row in student.Lessons)
            {
                var exists = await db.UserProgress.AnyAsync(
                    p => p.UserId == student.User.Id && p.LessonId == row.Lesson.Id, ct);
                if (exists)
                {
                    skipped++;
                    continue;
                }

                db.UserProgress.Add(new UserProgress
                {
                    UserId = student.User.Id,
                    LessonId = row.Lesson.Id,
                    Viewed = true,
                    SimulationCount = row.SimulationCount,
                    BestScore = row.BestScore,
                    CompletedAt = row.CompletedAt,
                    UpdatedAt = row.UpdatedAt
                });
                added++;
            }
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seed: V2 UserProgress thêm {Added} / bỏ qua {Skipped} (đã tồn tại)", added, skipped);
    }

    // ── 2. UserNodeProgress V2 ──────────────────────────────────

    /// <summary>
    /// ~602 dòng UserNodeProgress V2: Showcase pass 100% 5 lộ trình (13 node có exercise Status=2 +
    /// 5 node luyện tập Status=1); Hardworking pass 2 lộ trình đầu (6-8 node) + nhiều node đang học;
    /// Average 1 lộ trình (2-4 node pass); Slacker 0-1 node pass; New 1-2 node đang học (0 pass).
    /// Status=2 BẮT BUỘC có bài nộp full-score của exercise gắn node (PassedAt = SubmittedAt bài đó;
    /// NodeScore = MaxScore; Stars = 3); không tìm thấy bài full → hạ Status=1 cho an toàn.
    /// Guard (UserId, NodeId).
    /// </summary>
    public static async Task SeedUserNodeProgressV2Async(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        var now = NowUtc7(clock);
        var (users, lessons, exercises, nodes) = await LoadV2ContextAsync(db, ct);
        var plan = BuildPlanV2(users, lessons, exercises, nodes, now);

        var added = 0;
        var skipped = 0;
        foreach (var student in plan)
        {
            foreach (var row in student.Nodes)
            {
                var exists = await db.UserNodeProgress.AnyAsync(
                    p => p.UserId == student.User.Id && p.NodeId == row.Node.Id, ct);
                if (exists)
                {
                    skipped++;
                    continue;
                }

                db.UserNodeProgress.Add(new UserNodeProgress
                {
                    UserId = student.User.Id,
                    NodeId = row.Node.Id,
                    Status = row.Status,
                    Stars = row.Stars,
                    NodeScore = row.NodeScore,
                    UnlockedAt = row.UnlockedAt,
                    PassedAt = row.PassedAt,
                    UpdatedAt = row.UpdatedAt
                });
                added++;
            }
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seed: V2 UserNodeProgress thêm {Added} / bỏ qua {Skipped} (đã tồn tại)", added, skipped);
    }

    // ── 3. ExerciseSubmissions V2 ───────────────────────────────

    /// <summary>
    /// ~254 bài nộp V2: bài full-score cho TỪNG node Status=2 (khớp PassedAt/CompletedAt) + bài luyện
    /// thêm (điểm đa dạng theo persona: Hardworking 74-99, Average 49-99, Slacker 0-50, Showcase 79-99);
    /// SubmittedAt rải deterministic từ CreatedAt → hôm nay; ClassAssignmentId = null. Không có UNIQUE
    /// → guard (user, exercise, score): bài full-score chỉ thêm khi user chưa có submission full-score
    /// cho exercise đó; bài luyện thêm luôn chọn exercise KHÁC bài pass (điểm khác MaxScore).
    /// </summary>
    public static async Task SeedExerciseSubmissionsV2Async(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        var now = NowUtc7(clock);
        var (users, lessons, exercises, nodes) = await LoadV2ContextAsync(db, ct);
        var plan = BuildPlanV2(users, lessons, exercises, nodes, now);

        var exerciseIds = plan.SelectMany(p => p.Submissions).Select(s => s.Exercise.Id).Distinct().ToList();
        var questions = await db.Questions.AsNoTracking()
            .Where(q => exerciseIds.Contains(q.ExerciseId))
            .ToListAsync(ct);
        var questionsByExercise = questions
            .GroupBy(q => q.ExerciseId)
            .ToDictionary(g => g.Key, g => g.OrderBy(q => q.SortOrder).ToList());

        var added = 0;
        var skipped = 0;
        foreach (var student in plan)
        {
            foreach (var sub in student.Submissions)
            {
                var exists = await db.ExerciseSubmissions.AnyAsync(
                    s => s.UserId == student.User.Id && s.ExerciseId == sub.Exercise.Id && s.Score == sub.Score, ct);
                if (exists)
                {
                    skipped++;
                    continue;
                }

                var rng = new Random(PlanSeedV2 + sub.Exercise.Id * 31 + sub.Score * 7);
                var (answersJson, resultJson) = BuildSubmissionJson(sub.Exercise, sub.Score, questionsByExercise, rng);
                db.ExerciseSubmissions.Add(new ExerciseSubmission
                {
                    UserId = student.User.Id,
                    ExerciseId = sub.Exercise.Id,
                    ClassAssignmentId = null,          // task SEED-3 gắn ClassAssignment sau
                    Score = sub.Score,
                    AnswersJson = answersJson,
                    ResultJson = resultJson,
                    DurationSeconds = 30 + rng.Next(0, 20) * 30,
                    SubmittedAt = sub.SubmittedAt
                });
                added++;
            }
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seed: V2 ExerciseSubmissions thêm {Added} / bỏ qua {Skipped} (đã tồn tại)", added, skipped);
    }

    // ── Kế hoạch dữ liệu deterministic V2 (dùng chung 3 method) ──

    /// <summary>
    /// Số node PASS (Status=2) theo persona — Showcase 13 node có exercise (8 bài học + 5 kiểm tra cuối),
    /// Hardworking 6-8, Average 2-4, Slacker 0-1, New 0. Sinh deterministic theo Index (KHÔNG random).
    /// </summary>
    private static int PassCountV2(string persona, int index) => persona switch
    {
        V2Users.ShowcasePersona => 13,
        V2Users.HardworkingPersona => 6 + index % 3,
        V2Users.AveragePersona => 2 + index % 3,
        V2Users.SlackerPersona => index % 3 == 0 ? 1 : 0,
        _ => 0
    };

    /// <summary>
    /// Số node đang học (Status=1) theo persona — Showcase 5 node luyện tập tổng hợp, Hardworking 9-10,
    /// Average 6-7, Slacker 3-4, New 1-2. Sinh deterministic theo Index.
    /// </summary>
    private static int ActiveCountV2(string persona, int index) => persona switch
    {
        V2Users.ShowcasePersona => 5,
        V2Users.HardworkingPersona => 9 + index % 2,
        V2Users.AveragePersona => 6 + index % 2,
        V2Users.SlackerPersona => 3 + index % 2,
        _ => 1 + index % 2
    };

    /// <summary>Số dòng UserProgress theo persona — Showcase/Hardworking 8 lesson (tối đa DB), Average 7-8,
    /// Slacker 1-2, New 0-1 (chỉ xem, không nộp).</summary>
    private static int ProgressCountV2(string persona, int index) => persona switch
    {
        V2Users.ShowcasePersona or V2Users.HardworkingPersona => 8,
        V2Users.AveragePersona => 7 + index % 2,
        V2Users.SlackerPersona => 1 + index % 2,
        _ => index % 2
    };

    /// <summary>Số bài nộp luyện thêm (điểm &lt; MaxScore, exercise khác bài pass) theo persona.</summary>
    private static int ExtraSubsV2(string persona, int index) => persona switch
    {
        V2Users.ShowcasePersona => 5,
        V2Users.HardworkingPersona => 1 + index % 2,
        V2Users.AveragePersona => index % 2 == 0 ? 1 : 0,
        V2Users.SlackerPersona => index % 3 == 0 ? 1 : (index % 2 == 0 ? 1 : 0),
        _ => 0
    };

    /// <summary>Điểm bài luyện thêm theo persona (luôn &lt; MaxScore để không trùng bài full-score — guard an toàn).</summary>
    private static int ExtraScoreV2(Exercise exercise, string persona, Random rng) => persona switch
    {
        V2Users.ShowcasePersona => Math.Max(0, exercise.MaxScore - 1 - rng.Next(0, Math.Max(1, exercise.MaxScore / 5))),
        V2Users.HardworkingPersona => Math.Max(0, exercise.MaxScore - 1 - rng.Next(0, Math.Max(1, exercise.MaxScore / 4))),
        V2Users.AveragePersona => Math.Max(0, exercise.MaxScore - 1 - rng.Next(0, Math.Max(1, exercise.MaxScore / 2))),
        _ => rng.Next(0, Math.Max(2, exercise.MaxScore / 2))
    };

    /// <summary>
    /// Sinh TOÀN BỘ kế hoạch activity V2 (submissions → lessons → nodes) từ V2Users.All + dữ liệu DB +
    /// Random cố định <see cref="PlanSeedV2"/>. 3 method gọi với cùng tham số → cùng kế hoạch →
    /// CompletedAt/PassedAt/SubmittedAt của bài full-score trùng chính xác trong 1 lần chạy; chạy lại
    /// lần 2 → guard chặn hết (0 thêm). Status=2 sinh song song với bài nộp full-score của exercise gắn
    /// node (node bài học → Quiz NodeId=node.Id; node kiểm tra cuối → FinalTestId) — đảm bảo cấu trúc
    /// "Status=2 ⇔ có submission full-score" không thể lệch.
    /// </summary>
    private static List<PlannedStudent> BuildPlanV2(
        IReadOnlyDictionary<string, User> usersByEmail,
        IReadOnlyList<Lesson> lessons,
        IReadOnlyList<Exercise> exercises,
        IReadOnlyList<LearningPathNode> nodes,
        DateTime now)
    {
        if (usersByEmail.Count == 0 || lessons.Count == 0 || exercises.Count == 0 || nodes.Count == 0)
        {
            return [];
        }

        var exercisesById = exercises.ToDictionary(e => e.Id);
        var byPath = nodes
            .GroupBy(n => n.PathId)
            .OrderBy(g => g.Key)
            .Select(g => g.OrderBy(n => n.SortOrder).ToList())
            .ToList();

        // Node có thể học/pass: các node playable (không phải folder) theo SortOrder của từng path.
        var exercisable = new List<LearningPathNode>();
        foreach (var pathNodes in byPath)
        {
            exercisable.AddRange(pathNodes.Where(n => n.ItemType != PathItemType.Folder).OrderBy(n => n.SortOrder));
        }

        // Exercise "pass" của node: kiểm tra cuối → FinalTestId; node bài học → Quiz (NodeId = node.Id).
        Exercise? PassExercise(LearningPathNode node)
        {
            if (node.FinalTestId is { } finalId && exercisesById.TryGetValue(finalId, out var final))
            {
                return final;
            }

            return node.LessonId is null
                ? null
                : exercises.FirstOrDefault(e => e.NodeId == node.Id && e.Type == ExerciseType.Mcq);
        }

        var plan = new List<PlannedStudent>(usersByEmail.Count);
        foreach (var seed in V2Users.All)
        {
            if (!usersByEmail.TryGetValue(seed.Email, out var user))
            {
                continue;
            }

            var rng = new Random(PlanSeedV2 + seed.Index * 37);

            var passNodes = exercisable.Take(PassCountV2(seed.Persona, seed.Index)).ToList();
            var passedIds = passNodes.Select(n => n.Id).ToHashSet();
            var activeNodes = nodes
                .Where(n => !passedIds.Contains(n.Id))
                .OrderBy(n => n.PathId)
                .ThenBy(n => n.SortOrder)
                .Take(ActiveCountV2(seed.Persona, seed.Index))
                .ToList();

            // Mốc thời gian: activity KHÔNG sớm hơn CreatedAt (deterministic — rải đều từ CreatedAt → now).
            var start = user.CreatedAt.AddHours(1);
            if (start > now)
            {
                start = now.AddHours(-1);
            }

            DateTime Spread(int i, int count)
            {
                var spanMinutes = Math.Max(60, (now - start).TotalMinutes);
                var at = start.AddMinutes(spanMinutes * (i + 1) / (count + 1) + rng.Next(-60, 61));
                return at < start ? start : at > now ? now : at;
            }

            var submissions = new List<PlannedSubmission>();

            // 1) Bài nộp full-score cho TỪNG node pass (rule Status=2 ⇔ Score == MaxScore)
            for (var j = 0; j < passNodes.Count; j++)
            {
                var exercise = PassExercise(passNodes[j]);
                if (exercise is null)
                {
                    continue;
                }

                submissions.Add(new PlannedSubmission(exercise, exercise.MaxScore, Spread(j, passNodes.Count)));
            }

            // 2) Bài nộp luyện thêm: exercise khác bài pass (quiz/lab/code của lesson đang học), điểm < MaxScore
            var extraCount = ExtraSubsV2(seed.Persona, seed.Index);
            if (extraCount > 0)
            {
                var passExerciseIds = submissions.Select(s => s.Exercise.Id).ToHashSet();
                var pool = exercises
                    .Where(e => !passExerciseIds.Contains(e.Id))
                    .OrderBy(e => e.Id)
                    .ToList();
                for (var k = 0; k < extraCount && pool.Count > 0; k++)
                {
                    var exercise = pool[(seed.Index * 5 + k * 7) % pool.Count];
                    submissions.Add(new PlannedSubmission(
                        exercise,
                        ExtraScoreV2(exercise, seed.Persona, rng),
                        Spread(passNodes.Count + k, passNodes.Count + extraCount)));
                }
            }

            // UserNodeProgress: pass → Status=2 (khớp bài full-score); còn lại → Status=1
            var nodeRows = new List<PlannedNodeRow>(passNodes.Count + activeNodes.Count);
            foreach (var node in passNodes)
            {
                var passEx = PassExercise(node);
                var pass = passEx != null
                    ? submissions.FirstOrDefault(s => s.Exercise.Id == passEx.Id && s.Score == s.Exercise.MaxScore)
                    : null;

                if (pass is null && passEx != null)
                {
                    continue;   // an toàn: node có exercise nhưng không có bài full → bỏ qua
                }

                var passedAt = pass?.SubmittedAt ?? ClampAfter(now.AddDays(-rng.Next(2, 5)), user.CreatedAt.AddHours(2));
                var unlockedAt = ClampAfter(passedAt.AddDays(-rng.Next(1, 4)), user.CreatedAt.AddHours(1));
                var score = pass?.Exercise.MaxScore ?? 100;
                nodeRows.Add(new PlannedNodeRow(node, 2, score, 3, unlockedAt, passedAt, passedAt));
            }

            foreach (var node in activeNodes)
            {
                DateTime unlockedAt;
                if (node.LessonId is { } lessonId)
                {
                    var latest = submissions
                        .Where(s => s.Exercise.LessonId == lessonId)
                        .Select(s => (DateTime?)s.SubmittedAt)
                        .Max();
                    unlockedAt = latest ?? ClampAfter(
                        now.AddDays(-rng.Next(1, 6)).AddHours(-rng.Next(0, 12)), user.CreatedAt.AddHours(1));
                }
                else
                {
                    unlockedAt = ClampAfter(
                        now.AddDays(-rng.Next(1, 6)).AddHours(-rng.Next(0, 12)), user.CreatedAt.AddHours(1));
                }

                nodeRows.Add(new PlannedNodeRow(node, 1, 0, 0, unlockedAt, null, unlockedAt));
            }

            // UserProgress: lesson có bài nộp trước, bổ sung lesson chỉ xem cho đủ số lượng persona
            var lessonRows = new List<PlannedLessonRow>();
            var includedLessonIds = new HashSet<int>();
            var studiedLessonIds = submissions
                .Select(s => s.Exercise.LessonId)
                .Where(id => id > 0)
                .Distinct()
                .OrderBy(id => id)
                .ToList();
            var targetCount = ProgressCountV2(seed.Persona, seed.Index);
            var lessonCount = Math.Min(lessons.Count, Math.Max(targetCount, studiedLessonIds.Count));
            foreach (var lessonId in studiedLessonIds)
            {

                var lesson = lessons.FirstOrDefault(l => l.Id == lessonId);
                if (lesson is null)
                {
                    continue;
                }

                var lessonSubs = submissions.Where(s => s.Exercise.LessonId == lessonId).ToList();
                int? bestScore = lessonSubs.Count == 0 ? null : lessonSubs.Max(s => s.Score);
                DateTime? completedAt = lessonSubs
                    .Where(s => s.Score == s.Exercise.MaxScore)
                    .Select(s => (DateTime?)s.SubmittedAt)
                    .OrderBy(x => x)
                    .FirstOrDefault();
                var updatedAt = lessonSubs.Max(s => s.SubmittedAt);
                lessonRows.Add(new PlannedLessonRow(lesson, rng.Next(1, 7), bestScore, completedAt, updatedAt));
                includedLessonIds.Add(lessonId);
            }

            var remainingLessons = lessons
                .Where(l => !includedLessonIds.Contains(l.Id))
                .OrderBy(l => l.Id)
                .ToList();
            while (lessonRows.Count < lessonCount && remainingLessons.Count > 0)
            {
                var pickIdx = seed.Index % remainingLessons.Count;
                var lesson = remainingLessons[pickIdx];
                remainingLessons.RemoveAt(pickIdx);
                lessonRows.Add(new PlannedLessonRow(
                    lesson,
                    rng.Next(1, 7),
                    null,
                    null,
                    ClampAfter(now.AddDays(-rng.Next(1, 4)).AddHours(-rng.Next(0, 12)), user.CreatedAt.AddHours(1))));
                includedLessonIds.Add(lesson.Id);
            }

            plan.Add(new PlannedStudent(user, submissions, lessonRows, nodeRows));
        }

        return plan;
    }

    // ── Helpers chung ─────────────────────────────────────────

    /// <summary>69 user V2 theo V2Users.All (dict theo Email) + lessons/exercises/nodes active từ DB.</summary>
    private static async Task<(IReadOnlyDictionary<string, User> Users, IReadOnlyList<Lesson> Lessons, IReadOnlyList<Exercise> Exercises, IReadOnlyList<LearningPathNode> Nodes)>
        LoadV2ContextAsync(AppDbContext db, CancellationToken ct)
    {
        var emails = V2Users.All.Select(u => u.Email).ToList();
        var users = await db.Users.AsNoTracking()
            .Where(u => u.Role == UserRole.Student && u.DeletedAt == null && emails.Contains(u.Email))
            .ToListAsync(ct);
        var lessons = await db.Lessons.AsNoTracking()
            .Where(l => l.DeletedAt == null && l.Status == LessonStatus.Active)
            .OrderBy(l => l.Id)
            .ToListAsync(ct);
        var exercises = await db.Exercises.AsNoTracking()
            .Where(e => e.DeletedAt == null && e.Status == ExerciseStatus.Active)
            .OrderBy(e => e.Id)
            .ToListAsync(ct);
        var activePathIds = await db.LearningPaths.AsNoTracking()
            .Where(p => p.IsActive && p.Status == LearningPathStatus.Active)
            .Select(p => p.Id).ToListAsync(ct);
        var nodes = await db.LearningPathNodes.AsNoTracking()
            .Where(n => activePathIds.Contains(n.PathId))
            .OrderBy(n => n.PathId)
            .ThenBy(n => n.SortOrder)
            .ToListAsync(ct);
        return (users.ToDictionary(u => u.Email, StringComparer.OrdinalIgnoreCase), lessons, exercises, nodes);
    }
}
