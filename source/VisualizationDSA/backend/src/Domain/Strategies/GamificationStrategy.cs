using VisualizationDSA.Domain.Engine;

namespace VisualizationDSA.Domain.Strategies
{
    /// <summary>
    /// Gamification Strategy: Cấu hình cấp độ, danh hiệu và quy tắc tính XP/Level của nền tảng.
    /// Toàn bộ dữ liệu người dùng và bảng xếp hạng được truy vấn trực tiếp từ cơ sở dữ liệu.
    /// </summary>
    public class GamificationStrategy
    {
        private static readonly (int level, string name, int xpRequired, string color)[] LevelTable =
        {
            (1, "Novice",       0,    "#64748b"),
            (2, "Explorer",     100,  "#22c55e"),
            (3, "Learner",      300,  "#3b82f6"),
            (4, "Practitioner", 600,  "#8b5cf6"),
            (5, "Expert",       1000, "#f59e0b"),
            (6, "Master",       1500, "#ef4444"),
            (7, "Grandmaster",  2200, "#ec4899"),
            (8, "Legend",       3000, "#f97316"),
        };

        private static readonly StatelessBadgeDto[] BadgeTemplates =
        {
            new() { Id = "first-steps",      Name = "First Steps",      Description = "Hoàn thành bài trắc nghiệm đầu tiên",     Icon = "🎯", Color = "#22c55e" },
            new() { Id = "sorting-wizard",   Name = "Sorting Wizard",   Description = "Hoàn thành 4 thuật toán sắp xếp",         Icon = "⚡", Color = "#3b82f6" },
            new() { Id = "oop-guru",         Name = "OOP Guru",         Description = "Hiểu rõ Encapsulation & Inheritance",      Icon = "🔐", Color = "#8b5cf6" },
            new() { Id = "solid-master",     Name = "SOLID Master",     Description = "Áp dụng đúng 5 nguyên lý SOLID",           Icon = "🏛️", Color = "#f59e0b" },
            new() { Id = "pattern-hunter",   Name = "Pattern Hunter",   Description = "Sử dụng 3 Design Patterns",                Icon = "🎨", Color = "#ec4899" },
            new() { Id = "streak-keeper",    Name = "Streak Keeper",    Description = "Học liên tục 7 ngày",                      Icon = "🔥", Color = "#ef4444" },
            new() { Id = "system-architect", Name = "System Architect", Description = "Thiết kế hệ thống phân tán",               Icon = "🏗️", Color = "#f97316" },
            new() { Id = "dsa-champion",     Name = "DSA Champion",     Description = "Hoàn thành toàn bộ khóa học",              Icon = "👑", Color = "#eab308" },
        };

        public GamificationStrategy()
        {
        }

        public List<StatelessBadgeDto> GetAllBadges() =>
            BadgeTemplates.Select(b => new StatelessBadgeDto
            {
                Id = b.Id, Name = b.Name, Description = b.Description,
                Icon = b.Icon, Color = b.Color,
                EarnedAt = string.Empty
            }).ToList();

        public object GetConfig() => new
        {
            levels = LevelTable.Select(l => new { l.level, l.name, l.xpRequired, l.color }),
            badges = BadgeTemplates.Select(b => new { b.Id, b.Name, b.Description, b.Icon, b.Color }),
            xpEvents = new[]
            {
                new { type = "QUIZ_COMPLETE",  defaultXp = 50,  description = "Hoàn thành một quiz" },
                new { type = "MODULE_FINISH",  defaultXp = 100, description = "Hoàn thành một module học tập" },
                new { type = "STREAK_BONUS",   defaultXp = 25,  description = "Bonus streak hàng ngày" },
                new { type = "ACHIEVEMENT",    defaultXp = 200, description = "Đạt thành tích đặc biệt" },
            }
        };

        public static int CalculateLevel(int totalXp)
        {
            for (int i = LevelTable.Length - 1; i >= 0; i--)
                if (totalXp >= LevelTable[i].xpRequired) return LevelTable[i].level;
            return 1;
        }

        public static string GetLevelName(int level) =>
            LevelTable.FirstOrDefault(l => l.level == level).name ?? "Novice";
    }
}
