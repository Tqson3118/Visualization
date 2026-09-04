using DsaVisual.Application.Common;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Persistence.Seed;

/// <summary>
/// SEED-3 — nhóm lớp học của <see cref="SeedDemoActivity"/> (SDD §7.5, Module H — SDD §7.3.16/17/18):
/// 2 lớp từ SeedData.ClassProfiles (DSA213 mở, 7 thành viên / ADVNCE đóng, 5 thành viên) +
/// ClassMember + ClassAssignment (DueAt quá khứ &amp; tương lai) + GẮN ExerciseSubmission.ClassAssignmentId
/// (bước 7 để null, gắn tại đây) để báo cáo lớp (ClassService.GetReportAsync, FR-8.3/8.4) có đủ 3 nhóm:
/// nộp đúng hạn (SubmittedAt ≤ DueAt) / nộp trễ (SubmittedAt &gt; DueAt) / chưa nộp → LaggingLearners
/// (thiếu ≥ 2 bài gán) hiển thị đúng với dữ liệu demo.
///
/// Idempotent theo pattern SeedRunner: guard AnyAsync/FirstOrDefault → Add/modify → SaveChanges → log
/// ("Seed: X thêm / bỏ qua (đã tồn tại)"); chạy lần 2 → 0 thêm. KHÔNG tạo user mới, KHÔNG đụng user rác
/// smoke, KHÔNG RemoveRange/ExecuteDelete. ClassMember không có cột Role (entity chỉ ClassId/UserId/JoinedAt).
/// Nộp bài: ưu tiên tái dùng ExerciseSubmission đã seed ở bước 7 (query theo user+exercise, khớp đúng
/// hạn/trễ với DueAt); không có submission phù hợp → tự tạo mới (Score 0..MaxScore, trễ → SubmittedAt &gt; DueAt).
/// </summary>
public static partial class SeedDemoActivity
{
    private static partial Task SeedClassesAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
        => SeedClassesCoreAsync(db, clock, logger, ct);

    private static async Task SeedClassesCoreAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        var now = NowUtc7(clock);

        // Chủ lớp = teacher@demo.local (SeedRunner đã tạo); fallback admin nếu thiếu. KHÔNG tạo user mới.
        var teacher = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == "teacher@demo.local", ct);
        if (teacher is null)
        {
            teacher = await db.Users.AsNoTracking().FirstAsync(u => u.IsPrimaryAdmin, ct);
        }

        var students = await LoadDemoStudentsAsync(db, ct);
        if (students.Count == 0)
        {
            logger.LogWarning("Seed: Classes bỏ qua (chưa có student demo)");
            return;
        }

        var studentsByEmail = students.ToDictionary(u => u.Email, StringComparer.OrdinalIgnoreCase);
        var lessons = (await db.Lessons.AsNoTracking()
            .Where(l => l.DeletedAt == null && l.Status == LessonStatus.Active)
            .ToListAsync(ct))
            .GroupBy(l => l.Title, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
        var exercises = (await db.Exercises.AsNoTracking()
            .Where(e => e.DeletedAt == null && e.Status == ExerciseStatus.Active)
            .ToListAsync(ct))
            .GroupBy(e => e.Title, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        var classesAdded = 0;
        var classesSkipped = 0;
        var membersAdded = 0;
        var membersSkipped = 0;
        var assignmentsAdded = 0;
        var assignmentsSkipped = 0;
        var attached = 0;
        var createdNew = 0;
        var attachSkipped = 0;

        foreach (var spec in BuildClassPlan())
        {
            if (spec.ProfileIndex >= SeedData.ClassProfiles.Count)
            {
                continue;
            }

            var profile = SeedData.ClassProfiles[spec.ProfileIndex];

            // ── 1. Classes — guard theo InviteCode (UNIQUE); fallback tự nhiên (ClassName, Semester) ──
            var classRoom = await db.Classes.FirstOrDefaultAsync(c => c.InviteCode == profile.InviteCode, ct);
            if (classRoom is null)
            {
                classRoom = new Class
                {
                    Name = profile.ClassName,
                    InviteCode = profile.InviteCode,
                    Semester = profile.Semester,
                    Description = profile.Description,
                    OwnerId = teacher.Id,
                    Status = (ClassStatus)profile.Status,
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
                if (!studentsByEmail.TryGetValue(member.Email, out var user))
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
                    if (!exercises.TryGetValue(exerciseTitle, out var exercise))
                    {
                        logger.LogWarning("Seed: ClassAssignments bỏ qua {Title} (exercise chưa tồn tại)", exerciseTitle);
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
                    var result = await AttachClassSubmissionsAsync(
                        db, studentsByEmail, entity, exercise, assignment.Submissions, now, ct);
                    attached += result.Attached;
                    createdNew += result.CreatedNew;
                    attachSkipped += result.Skipped;
                }
                else if (assignment.LessonTitle is { } lessonTitle)
                {
                    if (!lessons.TryGetValue(lessonTitle, out var lesson))
                    {
                        logger.LogWarning("Seed: ClassAssignments bỏ qua {Title} (lesson chưa tồn tại)", lessonTitle);
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

        logger.LogInformation("Seed: Classes thêm {Added} / bỏ qua {Skipped} (đã tồn tại)", classesAdded, classesSkipped);
        logger.LogInformation("Seed: ClassMembers thêm {Added} / bỏ qua {Skipped} (đã tồn tại)", membersAdded, membersSkipped);
        logger.LogInformation("Seed: ClassAssignments thêm {Added} / bỏ qua {Skipped} (đã tồn tại)", assignmentsAdded, assignmentsSkipped);
        logger.LogInformation(
            "Seed: ClassAssignmentId gắn {Attached} / tạo mới {CreatedNew} / bỏ qua {Skipped} (đã gắn)",
            attached, createdNew, attachSkipped);
    }

    /// <summary>
    /// Gắn ClassAssignmentId cho submission của member theo kế hoạch (đúng hạn/trễ). Ưu tiên tái dùng
    /// submission bước 7 còn ClassAssignmentId = null khớp thời điểm; không có → tạo mới (Score hợp lệ,
    /// SubmittedAt không mâu thuẫn DueAt). Guard: không gắn 2 lần cho (UserId, ClassAssignmentId).
    /// </summary>
    private static async Task<(int Attached, int CreatedNew, int Skipped)> AttachClassSubmissionsAsync(
        AppDbContext db,
        IReadOnlyDictionary<string, User> studentsByEmail,
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
            .Select(s => studentsByEmail.TryGetValue(s.Email, out var user) ? (int?)user.Id : null)
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
            if (!studentsByEmail.TryGetValue(sub.Email, out var user))
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

            // Ưu tiên tái dùng submission đã seed (ClassAssignmentId = null) khớp đúng hạn/trễ
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

    // ── Kế hoạch dữ liệu deterministic (dùng chung guard, chạy lại lần 2 → 0 thêm) ──

    private sealed record PlannedClassSpec(
        int ProfileIndex,
        int CreatedDaysAgo,
        IReadOnlyList<PlannedClassMemberSpec> Members,
        IReadOnlyList<PlannedClassAssignmentSpec> Assignments);

    private sealed record PlannedClassMemberSpec(string Email, int JoinedDaysAgo);

    private sealed record PlannedClassAssignmentSpec(
        string? ExerciseTitle,
        string? LessonTitle,
        int CreatedDaysAgo,
        int DueOffsetDays,   // âm = quá khứ, dương = tương lai (so với hôm nay UTC+7)
        IReadOnlyList<PlannedClassSubmissionSpec> Submissions);

    /// <summary>NewScore/NewOffsetDays chỉ dùng khi phải TẠO MỚI submission (không tái dùng được).</summary>
    private sealed record PlannedClassSubmissionSpec(
        string Email,
        bool Late,
        int NewScore,
        int NewOffsetDays);

    /// <summary>
    /// Lớp 1 (mở, 7 thành viên): 4 bài gán — 2 quá khứ (Quiz Bubble Sort hết hạn 20 ngày trước, Quiz
    /// Binary Search hết hạn 13 ngày trước) + 2 tương lai (Lab Bubble Sort +5 ngày, Lesson Stack +12 ngày).
    /// Mỗi bài gán quá khứ: 3 nộp đúng hạn (m0-m2 tái dùng full-score), 2 nộp trễ (m3-m4 tái dùng bài
    /// nộp sau hạn), 2 chưa nộp (m5 Nguyễn Trang + m8 student@demo.local → LaggingLearners, thiếu ≥ 2).
    ///
    /// Lớp 2 (đóng, 5 thành viên): 3 bài gán — Code Bubble Sort hết hạn 9 ngày trước (3 đúng hạn, 1 trễ),
    /// Quiz Stack hết hạn 4 ngày trước (3 đúng hạn, 1 trễ), Lesson Binary Search +8 ngày (không nộp —
    /// bài gán theo lesson). student@demo.local không nộp gì → LaggingLearners.
    /// </summary>
    private static IReadOnlyList<PlannedClassSpec> BuildClassPlan()
    {
        // Thứ tự LoadDemoStudentsAsync: SeedData.Students (0-7) + student@demo.local (8)
        string[] emails =
        [
            "nguyenminhanh@university.edu.vn",   // 0
            "tranquocbao@university.edu.vn",     // 1
            "lethikimngan@university.edu.vn",    // 2
            "phamhoanglong@university.edu.vn",   // 3
            "vuthanhtung@university.edu.vn",     // 4
            "nguyentrang@university.edu.vn",     // 5
            "doanminhduc@university.edu.vn",     // 6
            "huynhthuy@university.edu.vn",       // 7
            "student@demo.local",                // 8
        ];
        string E(int i) => emails[i];

        return
        [
            // ── Lớp 1: SD21361 — Cấu trúc dữ liệu HK1 2026 (MỞ, 7 thành viên) ──
            new PlannedClassSpec(
                ProfileIndex: 0,
                CreatedDaysAgo: 28,
                Members:
                [
                    new(E(0), 25), new(E(1), 22), new(E(2), 19), new(E(3), 15),
                    new(E(4), 13), new(E(5), 6), new(E(8), 3),
                ],
                Assignments:
                [
                    new(
                        ExerciseTitle: "Quiz: Bubble Sort",
                        LessonTitle: null,
                        CreatedDaysAgo: 28,
                        DueOffsetDays: -20,
                        Submissions:
                        [
                            new(E(0), Late: false, NewScore: 0, NewOffsetDays: 0),
                            new(E(1), Late: false, NewScore: 0, NewOffsetDays: 0),
                            new(E(2), Late: false, NewScore: 0, NewOffsetDays: 0),
                            new(E(3), Late: true, NewScore: 6, NewOffsetDays: 2),
                            new(E(4), Late: true, NewScore: 7, NewOffsetDays: 3),
                        ]),
                    new(
                        ExerciseTitle: "Quiz: Binary Search",
                        LessonTitle: null,
                        CreatedDaysAgo: 21,
                        DueOffsetDays: -13,
                        Submissions:
                        [
                            new(E(0), Late: false, NewScore: 0, NewOffsetDays: 0),
                            new(E(1), Late: false, NewScore: 0, NewOffsetDays: 0),
                            new(E(2), Late: false, NewScore: 0, NewOffsetDays: 0),
                            new(E(3), Late: true, NewScore: 5, NewOffsetDays: 2),
                            new(E(4), Late: true, NewScore: 6, NewOffsetDays: 3),
                        ]),
                    new(
                        ExerciseTitle: "Lab: Bubble Sort",
                        LessonTitle: null,
                        CreatedDaysAgo: 3,
                        DueOffsetDays: 5,
                        Submissions:
                        [
                            new(E(0), Late: false, NewScore: 0, NewOffsetDays: 0),
                            new(E(1), Late: false, NewScore: 0, NewOffsetDays: 0),
                            new(E(2), Late: false, NewScore: 8, NewOffsetDays: -7),
                            new(E(3), Late: false, NewScore: 0, NewOffsetDays: 0),
                            new(E(4), Late: false, NewScore: 0, NewOffsetDays: 0),
                        ]),
                    new(ExerciseTitle: null, LessonTitle: "Stack", CreatedDaysAgo: 1, DueOffsetDays: 12, Submissions: []),
                ]),
            // ── Lớp 2: SD21361NC — Giải thuật nâng cao HK1 2026 (ĐÓNG, 5 thành viên) ──
            new PlannedClassSpec(
                ProfileIndex: 1,
                CreatedDaysAgo: 18,
                Members:
                [
                    new(E(0), 14), new(E(1), 11), new(E(2), 8), new(E(3), 5), new(E(8), 2),
                ],
                Assignments:
                [
                    new(
                        ExerciseTitle: "Code: Bubble Sort",
                        LessonTitle: null,
                        CreatedDaysAgo: 16,
                        DueOffsetDays: -9,
                        Submissions:
                        [
                            new(E(0), Late: false, NewScore: 0, NewOffsetDays: 0),
                            new(E(1), Late: false, NewScore: 0, NewOffsetDays: 0),
                            new(E(2), Late: false, NewScore: 90, NewOffsetDays: -3),
                            new(E(3), Late: true, NewScore: 55, NewOffsetDays: 2),
                        ]),
                    new(
                        ExerciseTitle: "Quiz: Stack",
                        LessonTitle: null,
                        CreatedDaysAgo: 7,
                        DueOffsetDays: -4,
                        Submissions:
                        [
                            new(E(0), Late: false, NewScore: 0, NewOffsetDays: 0),
                            new(E(1), Late: false, NewScore: 0, NewOffsetDays: 0),
                            new(E(2), Late: false, NewScore: 0, NewOffsetDays: 0),
                            new(E(3), Late: true, NewScore: 5, NewOffsetDays: 2),
                        ]),
                    new(ExerciseTitle: null, LessonTitle: "Binary Search", CreatedDaysAgo: 1, DueOffsetDays: 8, Submissions: []),
                ]),
        ];
    }
}
