using System.Text.Json;
using DsaVisual.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DsaVisual.Application.Services;

/// <summary>
/// Helper tăng Progress quest theo SỰ KIỆN học tập (finding #6, FR-10.3 — "tiến độ TỰ cập nhật theo sự
/// kiện học tập"): atomic UPDATE <c>Progress = Progress + 1 WHERE Progress &lt; target</c> (+@@ROWCOUNT) cho
/// mọi quest hôm nay (QuestDate = hôm nay UTC+7, chưa claim) có ConditionJson.activity khớp.
/// Map activity (xem SeedData.Quests — 8 template FR-10.3A; ConditionJson key "activity"):
///   pass_node      → ExerciseService.SubmitAsync khi node VỪA chuyển sang passed (Status 1→2)
///   pass_quiz      → ExerciseService.SubmitAsync thành công (exercise type MCQ)
///   pass_lab       → ExerciseService.SubmitAsync thành công (exercise type SimulationLab)
///   code_run       → ExerciseService.SubmitCodeAsync thành công
///   lesson_viewed  → LessonService.MarkViewedAsync
///   streak         → không có sự kiện riêng (StreakDays là giá trị tích lũy — không tăng Progress)
/// Gọi TRONG CÙNG transaction với hành động học (raw SQL tham gia ambient tx).
/// Lưới an toàn bổ sung: GamificationService.GetQuestsAsync tự sync Progress từ trạng thái thật khi đọc.
/// </summary>
internal static class QuestProgressWriter
{
    public static async Task IncrementAsync(AppDbContext db, int userId, string activity, CancellationToken ct)
    {
        var today = DateTime.UtcNow.AddHours(7).Date;   // cùng quy ước TodayUtc7() của GamificationService

        var rows = await db.UserQuests.AsNoTracking()
            .Where(q => q.UserId == userId && q.QuestDate == today && !q.Claimed)
            .Join(db.DailyQuests.AsNoTracking(), uq => uq.QuestId, dq => dq.Id,
                (uq, dq) => new { UserQuestId = uq.Id, dq.ConditionJson })
            .ToListAsync(ct);

        foreach (var row in rows)
        {
            if (!string.Equals(GetActivity(row.ConditionJson), activity, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var target = GetTarget(row.ConditionJson);
            // Atomic + điều kiện: không tăng quá target, không đua double-count
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE UserQuests SET Progress = Progress + 1 WHERE Id = {row.UserQuestId} AND UserId = {userId} AND QuestDate = {today} AND Progress < {target}", ct);
        }
    }

    private static string? GetActivity(string conditionJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(conditionJson);
            return doc.RootElement.TryGetProperty("activity", out var activity) && activity.ValueKind == JsonValueKind.String
                ? activity.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static int GetTarget(string conditionJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(conditionJson);
            return doc.RootElement.TryGetProperty("count", out var count) && count.TryGetInt32(out var value)
                ? value
                : 1;
        }
        catch (JsonException)
        {
            return 1;
        }
    }
}
