using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VisualizationDSA.Domain.Entities;

namespace VisualizationDSA.Infrastructure.Data
{
    public partial class DbSeeder
    {
        // ══════════════════════════════════════════════════════════════════════
        // S8 — THEORY ARTICLES (6 bài tiếng Việt, có version) + NOTIFICATIONS
        // ══════════════════════════════════════════════════════════════════════
        private async Task SeedTheoryArticlesAsync()
        {
            var author = await _context.Users.FirstOrDefaultAsync(u => u.Role == "Teacher")
                         ?? await _context.Users.FirstOrDefaultAsync(u => u.Role == "Admin");
            Console.WriteLine($"[SeedDebug] SeedTheoryArticles author={(author == null ? "NULL" : author.Email)}");
            if (author == null) return;

            await EnsureTheoryArticleAsync(author.Id, "Thuật toán Bubble Sort — Giải thích trực quan",
                "bubble-sort-truc-quan", "sorting", "Beginner", "sorting,bubble-sort,visualization", 6,
                "# Bubble Sort\n\nBubble Sort là thuật toán sắp xếp đơn giản nhất: lặp lại việc so sánh từng cặp liền kề và hoán đổi nếu sai thứ tự.\n\n## Cách hoạt động\n1. Duyệt từ đầu mảng, so sánh `a[j]` và `a[j+1]`.\n2. Nếu `a[j] > a[j+1]` thì hoán đổi.\n3. Sau mỗi lượt, phần tử lớn nhất 'nổi' về cuối.\n\n## Độ phức tạp\n- Tốt nhất: **O(n)** (có cờ dừng sớm).\n- Trung bình / xấu nhất: **O(n²)**.\n- Bộ nhớ: **O(1)**, ổn định (stable).\n\n## Mẹo\n- Mảng gần sắp xếp → Bubble Sort chạy rất nhanh.\n- Mở Visualizer trong ứng dụng để xem từng bước hoán đổi!");

            await EnsureTheoryArticleAsync(author.Id, "Quick Sort — Chia để trị trong thực tế",
                "quick-sort-chia-de-tri", "sorting", "Intermediate", "sorting,quick-sort,divide-conquer", 7,
                "# Quick Sort\n\nQuick Sort dùng pivot để phân hoạch mảng rồi đệ quy sắp xếp hai nửa.\n\n## Các bước\n1. Chọn pivot (thường là phần tử cuối hoặc ngẫu nhiên).\n2. Partition: đưa pivot về đúng vị trí, chia mảng thành < pivot và > pivot.\n3. Đệ quy hai nửa.\n\n## Độ phức tạp\n- Trung bình: **O(n log n)**.\n- Xấu nhất: **O(n²)** khi pivot luôn ở biên.\n\n## Mẹo\n- Chọn pivot ngẫu nhiên / median-of-three để tránh trường hợp xấu.\n- Thử đổi pivot và quan sát số lần so sánh trong Visualizer.");

            await EnsureTheoryArticleAsync(author.Id, "Duyệt đồ thị BFS & DFS",
                "bfs-dfs-duyet-do-thi", "graph", "Intermediate", "graph,bfs,dfs,traversal", 8,
                "# Duyệt đồ thị BFS & DFS\n\nBFS (chiều rộng) và DFS (chiều sâu) là hai cách duyệt đồ thị cơ bản.\n\n## BFS\n- Dùng **hàng đợi (queue)**, duyệt theo tầng.\n- Tìm đường đi ngắn nhất theo số cạnh trên đồ thị không trọng số.\n- Độ phức tạp **O(V + E)**.\n\n## DFS\n- Dùng **ngăn xếp (stack) / đệ quy**, đi sâu trước khi quay lui.\n- Phát hiện chu trình, thành phần liên thông, sắp xếp tô pô.\n- Độ phức tạp **O(V + E)**.\n\n## Mẹo\n- Luôn đánh dấu `visited` để tránh lặp vô hạn trên đồ thị có chu trình.\n- Xem hai thuật toán chạy song song trong Visualizer để so sánh thứ tự duyệt.");

            await EnsureTheoryArticleAsync(author.Id, "Lập trình hướng đối tượng (OOP) — 4 trụ cột",
                "oop-4-tru-cot", "oop", "Beginner", "oop,encapsulation,inheritance,polymorphism,abstraction", 9,
                "# 4 trụ cột của OOP\n\n## 1. Đóng gói (Encapsulation)\nẨn dữ liệu qua `private` + getter/setter, bảo vệ bất biến.\n\n## 2. Kế thừa (Inheritance)\nLớp con tái sử dụng thành viên lớp cha, override hành vi khi cần.\n\n## 3. Đa hình (Polymorphism)\nCùng giao diện nhưng hành vi khác nhau theo kiểu runtime (virtual/interface).\n\n## 4. Trừu tượng hóa (Abstraction)\nChỉ phơi bày giao diện, che giấu phức tạp (abstract class / interface).\n\n## Mẹo\n- Ưu tiên **composition** hơn inheritance khi quan hệ is-a không rõ ràng.\n- Áp dụng cùng SOLID để thiết kế bền vững.");

            await EnsureTheoryArticleAsync(author.Id, "SOLID — 5 nguyên lý thiết kế phần mềm",
                "solid-5-nguyen-ly", "solid", "Intermediate", "solid,srp,ocp,lsp,isp,dip", 10,
                "# SOLID\n\n## S — Single Responsibility\nMỗi lớp một trách nhiệm duy nhất.\n\n## O — Open/Closed\nMở cho mở rộng, đóng cho sửa đổi.\n\n## L — Liskov Substitution\nLớp con thay thế được lớp cha không phá vỡ hành vi.\n\n## I — Interface Segregation\nNhiều interface nhỏ, chuyên biệt.\n\n## D — Dependency Inversion\nPhụ thuộc trừu tượng, không phụ thuộc chi tiết.\n\n## Mẹo\n- Nhận diện vi phạm qua code smell: God class, switch theo type, ép kế thừa.\n- Dùng interface + DI container để tuân thủ DIP.");

            await EnsureTheoryArticleAsync(author.Id, "Độ phức tạp thuật toán (Big O) — Cẩm nang phỏng vấn",
                "big-o-cam-nang", "dsa", "Beginner", "big-o,complexity,interview", 6,
                "# Big O Notation\n\nBig O mô tả tốc độ tăng trưởng thời gian/bộ nhớ theo kích thước đầu vào n.\n\n## Các mức phổ biến\n- **O(1)** — hằng số: truy cập mảng.\n- **O(log n)** — logarit: binary search.\n- **O(n)** — tuyến tính: linear search.\n- **O(n log n)** — merge/quick/heap sort.\n- **O(n²)** — bubble/selection/insertion sort.\n- **O(2^n)** — đệ quy Fibonacci thuần.\n\n## Mẹo\n- Bỏ hằng số và hệ số: `O(2n)` → `O(n)`.\n- Giữ mức phức tạp lớn nhất: `O(n + n²)` → `O(n²)`.\n- Luôn hỏi: trường hợp tốt nhất / trung bình / xấu nhất?");

            await _context.SaveChangesAsync();
        }

        private async Task EnsureTheoryArticleAsync(Guid authorId, string title, string slug, string category, string difficulty, string tags, int readTime, string contentMd)
        {
            var article = await _context.TheoryArticles.FirstOrDefaultAsync(a => a.Slug == slug);
            if (article == null)
            {
                article = new TheoryArticle(authorId, title, slug, contentMd, category, difficulty, tags, readTime);
                article.Publish();
                article.CreateVersion(contentMd, "Phiên bản gốc khi seed", authorId);
                _context.TheoryArticles.Add(article);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedNotificationsAsync()
        {
            await EnsureNotificationAsync("demo@visualizationdsa.dev",
                "Đơn xin trở thành Giáo viên của bạn đã được admin duyệt. Chúc mừng!", "/profile", "TeacherApplication");
            await EnsureNotificationAsync("demo@visualizationdsa.dev",
                "Roadmap 'Thuật toán Sắp xếp (Sorting)' đã được duyệt và xuất bản.", "/courses", "Roadmap");
            await EnsureNotificationAsync("demo@visualizationdsa.dev",
                "Chào mừng bạn đến với VisualizationDSA! Bạn có 10 tim miễn phí để bắt đầu hành trình học.", "/algorithms", "Welcome");
            await EnsureNotificationAsync("demo@visualizationdsa.dev",
                "Streak 4 ngày! Học thêm hôm nay để giữ vững chuỗi ngày.", "/dashboard", "Streak");

            await EnsureNotificationAsync("reviewdemo@visualizationdsa.dev",
                "Bạn đã hoàn thành roadmap 'Thuật toán Sắp xếp (Sorting)' — hãy để lại đánh giá của bạn!", "/courses", "Roadmap");
            await EnsureNotificationAsync("nguyenvana@visualizationdsa.dev",
                "Chúc mừng! Bạn đã hoàn thành roadmap 'Đồ thị & Đường đi ngắn nhất (Graph)'.", "/courses", "Roadmap");

            await _context.SaveChangesAsync();
        }

        private async Task EnsureNotificationAsync(string email, string content, string linkUrl, string type)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return;

            var exists = await _context.Notifications.AnyAsync(n => n.UserId == user.Id && n.Content == content);
            if (exists) return;

            _context.Notifications.Add(new Notification(user.Id, content, linkUrl, type));
        }

        // ══════════════════════════════════════════════════════════════════════
        // S10 — QUESTS + INVENTORY + DAILY QUESTS (Gems Shop demo có trạng thái)
        // ══════════════════════════════════════════════════════════════════════
        private async Task SeedQuestsAndInventoryAsync()
        {
            // Quest templates bổ sung (DbContext đã HasData 6 quest cơ bản).
            await EnsureQuestTemplateAsync("STREAK", "Easy", "Duy trì streak 3 ngày liên tiếp", 3, 20);
            await EnsureQuestTemplateAsync("COMPLETE_LAB", "Medium", "Hoàn thành 1 codelab", 1, 30);
            await EnsureQuestTemplateAsync("WATCH_AD", "Easy", "Xem 1 quảng cáo nhận tim", 1, 10);
            await EnsureQuestTemplateAsync("LOGIN_DAILY", "Easy", "Đăng nhập 1 lần trong ngày", 1, 5);

            var demo = await _context.Users.FirstOrDefaultAsync(u => u.Email == "demo@visualizationdsa.dev");
            if (demo != null)
            {
                // UserDailyQuest cho hôm nay (demo có nhiệm vụ ngày hiển thị).
                var today = DateTime.UtcNow.Date;
                if (!await _context.UserDailyQuests.AnyAsync(q => q.UserId == demo.Id && q.Date == today))
                {
                    var quests = new[]
                    {
                        new UserDailyQuest(demo.Id, today, "EARN_XP", "Easy", "Kiếm 50 XP", 50, 10),
                        new UserDailyQuest(demo.Id, today, "COMPLETE_QUIZ", "Easy", "Hoàn thành 1 bài trắc nghiệm", 1, 10),
                        new UserDailyQuest(demo.Id, today, "COMPLETE_LAB", "Medium", "Hoàn thành 1 codelab", 1, 30),
                    };
                    quests[0].AddProgress(35);
                    _context.UserDailyQuests.AddRange(quests);
                }

                // UserInventory để Gems Shop / my-inventory có trạng thái.
                await EnsureInventoryAsync(demo.Id, "ai_hint_token", "Consumable");
                await EnsureInventoryAsync(demo.Id, "ai_hint_token", "Consumable");
                await EnsureInventoryAsync(demo.Id, "ai_hint_token", "Consumable");
                await EnsureInventoryAsync(demo.Id, "streak_freeze", "Consumable");
                await EnsureInventoryAsync(demo.Id, "frame_neon", "Permanent");
            }

            await _context.SaveChangesAsync();
        }

        private async Task EnsureQuestTemplateAsync(string questType, string difficulty, string description, int targetValue, int gemsReward)
        {
            var exists = await _context.QuestTemplates.AnyAsync(q => q.QuestType == questType && q.Description == description);
            if (exists) return;
            _context.QuestTemplates.Add(new QuestTemplate(questType, difficulty, description, targetValue, gemsReward));
        }

        private async Task EnsureInventoryAsync(Guid userId, string itemId, string itemType)
        {
            var exists = await _context.UserInventory.AnyAsync(i => i.UserId == userId && i.ItemId == itemId && i.ItemType == itemType);
            if (exists) return;
            _context.UserInventory.Add(new UserInventory(userId, itemId, itemType));
        }
    }
}
