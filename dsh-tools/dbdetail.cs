#:package Microsoft.Data.SqlClient@6.0.2
#:property PublishAot=false

using Microsoft.Data.SqlClient;

var cs = "Server=db65198.public.databaseasp.net; Database=db65198; User Id=db65198; Password=4Rn+#6EoB!a8; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True; Connect Timeout=20;";
using var conn = new SqlConnection(cs);
conn.Open();

void Dump(string title, string sql) {
    Console.WriteLine("== " + title + " ==");
    try {
        using var cmd = new SqlCommand(sql, conn);
        using var rd = cmd.ExecuteReader();
        while (rd.Read()) { for (int i = 0; i < rd.FieldCount; i++) Console.Write((rd.IsDBNull(i) ? "NULL" : rd[i]) + " | "); Console.WriteLine(); }
    } catch (Exception ex) { Console.WriteLine("ERR: " + ex.Message); }
}

Dump("CLASSES ALL", "SELECT Id, Name, InviteCode, Semester, Status, OwnerId, CreatedAt FROM [Classes] ORDER BY Id");
Dump("PATH STATUS DIST", "SELECT Status, Visibility, COUNT(*) FROM [LearningPaths] GROUP BY Status, Visibility");
Dump("PATHS 25", "SELECT Id, Title, Status, Visibility, IsActive, CreatedBy, AuthorId FROM [LearningPaths] ORDER BY Id");
Dump("SIMULATION KEYS DISTINCT", "SELECT SimulationKey, COUNT(*) FROM [LessonSimulations] GROUP BY SimulationKey ORDER BY SimulationKey");
Dump("USERS 77-84", "SELECT Id, Email, Role, DisplayName, IsActive, StaffCode, Department, AcademicDegree FROM [Users] WHERE Id >= 77 ORDER BY Id");
Dump("STUDENT DEMO STATE", "SELECT Hearts, HeartsMax, Gems, Xp, StreakDays, StreakFreeze, LastActivityDate, PremiumUntil FROM [Users] WHERE Email='student@demo.local'");
Dump("ROLES DIST", "SELECT Role, COUNT(*) FROM [Users] GROUP BY Role");
Dump("BUG STATUS", "SELECT Status, COUNT(*) FROM [BugReports] GROUP BY Status");
Dump("SHOP ITEMS", "SELECT Id, ItemKey, Name, PriceGems, Type FROM [ShopItems] ORDER BY Id");
Dump("INVENTORY STUDENT", "SELECT ui.ItemId, si.ItemKey, si.Name, ui.Quantity, ui.IsEquipped FROM [UserInventory] ui JOIN [ShopItems] si ON si.Id = ui.ItemId WHERE ui.UserId = 3");
Dump("USERQUESTS STUDENT TODAY", "SELECT TOP 5 uq.DailyQuestId, uq.Status, uq.ProgressJson, uq.CreatedAt, uq.ClaimedAt FROM [UserQuests] uq WHERE uq.UserId = 3 ORDER BY uq.Id DESC");
Dump("PATH PROGRESS STUDENT", "SELECT up.PathId, up.Status, up.ProgressPercent, up.CompletedNodes, up.TotalNodes, up.UpdatedAt FROM [UserProgress] up WHERE up.UserId = 3 ORDER BY up.PathId");