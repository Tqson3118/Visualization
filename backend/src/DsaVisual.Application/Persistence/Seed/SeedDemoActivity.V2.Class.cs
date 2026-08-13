using DsaVisual.Application.Common;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Persistence.Seed;

/// <summary>
/// SEED-V2 (PROMPT_K_SEED_PROD_V2) — Task 5: mở rộng Module H (SDD §7.3.16/17/18) cho 69 user V2.
/// Pattern y hệt V1 SeedDemoActivity.Class.cs (BuildClassPlan → guard → Add → SaveChanges → log,
/// AttachClassSubmissionsAsync tái dùng/tạo submission) nhưng kế hoạch RIÊNG (BuildClassPlanV2) và
/// KHÔNG đụng 2 lớp V1 (DSA213/ADVNCE — chỉ THÊM member/assignment mới) + KHÔNG đụng lớp rác QA (2TH0YJ).
///
/// Kế hoạch (tổng V2 — khớp khoảng mục tiêu Task 5):
///   ClassMembers V2 53 = AI1702 24 (HW 5 / AVG 12 / SLK 5 / NEW 2) + SD21361 16 (SLK 13 + HW 3)
///     + SD21361NC 13 (HW 5 + AVG 8) → tổng ClassMembers 13 cũ + 53 = 66 ≥ 60.
///   ClassAssignments V2 13 = AI1702 6 (3 exercise: Quiz/Lab/Code BFS + 3 lesson: BFS, Hash Table, BST)
///     + SD21361 4 (Lab/Code Binary Search, Quiz Stack, Lesson Linked List) + SD21361NC 3
///     (Quiz BST, Lab Stack, Lesson AVL) → tổng ClassAssignments 7 cũ + 13 = 20 ≥ 20.
///   Lớp MỚI "AI1702 — Thuật toán Đồ thị" (InviteCode GRPH21 — không trùng DSA213/ADVNCE/2TH0YJ),
///     Status = 1 (Đóng), OwnerId = teacher@demo.local, CreatedAt 15 ngày trước.
///
/// Mỗi lớp đảm bảo báo cáo (ClassService.GetReportAsync, FR-8.3/8.4) có đủ 3 nhóm: mỗi bài gán
/// exercise có ≥ 1 nộp đúng hạn (SubmittedAt ≤ DueAt) + ≥ 1 nộp trễ (SubmittedAt &gt; DueAt — chỉ
/// khả thi với bài gán quá hạn) + ≥ 1 chưa nộp (member thiếu bài gán); LaggingLearners ≥ 1 (member
/// thiếu ≥ 2 bài gán — NEW/Slacker của AI1702, Slacker của SD21361, AVG cuối của SD21361NC).
/// Nộp bài: ưu tiên tái dùng ExerciseSubmission V2 (Task 3, ClassAssignmentId = null) khớp thời điểm
/// (Late ? SubmittedAt &gt; DueAt : SubmittedAt ≤ DueAt); không có → tự tạo mới (Score hợp lệ,
/// AnswersJson/ResultJson "[]", SubmittedAt = DueAt + offset clamp [CreatedAt+1h, now-1h]).
/// Guard (UserId, ClassAssignmentId) chống gắn 2 lần; chạy lại lần 2 → 0 thêm.
/// KHÔNG sửa file V1/SeedData/entity, KHÔNG wire vào SeedDemoActivity.cs (Task 6 sẽ thêm call).
/// </summary>
public static partial class SeedDemoActivity
{
    /// <summary>
    /// Bước lớp V2: lớp mới AI1702 + thêm member/assignment cho SD21361 (mở) và SD21361NC (đóng).
    /// User dùng chung <see cref="V2Users.All"/> (guard email — user chưa seed thì bỏ qua an toàn).
    /// </summary>
    public static async Task SeedClassesV2Async(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        var now = NowUtc7(clock);

        // Chủ lớp = teacher@demo.local (SeedRunner đã tạo); fallback admin nếu thiếu. KHÔNG tạo user mới.
        var teacher = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == "teacher@demo.local", ct);
        if (teacher is null)
        {
            teacher = await db.Users.AsNoTracking().FirstAsync(u => u.IsPrimaryAdmin, ct);
        }

        var (users, lessons, exercises, _) = await LoadV2ContextAsync(db, ct);
        if (users.Count == 0)
        {
            logger.LogWarning("Seed: V2 Classes bỏ qua (chưa có user V2 nào)");
            return;
        }

        var usersByEmail = users;
        var lessonsByTitle = lessons.ToDictionary(l => l.Title, StringComparer.Ordinal);
        var exercisesByTitle = exercises.ToDictionary(e => e.Title, StringComparer.Ordinal);

        var classesAdded = 0;
        var classesSkipped = 0;
        var membersAdded = 0;
        var membersSkipped = 0;
        var assignmentsAdded = 0;
        var assignmentsSkipped = 0;
        var attached = 0;
        var createdNew = 0;
        var attachSkipped = 0;

        foreach (var spec in BuildClassPlanV2())
        {
            // ── 1. Classes — guard theo InviteCode (UNIQUE): AI1702 tạo mới; DSA213/ADVNCE đã tồn tại → bỏ qua ──
            var classRoom = await db.Classes.FirstOrDefaultAsync(c => c.InviteCode == spec.InviteCode, ct);
            if (classRoom is null)
            {
                classRoom = new Class
                {
                    Name = spec.Name,
                    InviteCode = spec.InviteCode,
                    Semester = spec.Semester,
                    Description = spec.Description,
                    OwnerId = teacher.Id,
                    Status = (ClassStatus)spec.Status,
                    CreatedAt = now.AddDays(-spec.CreatedDaysAgo)
                };
                db.Classes.Add(classRoom);
                await db.SaveChangesAsync(ct);   // lấy Id ngay cho member/assignment
                classesAdded++;
            }
            else
            {
                classesSkipped++;
            }

            // ── 2. ClassMembers — guard UNIQUE (ClassId, UserId); JoinedAt rải sau ngày tạo lớp ──
            foreach (var member in spec.Members)
            {
                if (!usersByEmail.TryGetValue(member.Email, out var user))
                {
                    membersSkipped++;
                    continue;
                }

                var exists = await db.ClassMembers.AnyAsync(
                    m => m.ClassId == classRoom.Id && m.UserId == user.Id, ct);
                if (exists)
                {
                    membersSkipped++;
                    continue;
                }

                db.ClassMembers.Add(new ClassMember
                {
                    ClassId = classRoom.Id,
                    UserId = user.Id,
                    JoinedAt = now.AddDays(-member.JoinedDaysAgo)
                        .AddHours(-(member.JoinedDaysAgo % 5))
                        .AddMinutes(-(member.JoinedDaysAgo * 7 % 60))
                });
                membersAdded++;
            }

            await db.SaveChangesAsync(ct);

            // ── 3. ClassAssignments — guard (ClassId, LessonId) / (ClassId, ExerciseId); CHECK 1 trong 2 ──
            foreach (var assignment in spec.Assignments)
            {
                if (assignment.ExerciseTitle is { } exerciseTitle)
                {
                    if (!exercisesByTitle.TryGetValue(exerciseTitle, out var exercise))
                    {
                        logger.LogWarning("Seed: V2 ClassAssignments bỏ qua {Title} (exercise chưa tồn tại)", exerciseTitle);
                        assignmentsSkipped++;
                        continue;
                    }

                    var entity = await db.ClassAssignments.FirstOrDefaultAsync(
                        a => a.ClassId == classRoom.Id && a.ExerciseId == exercise.Id, ct);
                    if (entity is null)
                    {
                        entity = new ClassAssignment
                        {
                            ClassId = classRoom.Id,
                            ExerciseId = exercise.Id,
                            DueAt = now.AddDays(assignment.DueOffsetDays),
                            CreatedAt = now.AddDays(-assignment.CreatedDaysAgo)
                        };
                        db.ClassAssignments.Add(entity);
                        await db.SaveChangesAsync(ct);
                        assignmentsAdded++;
                    }
                    else
                    {
                        assignmentsSkipped++;
                    }

                    // ── 4. Gắn ExerciseSubmission.ClassAssignmentId (bài gán theo exercise mới có nộp bài) ──
                    var result = await AttachClassSubmissionsV2Async(
                        db, usersByEmail, entity, exercise, assignment.Submissions, now, ct);
                    attached += result.Attached;
                    createdNew += result.CreatedNew;
                    attachSkipped += result.Skipped;
                }
                else if (assignment.LessonTitle is { } lessonTitle)
                {
                    if (!lessonsByTitle.TryGetValue(lessonTitle, out var lesson))
                    {
                        logger.LogWarning("Seed: V2 ClassAssignments bỏ qua {Title} (lesson chưa tồn tại)", lessonTitle);
                        assignmentsSkipped++;
                        continue;
                    }

                    var exists = await db.ClassAssignments.AnyAsync(
                        a => a.ClassId == classRoom.Id && a.LessonId == lesson.Id, ct);
                    if (exists)
                    {
                        assignmentsSkipped++;
                        continue;
                    }

                    db.ClassAssignments.Add(new ClassAssignment
                    {
                        ClassId = classRoom.Id,
                        LessonId = lesson.Id,
                        DueAt = now.AddDays(assignment.DueOffsetDays),
                        CreatedAt = now.AddDays(-assignment.CreatedDaysAgo)
                    });
                    await db.SaveChangesAsync(ct);
                    assignmentsAdded++;
                }
            }
        }

        logger.LogInformation("Seed: V2 Classes thêm {Added} / bỏ qua {Skipped} (đã tồn tại)", classesAdded, classesSkipped);
        logger.LogInformation("Seed: V2 ClassMembers thêm {Added} / bỏ qua {Skipped} (đã tồn tại)", membersAdded, membersSkipped);
        logger.LogInformation("Seed: V2 ClassAssignments thêm {Added} / bỏ qua {Skipped} (đã tồn tại)", assignmentsAdded, assignmentsSkipped);
        logger.LogInformation(
            "Seed: V2 ClassAssignmentId gắn {Attached} / tạo mới {CreatedNew} / bỏ qua {Skipped} (đã gắn)",
            attached, createdNew, attachSkipped);
    }

    /// <summary>
    /// Gắn ClassAssignmentId cho submission của member V2 theo kế hoạch (đúng hạn/trễ). Pattern y hệt
    /// AttachClassSubmissionsAsync V1: ưu tiên tái dùng submission Task 3 còn ClassAssignmentId = null
    /// khớp thời điểm; không có → tạo mới (Score hợp lệ, SubmittedAt không mâu thuẫn DueAt).
    /// Guard: không gắn 2 lần cho (UserId, ClassAssignmentId).
    /// </summary>
    private static async Task<(int Attached, int CreatedNew, int Skipped)> AttachClassSubmissionsV2Async(
        AppDbContext db,
        IReadOnlyDictionary<string, User> usersByEmail,
        ClassAssignment assignment,
        Exercise exercise,
        IReadOnlyList<PlannedClassSubmissionSpec> submissions,
        DateTime now,
        CancellationToken ct)
    {
        if (submissions.Count == 0)
        {
            return (0, 0, 0);
        }

        var userIds = submissions
            .Select(s => usersByEmail.TryGetValue(s.Email, out var user) ? (int?)user.Id : null)
            .Where(id => id is not null)
            .Select(id => id!.Value)
            .ToHashSet();
        if (userIds.Count == 0)
        {
            return (0, 0, 0);
        }

        var dueAt = assignment.DueAt ?? DateTime.MaxValue;
        var candidates = await db.ExerciseSubmissions
            .Where(s => s.ExerciseId == exercise.Id && userIds.Contains(s.UserId))
            .ToListAsync(ct);   // tracked — gắn ClassAssignmentId trực tiếp

        var attached = 0;
        var createdNew = 0;
        var skipped = 0;
        foreach (var sub in submissions)
        {
            if (!usersByEmail.TryGetValue(sub.Email, out var user))
            {
                skipped++;
                continue;
            }

            // Guard: user đã có submission gắn assignment này → bỏ qua (chống gắn 2 lần)
            if (candidates.Any(s => s.UserId == user.Id && s.ClassAssignmentId == assignment.Id))
            {
                skipped++;
                continue;
            }

            // Ưu tiên tái dùng submission Task 3 (ClassAssignmentId = null) khớp đúng hạn/trễ
            var match = candidates
                .Where(s => s.UserId == user.Id && s.ClassAssignmentId == null)
                .Where(s => sub.Late ? s.SubmittedAt > dueAt : s.SubmittedAt <= dueAt)
                .OrderByDescending(s => s.SubmittedAt)
                .FirstOrDefault();
            if (match is not null)
            {
                match.ClassAssignmentId = assignment.Id;
                attached++;
                continue;
            }

            // Không có submission phù hợp → tự tạo mới (Score 0..MaxScore; trễ → SubmittedAt > DueAt)
            var submittedAt = dueAt.AddDays(sub.NewOffsetDays);
            var minCreated = assignment.CreatedAt.AddHours(1);
            if (submittedAt < minCreated)
            {
                submittedAt = minCreated;
            }

            if (submittedAt > now)
            {
                submittedAt = now.AddHours(-1);
            }

            db.ExerciseSubmissions.Add(new ExerciseSubmission
            {
                UserId = user.Id,
                ExerciseId = exercise.Id,
                ClassAssignmentId = assignment.Id,
                Score = Math.Clamp(sub.NewScore, 0, Math.Max(0, exercise.MaxScore)),
                AnswersJson = "[]",
                ResultJson = "[]",
                DurationSeconds = null,
                SubmittedAt = submittedAt
            });
            createdNew++;
        }

        await db.SaveChangesAsync(ct);
        return (attached, createdNew, skipped);
    }

    // ── Kế hoạch dữ liệu deterministic V2 (dùng chung guard, chạy lại lần 2 → 0 thêm) ──

    /// <summary>Kế hoạch 1 lớp V2: lớp mới AI1702 đầy đủ thông tin; DSA213/ADVNCE chỉ cần InviteCode
    /// để guard (class đã tồn tại từ V1 — các field khác không dùng khi class đã có).</summary>
    private sealed record PlannedClassV2Spec(
        string Name,
        string InviteCode,
        string Semester,
        string Description,
        int Status,
        int CreatedDaysAgo,
        IReadOnlyList<PlannedClassMemberSpec> Members,
        IReadOnlyList<PlannedClassAssignmentSpec> Assignments);

    /// <summary>
    /// Lớp MỚI AI1702 (đóng, 24 member): 6 bài gán — 2 quá hạn (Quiz BFS −5 ngày, Lab BFS −2 ngày),
    /// 2 sắp hạn (Code BFS +3 ngày, Lesson BFS +5 ngày), 2 tương lai (Lesson Hash Table +9 ngày,
    /// Lesson BST +15 ngày). Bài gán exercise: HW/AVG nộp đúng hạn + 1 nhóm nộp trễ (chỉ bài
    /// gán quá hạn mới có Late khả thi — clamp SubmittedAt ≤ now) + NEW/Slacker chưa nộp → LaggingLearners.
    ///
    /// SD21361 (mở, giữ 7 member cũ): thêm 16 member (Slacker 13 + HW 3) + 4 bài gán (Lab Binary Search
    /// −1 ngày, Code Binary Search +2 ngày, Quiz Stack +6 ngày, Lesson Linked List +12 ngày).
    ///
    /// SD21361NC (đóng, giữ 5 member cũ): thêm 13 member (HW 5 + AVG 8) + 3 bài gán (Quiz BST −3 ngày,
    /// Lab Stack +1 ngày, Lesson AVL +10 ngày). AVG cuối nộp ít → LaggingLearners.
    ///
    /// Email lấy trực tiếp từ <see cref="V2Users"/> theo Index (H=Hardworking 0-12, A=Average 13-44,
    /// S=Slacker 45-57, N=New 58-67) — nguồn duy nhất, chạy lại vẫn ổn định.
    /// </summary>
    private static IReadOnlyList<PlannedClassV2Spec> BuildClassPlanV2()
    {
        string H(int i) => V2Users.Hardworking[i].Email;
        string A(int i) => V2Users.Average[i].Email;
        string S(int i) => V2Users.Slacker[i].Email;
        string N(int i) => V2Users.New[i].Email;

        return
        [
            // ── Lớp MỚI: AI1702 — Thuật toán Đồ thị (ĐÓNG, 24 thành viên) ──
            new PlannedClassV2Spec(
                Name: "AI1702 — Thuật toán Đồ thị",
                InviteCode: "GRPH21",
                Semester: "HK1-2026",
                Description: "Lớp chuyên đề Thuật toán Đồ thị — biểu diễn đồ thị, duyệt BFS/DFS và bài toán đường đi ngắn nhất dành cho sinh viên quan tâm lập trình thi đấu.",
                Status: (int)ClassStatus.Closed,
                CreatedDaysAgo: 15,
                Members:
                [
                    new(H(0), 13), new(H(1), 12), new(H(2), 11), new(H(3), 10), new(H(4), 9),
                    new(A(0), 14), new(A(1), 13), new(A(2), 12), new(A(3), 11), new(A(4), 10),
                    new(A(5), 9), new(A(6), 8), new(A(7), 7), new(A(8), 6), new(A(9), 5),
                    new(A(10), 4), new(A(11), 3),
                    new(S(0), 8), new(S(1), 7), new(S(2), 6), new(S(3), 5), new(S(4), 4),
                    new(N(0), 2), new(N(1), 1),
                ],
                Assignments:
                [
                    new(
                        ExerciseTitle: "Quiz: BFS",
                        LessonTitle: null,
                        CreatedDaysAgo: 12,
                        DueOffsetDays: -5,
                        Submissions:
                        [
                            new(H(0), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(H(1), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(H(2), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(A(0), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(A(1), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(A(2), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(A(3), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(A(4), Late: true, NewScore: 5, NewOffsetDays: 2),
                            new(A(5), Late: true, NewScore: 6, NewOffsetDays: 2),
                            new(A(6), Late: true, NewScore: 7, NewOffsetDays: 2),
                            new(S(0), Late: true, NewScore: 5, NewOffsetDays: 3),
                        ]),
                    new(
                        ExerciseTitle: "Lab: BFS",
                        LessonTitle: null,
                        CreatedDaysAgo: 8,
                        DueOffsetDays: -2,
                        Submissions:
                        [
                            new(H(0), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(H(1), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(H(2), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(H(3), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(A(0), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(A(1), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(A(2), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(A(3), Late: true, NewScore: 7, NewOffsetDays: 2),
                            new(A(4), Late: true, NewScore: 8, NewOffsetDays: 2),
                            new(S(1), Late: true, NewScore: 6, NewOffsetDays: 3),
                        ]),
                    new(
                        ExerciseTitle: "Code: BFS",
                        LessonTitle: null,
                        CreatedDaysAgo: 3,
                        DueOffsetDays: 3,
                        Submissions:
                        [
                            new(H(0), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(H(1), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(H(2), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(H(3), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(H(4), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(A(0), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(A(1), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(A(2), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(A(3), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(A(4), Late: false, NewScore: 0, NewOffsetDays: -1),
                        ]),
                    new(ExerciseTitle: null, LessonTitle: "BFS", CreatedDaysAgo: 1, DueOffsetDays: 5, Submissions: []),
                    new(ExerciseTitle: null, LessonTitle: "Hash Table", CreatedDaysAgo: 1, DueOffsetDays: 9, Submissions: []),
                    new(ExerciseTitle: null, LessonTitle: "BST", CreatedDaysAgo: 1, DueOffsetDays: 15, Submissions: []),
                ]),
            // ── SD21361 — Cấu trúc dữ liệu HK1 2026 (MỞ, giữ 7 member V1 + 7 assignment cũ) ──
            new PlannedClassV2Spec(
                Name: "SD21361 — Cấu trúc dữ liệu HK1 2026",
                InviteCode: "DSA213",
                Semester: "HK1-2026",
                Description: "Lớp chính khóa môn Cấu trúc dữ liệu và Giải thuật — học kỳ 1 năm 2026.",
                Status: (int)ClassStatus.Open,
                CreatedDaysAgo: 28,
                Members:
                [
                    new(S(0), 13), new(S(1), 12), new(S(2), 11), new(S(3), 10), new(S(4), 9),
                    new(S(5), 8), new(S(6), 7), new(S(7), 6), new(S(8), 5), new(S(9), 4),
                    new(S(10), 3), new(S(11), 2), new(S(12), 1),
                    new(H(10), 3), new(H(11), 2), new(H(12), 1),
                ],
                Assignments:
                [
                    new(
                        ExerciseTitle: "Lab: Binary Search",
                        LessonTitle: null,
                        CreatedDaysAgo: 2,
                        DueOffsetDays: -1,
                        Submissions:
                        [
                            new(H(10), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(H(11), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(H(12), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(S(0), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(S(1), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(S(2), Late: true, NewScore: 6, NewOffsetDays: 2),
                            new(S(3), Late: true, NewScore: 5, NewOffsetDays: 3),
                        ]),
                    new(
                        ExerciseTitle: "Code: Binary Search",
                        LessonTitle: null,
                        CreatedDaysAgo: 1,
                        DueOffsetDays: 2,
                        Submissions:
                        [
                            new(H(10), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(H(11), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(H(12), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(S(4), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(S(5), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(S(6), Late: false, NewScore: 0, NewOffsetDays: -1),
                        ]),
                    new(
                        ExerciseTitle: "Quiz: Stack",
                        LessonTitle: null,
                        CreatedDaysAgo: 1,
                        DueOffsetDays: 6,
                        Submissions:
                        [
                            new(H(10), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(H(11), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(H(12), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(S(0), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(S(1), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(S(2), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(S(3), Late: false, NewScore: 0, NewOffsetDays: -1),
                        ]),
                    new(ExerciseTitle: null, LessonTitle: "Linked List", CreatedDaysAgo: 1, DueOffsetDays: 12, Submissions: []),
                ]),
            // ── SD21361NC — Giải thuật nâng cao HK1 2026 (ĐÓNG, giữ 5 member V1 + 3 assignment cũ) ──
            new PlannedClassV2Spec(
                Name: "SD21361NC — Giải thuật nâng cao HK1 2026",
                InviteCode: "ADVNCE",
                Semester: "HK1-2026",
                Description: "Lớp nâng cao chuyên sâu thuật toán cho sinh viên khá giỏi — đã đóng tuyển.",
                Status: (int)ClassStatus.Closed,
                CreatedDaysAgo: 18,
                Members:
                [
                    new(H(5), 3), new(H(6), 3), new(H(7), 2), new(H(8), 2), new(H(9), 1),
                    new(A(0), 8), new(A(1), 7), new(A(2), 6), new(A(3), 5), new(A(4), 4),
                    new(A(5), 3), new(A(6), 2), new(A(7), 1),
                ],
                Assignments:
                [
                    new(
                        ExerciseTitle: "Quiz: BST",
                        LessonTitle: null,
                        CreatedDaysAgo: 4,
                        DueOffsetDays: -3,
                        Submissions:
                        [
                            new(H(5), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(H(6), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(H(7), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(H(8), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(A(0), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(A(1), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(A(2), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(A(3), Late: true, NewScore: 5, NewOffsetDays: 2),
                            new(A(4), Late: true, NewScore: 6, NewOffsetDays: 3),
                        ]),
                    new(
                        ExerciseTitle: "Lab: Stack",
                        LessonTitle: null,
                        CreatedDaysAgo: 1,
                        DueOffsetDays: 1,
                        Submissions:
                        [
                            new(H(5), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(H(6), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(H(7), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(H(8), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(H(9), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(A(0), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(A(1), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(A(2), Late: false, NewScore: 0, NewOffsetDays: -1),
                            new(A(3), Late: false, NewScore: 0, NewOffsetDays: -1),
                        ]),
                    new(ExerciseTitle: null, LessonTitle: "AVL", CreatedDaysAgo: 1, DueOffsetDays: 10, Submissions: []),
                ]),
        ];
    }
}
