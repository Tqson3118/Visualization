using System.Text.Json;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Persistence.Seed;

/// <summary>
/// SEED-2b — nhóm progress/submissions của <see cref="SeedDemoActivity"/> (SDD §7.5):
/// UserProgress (SDD §7.3.4), UserNodeProgress (SDD §7.3.26) và ExerciseSubmissions (SDD §7.3.10)
/// cho 9 student demo (8 student SeedData.Students + student@demo.local). KHÔNG đụng user rác smoke,
/// KHÔNG tạo user mới, ClassAssignmentId luôn null (task SEED-3 gắn sau).
///
/// Idempotent theo pattern SeedRunner: guard AnyAsync → Add → SaveChanges → log. Kế hoạch dữ liệu
/// sinh DETERMINISTIC (Random cố định <see cref="PlanSeed"/> + dữ liệu DB ổn định) → 3 method dùng
/// chung 1 kế hoạch nên UserProgress.CompletedAt / UserNodeProgress.PassedAt khớp CHÍNH XÁC với
/// ExerciseSubmission.SubmittedAt của bài nộp full-score tương ứng; chạy lại lần 2 → 0 dòng thêm.
///
/// Rule ràng buộc: UserNodeProgress.Status=2 BẮT BUỘC có ExerciseSubmission Score == MaxScore của
/// exercise gắn node — node bài học ("Học: X") gắn exercise qua NodeId (Quiz Stage=1, H-FINAL1) →
/// bài nộp Quiz full-score của lesson; node kiểm tra cuối gắn exercise qua NodeId/FinalTestId →
/// bài nộp full-score. Node luyện tập tổng hợp (không có exercise) chỉ đạt Status=1.
/// Stars = ceil(score×3/maxScore) clamp 0..3 (full-score → 3).
/// </summary>
public static partial class SeedDemoActivity
{
    /// <summary>Seed cố định để kế hoạch deterministic — mọi method phải sinh y hệt.</summary>
    private const int PlanSeed = 20260813;

    // ── 5. UserProgress (SDD §7.3.4) ──────────────────────────

    private static partial Task SeedUserProgressAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
        => SeedUserProgressCoreAsync(db, clock, logger, ct);

    /// <summary>~31 dòng: mỗi student 2-8 lesson khác nhau; Viewed=1, SimulationCount ≥ 1,
    /// BestScore = max Score bài nộp trong lesson (null nếu chưa nộp), CompletedAt = thời điểm bài
    /// nộp full-score ĐẦU TIÊN của lesson (null nếu không có) — khớp SubmittedAt bài nộp tương ứng.</summary>
    private static async Task SeedUserProgressCoreAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        var now = NowUtc7(clock);
        var students = await LoadDemoStudentsAsync(db, ct);
        var lessons = await db.Lessons.AsNoTracking()
            .Where(l => l.DeletedAt == null && l.Status == LessonStatus.Active)
            .OrderBy(l => l.Id).ToListAsync(ct);
        var exercises = await db.Exercises.AsNoTracking()
            .Where(e => e.DeletedAt == null && e.Status == ExerciseStatus.Active)
            .OrderBy(e => e.Id).ToListAsync(ct);
        var nodes = await db.LearningPathNodes.AsNoTracking()
            .OrderBy(n => n.PathId).ThenBy(n => n.SortOrder).ToListAsync(ct);

        var plan = BuildPlan(students, lessons, exercises, nodes, now);

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
        logger.LogInformation("Seed: UserProgress thêm {Added} / bỏ qua {Skipped} (đã tồn tại)", added, skipped);
    }

    // ── 6. UserNodeProgress (SDD §7.3.26) ─────────────────────

    private static partial Task SeedUserNodeProgressAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
        => SeedUserNodeProgressCoreAsync(db, clock, logger, ct);

    /// <summary>~33 dòng: student chăm (3 đầu danh sách) pass lộ trình 1-2 (node bài học + kiểm tra
    /// cuối Status=2, node luyện tập Status=1); student mới 1-3 node Status=1 hoặc 2. Status=2 BẮT
    /// BUỘC có bài nộp full-score của exercise gắn node (PassedAt = SubmittedAt bài đó; NodeScore =
    /// MaxScore; Stars = 3); không tìm thấy bài full → hạ xuống Status=1 cho an toàn.</summary>
    private static async Task SeedUserNodeProgressCoreAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        var now = NowUtc7(clock);
        var students = await LoadDemoStudentsAsync(db, ct);
        var lessons = await db.Lessons.AsNoTracking()
            .Where(l => l.DeletedAt == null && l.Status == LessonStatus.Active)
            .OrderBy(l => l.Id).ToListAsync(ct);
        var exercises = await db.Exercises.AsNoTracking()
            .Where(e => e.DeletedAt == null && e.Status == ExerciseStatus.Active)
            .OrderBy(e => e.Id).ToListAsync(ct);
        var nodes = await db.LearningPathNodes.AsNoTracking()
            .OrderBy(n => n.PathId).ThenBy(n => n.SortOrder).ToListAsync(ct);

        var plan = BuildPlan(students, lessons, exercises, nodes, now);

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
        logger.LogInformation("Seed: UserNodeProgress thêm {Added} / bỏ qua {Skipped} (đã tồn tại)", added, skipped);
    }

    // ── 7. ExerciseSubmissions (SDD §7.3.10) ──────────────────

    private static partial Task SeedExerciseSubmissionsAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
        => SeedExerciseSubmissionsCoreAsync(db, clock, logger, ct);

    /// <summary>50 bài nộp: mỗi student 3-10 bài; MCQ (quiz + final test) / LAB / CODE đều có;
    /// Score đa dạng (0 / thấp / full = MaxScore); SubmittedAt rải 1-27 ngày, nhiều dòng gần đây;
    /// ClassAssignmentId = null. Không có UNIQUE → guard (user, exercise, score): bài full-score chỉ
    /// thêm khi user chưa có submission full-score cho exercise đó.</summary>
    private static async Task SeedExerciseSubmissionsCoreAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        var now = NowUtc7(clock);
        var students = await LoadDemoStudentsAsync(db, ct);
        var lessons = await db.Lessons.AsNoTracking()
            .Where(l => l.DeletedAt == null && l.Status == LessonStatus.Active)
            .OrderBy(l => l.Id).ToListAsync(ct);
        var exercises = await db.Exercises.AsNoTracking()
            .Where(e => e.DeletedAt == null && e.Status == ExerciseStatus.Active)
            .OrderBy(e => e.Id).ToListAsync(ct);
        var nodes = await db.LearningPathNodes.AsNoTracking()
            .OrderBy(n => n.PathId).ThenBy(n => n.SortOrder).ToListAsync(ct);

        var plan = BuildPlan(students, lessons, exercises, nodes, now);

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

                var rng = new Random(PlanSeed + sub.Exercise.Id * 31 + sub.Score * 7);
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
        logger.LogInformation("Seed: ExerciseSubmissions thêm {Added} / bỏ qua {Skipped} (đã tồn tại)", added, skipped);
    }

    // ── Kế hoạch dữ liệu deterministic (dùng chung 3 method) ──

    private sealed record PlannedStudent(
        User User,
        List<PlannedSubmission> Submissions,
        List<PlannedLessonRow> Lessons,
        List<PlannedNodeRow> Nodes);

    private sealed record PlannedSubmission(Exercise Exercise, int Score, DateTime SubmittedAt);

    private sealed record PlannedLessonRow(
        Lesson Lesson,
        int SimulationCount,
        int? BestScore,
        DateTime? CompletedAt,
        DateTime UpdatedAt);

    private sealed record PlannedNodeRow(
        LearningPathNode Node,
        int Status,
        int NodeScore,
        int Stars,
        DateTime? UnlockedAt,
        DateTime? PassedAt,
        DateTime UpdatedAt);

    private enum NodeKind
    {
        Lesson0 = 0,
        Lesson1 = 1,
        Practice = 2,
        Final = 3
    }

    /// <summary>
    /// Sinh TOÀN BỘ kế hoạch activity demo (submissions → lessons → nodes) từ dữ liệu DB + Random cố
    /// định seed. 3 method gọi hàm này với cùng tham số → cùng kế hoạch → CompletedAt/PassedAt/
    /// SubmittedAt của bài full-score trùng chính xác trong 1 lần chạy; chạy lại lần 2 → guard chặn
    /// hết (0 thêm).
    /// </summary>
    private static List<PlannedStudent> BuildPlan(
        IReadOnlyList<User> students,
        IReadOnlyList<Lesson> lessons,
        IReadOnlyList<Exercise> exercises,
        IReadOnlyList<LearningPathNode> nodes,
        DateTime now)
    {
        if (students.Count == 0 || lessons.Count == 0 || exercises.Count == 0 || nodes.Count == 0)
        {
            return [];
        }

        var rng = new Random(PlanSeed);
        var lessonsByTitle = lessons.GroupBy(l => l.Title).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
        var byLesson = exercises.GroupBy(e => e.LessonId).ToDictionary(g => g.Key, g => g.ToList());

        // H-FINAL1: exercise lesson có NodeId != null (Quiz/Lab/Code gắn node "Học: X"); phân biệt
        // quiz lesson bằng title chính xác (final test cùng LessonId, title "Kiểm tra cuối: ...").
        Exercise Quiz(string title) => byLesson[lessonsByTitle[title].Id].First(e => e.Type == ExerciseType.Mcq && e.Title == $"Quiz: {title}");
        Exercise Lab(string title) => byLesson[lessonsByTitle[title].Id].First(e => e.Type == ExerciseType.SimulationLab);
        Exercise Code(string title) => byLesson[lessonsByTitle[title].Id].First(e => e.Type == ExerciseType.Code);
        var finals = exercises
            .Where(e => e.NodeId != null && e.Title.StartsWith("Kiểm tra cuối: ", StringComparison.Ordinal))
            .OrderBy(e => e.Id).ToList();

        DateTime At(Random r, int minDaysAgo, int maxDaysAgo) =>
            now.AddDays(-r.Next(minDaysAgo, maxDaysAgo + 1)).AddHours(-r.Next(0, 24)).AddMinutes(-r.Next(0, 60));

        var plan = new List<PlannedStudent>(students.Count);
        for (var i = 0; i < students.Count; i++)
        {
            var user = students[i];
            var submissions = BuildStudentSubmissions(i, Quiz, Lab, Code, idx => finals[idx], At, rng);
            var lessonsRows = BuildStudentLessons(i, lessonsByTitle, submissions, rng, now);
            var nodeRows = BuildStudentNodes(i, nodes, finals, submissions, user, rng, now);
            plan.Add(new PlannedStudent(user, submissions, lessonsRows, nodeRows));
        }

        return plan;
    }

    /// <summary>50 bài nộp: mỗi student 3-10 bài; MCQ (quiz + final test) / LAB / CODE đều có;
    /// Score đa dạng (0 / thấp / full = MaxScore); SubmittedAt rải 1-27 ngày, nhiều dòng gần đây.
    /// Full-score quiz của lesson = mốc pass node bài học; full-score final test = mốc pass node
    /// kiểm tra cuối.</summary>
    private static List<PlannedSubmission> BuildStudentSubmissions(
        int idx,
        Func<string, Exercise> quiz,
        Func<string, Exercise> lab,
        Func<string, Exercise> code,
        Func<int, Exercise> final,
        Func<Random, int, int, DateTime> at,
        Random rng)
    {
        var result = new List<PlannedSubmission>();
        void Add(Exercise exercise, int score, int minDaysAgo, int maxDaysAgo) =>
            result.Add(new PlannedSubmission(exercise, Math.Clamp(score, 0, Math.Max(0, exercise.MaxScore)), at(rng, minDaysAgo, maxDaysAgo)));

        switch (idx)
        {
            case 0: // Nguyễn Minh Anh — chăm nhất: pass lộ trình 1 + 2 (10 bài)
                Add(quiz("Bubble Sort"), quiz("Bubble Sort").MaxScore, 24, 27);
                Add(lab("Bubble Sort"), 8, 23, 26);
                Add(code("Bubble Sort"), code("Bubble Sort").MaxScore, 21, 24);
                Add(quiz("Binary Search"), quiz("Binary Search").MaxScore, 17, 20);
                Add(lab("Binary Search"), 7, 15, 18);
                Add(final(0), final(0).MaxScore, 13, 16);
                Add(quiz("Stack"), quiz("Stack").MaxScore, 11, 14);
                Add(quiz("Linked List"), quiz("Linked List").MaxScore, 8, 11);
                Add(final(1), final(1).MaxScore, 5, 8);
                Add(lab("Hash Table"), 5, 2, 4);
                break;

            case 1: // Trần Quốc Bảo — chăm: pass lộ trình 1 + 2 (8 bài)
                Add(quiz("Bubble Sort"), quiz("Bubble Sort").MaxScore, 23, 26);
                Add(lab("Bubble Sort"), 6, 21, 24);
                Add(code("Bubble Sort"), 60, 19, 22);
                Add(quiz("Binary Search"), quiz("Binary Search").MaxScore, 16, 19);
                Add(final(0), final(0).MaxScore, 12, 15);
                Add(quiz("Stack"), quiz("Stack").MaxScore, 9, 12);
                Add(quiz("Linked List"), quiz("Linked List").MaxScore, 6, 9);
                Add(final(1), final(1).MaxScore, 3, 6);
                break;

            case 2: // Lê Thị Kim Ngân — chăm: pass lộ trình 1 + 2 (7 bài)
                Add(quiz("Bubble Sort"), quiz("Bubble Sort").MaxScore, 22, 25);
                Add(quiz("Binary Search"), quiz("Binary Search").MaxScore, 15, 18);
                Add(final(0), final(0).MaxScore, 11, 14);
                Add(quiz("Stack"), quiz("Stack").MaxScore, 8, 11);
                Add(quiz("Linked List"), quiz("Linked List").MaxScore, 5, 8);
                Add(code("Linked List"), 80, 3, 6);
                Add(final(1), final(1).MaxScore, 1, 4);
                break;

            case 3: // Phạm Hoàng Long — mới: pass node đầu P1, đang học node 2 (5 bài)
                Add(quiz("Bubble Sort"), 3, 13, 16);
                Add(quiz("Bubble Sort"), quiz("Bubble Sort").MaxScore, 10, 13);
                Add(lab("Bubble Sort"), 4, 8, 11);
                Add(quiz("Binary Search"), 3, 4, 7);
                Add(code("Bubble Sort"), 0, 2, 5);
                break;

            case 4: // Vũ Thanh Tùng — mới: đang học node đầu P1 (5 bài)
                Add(quiz("Bubble Sort"), 4, 9, 12);
                Add(lab("Bubble Sort"), 7, 6, 9);
                Add(quiz("Binary Search"), 2, 4, 7);
                Add(quiz("Stack"), 1, 2, 5);
                Add(code("Bubble Sort"), 40, 1, 3);
                break;

            case 5: // Nguyễn Trang — mới: đang học P3 (4 bài)
                Add(quiz("BST"), 4, 10, 13);
                Add(lab("BST"), 6, 6, 9);
                Add(quiz("AVL"), 0, 4, 7);
                Add(quiz("Hash Table"), 3, 2, 5);
                break;

            case 6: // Đoàn Minh Đức — mới: đang học P4 (3 bài)
                Add(quiz("Hash Table"), 5, 8, 11);
                Add(lab("Hash Table"), 3, 5, 8);
                Add(code("Hash Table"), 0, 2, 5);
                break;

            case 7: // Huỳnh Thúy — mới: đang học P5 (3 bài)
                Add(quiz("BFS"), 6, 7, 10);
                Add(lab("BFS"), 4, 4, 7);
                Add(code("BFS"), 0, 1, 4);
                break;

            case 8: // student@demo.local — vừa học vừa chơi: pass node đầu P1 (5 bài)
                Add(quiz("Bubble Sort"), 3, 13, 16);
                Add(quiz("Bubble Sort"), quiz("Bubble Sort").MaxScore, 9, 12);
                Add(lab("Bubble Sort"), lab("Bubble Sort").MaxScore, 7, 10);
                Add(quiz("Binary Search"), 4, 4, 7);
                Add(quiz("Stack"), 2, 2, 4);
                break;
        }

        return result;
    }

    /// <summary>31 dòng UserProgress: mỗi student 2-8 lesson khác nhau, Viewed=1, SimulationCount ≥ 1,
    /// BestScore = max Score bài nộp trong lesson (null nếu chưa nộp), CompletedAt = thời điểm bài nộp
    /// full-score ĐẦU TIÊN của lesson (null nếu không có).</summary>
    private static List<PlannedLessonRow> BuildStudentLessons(
        int idx,
        IReadOnlyDictionary<string, Lesson> lessonsByTitle,
        IReadOnlyList<PlannedSubmission> submissions,
        Random rng,
        DateTime now)
    {
        string[] titles = idx switch
        {
            0 => ["Bubble Sort", "Binary Search", "Stack", "Linked List", "Hash Table", "AVL", "BFS"],
            1 => ["Bubble Sort", "Binary Search", "Stack", "Linked List", "BST"],
            2 => ["Bubble Sort", "Binary Search", "Stack", "Linked List"],
            3 => ["Bubble Sort", "Binary Search"],
            4 => ["Bubble Sort", "Binary Search", "Stack"],
            5 => ["BST", "AVL", "Hash Table"],
            6 => ["Hash Table", "Bubble Sort"],
            7 => ["BFS", "Linked List"],
            8 => ["Bubble Sort", "Binary Search", "Stack"],
            _ => []
        };

        var result = new List<PlannedLessonRow>(titles.Length);
        foreach (var title in titles)
        {
            var lesson = lessonsByTitle[title];
            var lessonSubs = submissions.Where(s => s.Exercise.LessonId == lesson.Id).ToList();

            int? bestScore = lessonSubs.Count == 0 ? null : lessonSubs.Max(s => s.Score);
            DateTime? completedAt = lessonSubs
                .Where(s => s.Score == s.Exercise.MaxScore)
                .Select(s => (DateTime?)s.SubmittedAt)
                .OrderBy(x => x)
                .FirstOrDefault();
            var updatedAt = lessonSubs.Count == 0
                ? now.AddDays(-rng.Next(1, 4)).AddHours(-rng.Next(0, 12))
                : lessonSubs.Max(s => s.SubmittedAt);

            result.Add(new PlannedLessonRow(lesson, rng.Next(1, 7), bestScore, completedAt, updatedAt));
        }

        return result;
    }

    /// <summary>33 dòng UserNodeProgress: student chăm (3 đầu danh sách) pass lộ trình 1-2 (node bài
    /// học + kiểm tra cuối Status=2, node luyện tập Status=1); student mới 1-3 node Status=1 hoặc 2.
    /// Status=2 BẮT BUỘC có bài nộp full-score của exercise gắn node (PassedAt = SubmittedAt bài đó;
    /// NodeScore = MaxScore; Stars = ceil(score×3/maxScore) → 3); không tìm thấy bài full → hạ xuống
    /// Status=1 cho an toàn.</summary>
    private static List<PlannedNodeRow> BuildStudentNodes(
        int idx,
        IReadOnlyList<LearningPathNode> nodes,
        IReadOnlyList<Exercise> finals,
        IReadOnlyList<PlannedSubmission> submissions,
        User user,
        Random rng,
        DateTime now)
    {
        var byPath = nodes
            .GroupBy(n => n.PathId)
            .OrderBy(g => g.Key)
            .Select(g => g.OrderBy(n => n.SortOrder).ToList())
            .ToList();

        (int Path, NodeKind Kind, int Status)[] specs = idx switch
        {
            0 or 1 or 2 => [
                (0, NodeKind.Lesson0, 2), (0, NodeKind.Lesson1, 2), (0, NodeKind.Practice, 1), (0, NodeKind.Final, 2),
                (1, NodeKind.Lesson0, 2), (1, NodeKind.Lesson1, 2), (1, NodeKind.Practice, 1), (1, NodeKind.Final, 2),
            ],
            3 => [(0, NodeKind.Lesson0, 2), (0, NodeKind.Lesson1, 1)],
            4 => [(0, NodeKind.Lesson0, 1)],
            5 => [(2, NodeKind.Lesson0, 1)],
            6 => [(3, NodeKind.Lesson0, 1)],
            7 => [(4, NodeKind.Lesson0, 1)],
            8 => [(0, NodeKind.Lesson0, 2), (0, NodeKind.Lesson1, 1), (1, NodeKind.Lesson0, 1)],
            _ => []
        };

        var result = new List<PlannedNodeRow>(specs.Length);
        foreach (var (pathIndex, kind, status) in specs)
        {
            if (pathIndex < 0 || pathIndex >= byPath.Count) continue;
            var pathNodes = byPath[pathIndex];
            var lessonNodes = pathNodes.Where(n => n.LessonId != null).ToList();
            var finalNode = pathNodes.FirstOrDefault(n => n.FinalTestId != null);
            if (lessonNodes.Count == 0) continue;

            var selectedNode = kind switch
            {
                NodeKind.Lesson0 => lessonNodes[0],
                NodeKind.Lesson1 => lessonNodes.Count > 1 ? lessonNodes[1] : lessonNodes[0],
                NodeKind.Practice => pathNodes.FirstOrDefault(n => n.LessonId == null && n.FinalTestId == null),
                _ => finalNode
            };
            if (selectedNode is null) continue;
            LearningPathNode node = selectedNode;
            if (kind == NodeKind.Final && finalNode is null) continue;

            // Bài nộp full-score của exercise gắn node (rule: Status=2 ⇐ Score == MaxScore)
            PlannedSubmission? pass = kind switch
            {
                NodeKind.Lesson0 or NodeKind.Lesson1 when node.LessonId is { } lessonId =>
                    submissions.FirstOrDefault(s =>
                        s.Exercise.NodeId == node.Id &&        // H-FINAL1: quiz lesson gắn NodeId node bài học
                        s.Exercise.Type == ExerciseType.Mcq &&
                        s.Score == s.Exercise.MaxScore),
                NodeKind.Final => submissions.FirstOrDefault(s =>
                    s.Exercise.Id == finalNode!.FinalTestId && s.Score == s.Exercise.MaxScore),
                _ => null
            };

            if (status == 2 && (pass is not null || kind == NodeKind.Practice))
            {
                var passedAt = pass?.SubmittedAt ?? ClampAfter(now.AddDays(-rng.Next(2, 5)), user.CreatedAt.AddHours(2));
                var unlockedAt = ClampAfter(passedAt.AddDays(-rng.Next(1, 4)), user.CreatedAt.AddHours(1));
                var score = pass?.Exercise.MaxScore ?? 100;
                result.Add(new PlannedNodeRow(node, 2, score, 3, unlockedAt, passedAt, passedAt));
            }
            else
            {
                var unlockedAt = UnlockTimeForActiveNode(kind, node, submissions, finalNode!, rng, now);
                result.Add(new PlannedNodeRow(node, 1, 0, 0, unlockedAt, null, unlockedAt));
            }
        }

        return result;
    }

    /// <summary>Thời điểm mở khóa node Status=1: bài nộp mới nhất trong lesson của node (nếu có);
    /// node luyện tập → vài ngày trước bài nộp full final test; còn lại → vài ngày gần đây.</summary>
    private static DateTime UnlockTimeForActiveNode(
        NodeKind kind,
        LearningPathNode node,
        IReadOnlyList<PlannedSubmission> submissions,
        LearningPathNode finalNode,
        Random rng,
        DateTime now)
    {
        if (kind is NodeKind.Lesson0 or NodeKind.Lesson1 && node.LessonId is { } lessonId)
        {
            var latest = submissions
                .Where(s => s.Exercise.LessonId == lessonId)
                .Select(s => (DateTime?)s.SubmittedAt)
                .Max();
            if (latest is not null)
            {
                return latest.Value;
            }
        }

        if (kind == NodeKind.Practice && finalNode.FinalTestId is { } finalId)
        {
            var finalPass = submissions.FirstOrDefault(s => s.Exercise.Id == finalId && s.Score == s.Exercise.MaxScore);
            if (finalPass is not null)
            {
                return finalPass.SubmittedAt.AddDays(-rng.Next(2, 5));
            }
        }

        return now.AddDays(-rng.Next(1, 6)).AddHours(-rng.Next(0, 12));
    }

    private static DateTime ClampAfter(DateTime value, DateTime min) => value < min ? min : value;

    // ── JSON bài nộp (đúng format ExerciseService: AnswerDto / QuestionResultDto / CodeSubmitRequest) ──

    private static (string AnswersJson, string ResultJson) BuildSubmissionJson(
        Exercise exercise,
        int score,
        IReadOnlyDictionary<int, List<Question>> questionsByExercise,
        Random rng)
    {
        var answers = new List<AnswerDto>();
        var results = new List<QuestionResultDto>();

        switch (exercise.Type)
        {
            case ExerciseType.Mcq:
            {
                var questions = questionsByExercise.GetValueOrDefault(exercise.Id) ?? [];
                var remaining = score;
                foreach (var q in questions)
                {
                    var correct = remaining >= q.Points;
                    if (correct)
                    {
                        remaining -= q.Points;
                    }

                    var correctIndices = ParseAnswerIndices(q.AnswerJson);
                    var selected = correctIndices;
                    if (!correct)
                    {
                        var options = ParseOptions(q.OptionsJson);
                        var wrong = correctIndices.Count > 0 ? correctIndices[0] : 0;
                        if (options.Count > 1)
                        {
                            var pick = rng.Next(0, options.Count);
                            while (pick == wrong)
                            {
                                pick = (pick + 1) % options.Count;
                            }

                            selected = [pick];
                        }
                        else
                        {
                            selected = [];
                        }
                    }

                    answers.Add(new AnswerDto { QuestionId = q.Id, Selected = selected });
                    results.Add(new QuestionResultDto
                    {
                        QuestionId = q.Id,
                        Correct = correct,
                        CorrectAnswer = ParseElement(q.AnswerJson),
                        Explanation = q.Explanation
                    });
                }

                break;
            }

            case ExerciseType.SimulationLab:
            {
                var maxSteps = ParseMaxSteps(exercise.ConfigJson);
                var full = exercise.MaxScore > 0 && score >= exercise.MaxScore;
                var steps = full ? rng.Next(1, maxSteps + 1) : maxSteps + rng.Next(1, 4);
                var state = JsonDocument.Parse("[1,2,3]").RootElement.Clone();
                answers.Add(new AnswerDto
                {
                    QuestionId = 0,
                    Selected = [],
                    LabAnswer = new LabAnswerDto { FinalState = state, StepsUsed = steps, MaxSteps = maxSteps }
                });
                results.Add(new QuestionResultDto
                {
                    QuestionId = 0,
                    Correct = full,
                    CorrectAnswer = state,
                    Explanation = full
                        ? $"Trạng thái cuối khớp chuẩn; {steps} bước ≤ giới hạn {(int)Math.Ceiling(maxSteps * 1.5)} (chuẩn {maxSteps} × 1.5)"
                        : "Trạng thái cuối không khớp chuẩn StepExecutor"
                });
                break;
            }

            case ExerciseType.Code:
            {
                var total = ParseTestCount(exercise.ConfigJson);
                var passed = exercise.MaxScore > 0 ? (int)Math.Round(total * score / (double)exercise.MaxScore) : 0;
                var codeResults = Enumerable.Range(0, total).Select(i => new CodeTestCaseResultDto
                {
                    TestId = $"t{i + 1}",
                    Passed = i < passed,
                    Input = null,
                    Expected = null,
                    Output = i < passed ? "OK" : null
                }).ToList();

                var answersJson = JsonSerializer.Serialize(new
                {
                    Code = "function solve() {\n  // bài nộp demo (seed SEED-2b)\n}",
                    ExerciseId = exercise.Id,
                    Score = score,
                    Passed = passed,
                    Total = total,
                    Results = codeResults
                });
                return (answersJson, JsonSerializer.Serialize(codeResults));
            }

            default:
                break;
        }

        return (JsonSerializer.Serialize(answers), JsonSerializer.Serialize(results));
    }

    private static List<int> ParseAnswerIndices(string answerJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(answerJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            return doc.RootElement.EnumerateArray()
                .Where(e => e.TryGetInt32(out _))
                .Select(e => e.GetInt32())
                .ToList();
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static List<string> ParseOptions(string optionsJson)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(optionsJson) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static JsonElement ParseElement(string json)
    {
        try
        {
            return JsonDocument.Parse(json).RootElement.Clone();
        }
        catch (JsonException)
        {
            return JsonSerializer.SerializeToElement(new List<int>());
        }
    }

    private static int ParseMaxSteps(string? configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
        {
            return 6;
        }

        try
        {
            using var doc = JsonDocument.Parse(configJson);
            return doc.RootElement.TryGetProperty("maxSteps", out var max) && max.TryGetInt32(out var value) && value > 0
                ? value
                : 6;
        }
        catch (JsonException)
        {
            return 6;
        }
    }

    private static int ParseTestCount(string? configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
        {
            return 11;
        }

        try
        {
            using var doc = JsonDocument.Parse(configJson);
            // ConfigJson code có thể là object {signature, testCases:[...]} HOẶC array tasks (mỗi task có testCases)
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                var total = 0;
                foreach (var task in doc.RootElement.EnumerateArray())
                {
                    if (task.TryGetProperty("testCases", out var tests) && tests.ValueKind == JsonValueKind.Array)
                    {
                        total += tests.GetArrayLength();
                    }
                }

                return total > 0 ? total : 11;
            }

            return doc.RootElement.TryGetProperty("testCases", out var testCases) && testCases.ValueKind == JsonValueKind.Array
                ? testCases.GetArrayLength()
                : 11;
        }
        catch (JsonException)
        {
            return 11;
        }
    }

    // ── Helpers chung ─────────────────────────────────────────

    /// <summary>Hôm nay theo UTC+7 (quy ước SeedDemoActivity: clock.UtcNow + 7h), cắt còn phút để
    /// 3 method tính cùng mốc thời gian trong 1 lần chạy.</summary>
    private static DateTime NowUtc7(IDateTimeProvider clock)
    {
        var now = clock.UtcNow.AddHours(7);
        return now.AddTicks(-(now.Ticks % TimeSpan.TicksPerMinute));
    }

    /// <summary>9 student demo theo thứ tự SeedData.Students + student@demo.local (student chăm ở đầu
    /// danh sách). KHÔNG tạo user mới, KHÔNG đụng user rác smoke.</summary>
    private static async Task<List<User>> LoadDemoStudentsAsync(AppDbContext db, CancellationToken ct)
    {
        var emails = SeedData.Students.Select(s => s.Email).Append("student@demo.local").ToList();
        var users = await db.Users.AsNoTracking()
            .Where(u => u.Role == UserRole.Student && u.DeletedAt == null && emails.Contains(u.Email))
            .ToListAsync(ct);
        return emails
            .Select(e => users.FirstOrDefault(u => u.Email == e))
            .Where(u => u is not null)
            .Cast<User>()
            .ToList();
    }
}
