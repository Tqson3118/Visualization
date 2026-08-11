using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VisualizationDSA.Domain.Entities;

namespace VisualizationDSA.Infrastructure.Data
{
    public partial class DbSeeder
    {
        // ══════════════════════════════════════════════════════════════════════
        // S1 — USERS (18 user, 3 vai, XP tăng dần cho BXH, 3 Premium, 2 Teacher)
        // Mật khẩu: Admin@2024 / Teacher@2024 / Demo@2024 / User@2024
        // ══════════════════════════════════════════════════════════════════════
        private async Task SeedUsersAsync()
        {
            var users = new (string email, string username, string password, string role, int xp, int streak, bool premium)[]
            {
                ("admin@visualizationdsa.dev",      "VisualizationDSA Admin", "Admin@2024",   "Admin",   9999, 30, false),
                ("teacher@visualizationdsa.dev",    "Nguyễn Minh Anh",         "Teacher@2024", "Teacher", 5200, 25, false),
                ("phungthilan@visualizationdsa.dev","Phùng Văn Long",          "Teacher@2024", "Teacher", 3600, 15, false),
                ("demo@visualizationdsa.dev",       "Học viên Demo",           "Demo@2024",    "Student",  680,  4, true),
                ("reviewdemo@visualizationdsa.dev", "Học viên Review Demo",    "Demo@2024",    "Student",  640,  3, false),
                ("nguyenvana@visualizationdsa.dev", "Nguyễn Văn An",           "User@2024",    "Student", 2850, 14, true),
                ("tranthib@visualizationdsa.dev",   "Trần Thị Bích",           "User@2024",    "Student", 2200, 10, true),
                ("levanc@visualizationdsa.dev",     "Lê Văn Cường",            "User@2024",    "Student", 1800,  8, false),
                ("phamthid@visualizationdsa.dev",   "Phạm Thị Dung",           "User@2024",    "Student", 1500, 12, false),
                ("hoangvane@visualizationdsa.dev",  "Hoàng Văn Em",            "User@2024",    "Student", 1200,  6, false),
                ("vuthif@visualizationdsa.dev",     "Vũ Thị Phương",           "User@2024",    "Student",  950,  5, false),
                ("dangvang@visualizationdsa.dev",   "Đặng Văn Giang",          "User@2024",    "Student",  700,  4, false),
                ("buithih@visualizationdsa.dev",    "Bùi Thị Hạnh",            "User@2024",    "Student",  450,  3, false),
                ("dovani@visualizationdsa.dev",     "Đỗ Văn Hưng",             "User@2024",    "Student",  250,  2, false),
                ("hongthikim@visualizationdsa.dev", "Hồng Thị Kim",            "User@2024",    "Student",  130,  1, false),
                ("duongvanlam@visualizationdsa.dev","Dương Văn Lâm",           "User@2024",    "Student",   90,  1, false),
                ("ngothimai@visualizationdsa.dev",  "Ngô Thị Mai",             "User@2024",    "Student",   60,  1, false),
                ("vovannam@visualizationdsa.dev",   "Võ Văn Nam",              "User@2024",    "Student",   30,  1, false),
            };

            foreach (var (email, username, password, role, xp, streak, premium) in users)
            {
                var user = await EnsureUserAsync(email, username, password, role, xp, streak, premium);
                if (role == "Teacher")
                {
                    user.SetTeacherAppStatus("Approved");
                }
            }

            // Gems cho user demo để Gems Shop demo có số dư (thêm vài token + freeze + frame).
            var demo = await _context.Users.FirstOrDefaultAsync(u => u.Email == "demo@visualizationdsa.dev");
            if (demo != null && demo.GemsCount == 0) demo.AddGems(320);

            await _context.SaveChangesAsync();
        }

        // Tạo/update user idempotent theo email. XP/streak/premium chỉ set lúc tạo mới
        // để không phình số liệu khi reseed trên DB cũ.
        private async Task<User> EnsureUserAsync(string email, string username, string password, string role, int xp, int streak, bool premium)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                user = new User(email, username, HashPasswordSHA256(password));
                _context.Users.Add(user);
                if (xp > 0) user.AwardXP(xp);
                if (premium) user.SetPremium(DateTime.UtcNow.AddMonths(6));
                user.RecordActivity();
                if (streak > 1) SetStreakForSeed(user, streak);
            }
            else
            {
                user.SetRole(role);
                if (premium && !user.IsPremium) user.SetPremium(DateTime.UtcNow.AddMonths(6));
                if (user.LastActivityDate == null) user.RecordActivity();
            }
            return user;
        }

        // User không có public setter cho StreakDays/LastActivityDate (chỉ RecordActivity tự cập nhật)
        // → set trực tiếp để BXH demo có streak tăng dần. Chỉ áp dụng cho dữ liệu seed.
        private static void SetStreakForSeed(User user, int streak)
        {
            typeof(User).GetProperty("StreakDays")?.SetValue(user, streak);
            typeof(User).GetProperty("LastActivityDate")?.SetValue(user, DateTime.UtcNow.AddDays(-(Math.Max(1, streak) - 1)));
        }
    }
}
