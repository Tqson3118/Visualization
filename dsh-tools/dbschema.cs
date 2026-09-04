#:package Microsoft.Data.SqlClient@6.0.2
#:property PublishAot=false

using Microsoft.Data.SqlClient;

var cs = "Server=db65198.public.databaseasp.net; Database=db65198; User Id=db65198; Password=4Rn+#6EoB!a8; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True; Connect Timeout=20;";

using var conn = new SqlConnection(cs);
conn.Open();

void PrintSchema(string table) {
    Console.WriteLine("== COLUMNS of " + table + " ==");
    var sql = "SELECT c.name, t.name AS typ, c.max_length, c.is_nullable FROM sys.columns c JOIN sys.types t ON c.user_type_id = t.user_type_id WHERE c.object_id = OBJECT_ID('" + table + "') ORDER BY c.column_id";
    using var cmd = new SqlCommand(sql, conn);
    using var rd = cmd.ExecuteReader();
    while (rd.Read()) Console.WriteLine("  " + rd.GetString(0).PadRight(32) + rd.GetString(1).PadRight(16) + " len=" + rd[2] + " null=" + rd[3]);
}

foreach (var t in new[] { "Users", "Classes", "LearningPaths", "LearningPathNodes", "Lessons", "LessonSimulations", "Exercises", "BugReports", "ShopItems", "DailyQuests", "ClassAssignments", "CodeSubmissions", "UserInventory" })
    PrintSchema(t);

Console.WriteLine("== DEMO USERS ==");
using (var cmd = new SqlCommand("SELECT Id, Email, Role, DisplayName FROM [Users] ORDER BY Id", conn))
using (var rd = cmd.ExecuteReader())
    while (rd.Read()) Console.WriteLine("Id=" + rd[0].ToString().PadLeft(3) + " | " + rd[1] + " | " + rd[2] + " | " + (rd.IsDBNull(3) ? "" : rd[3]));

Console.WriteLine("== CLASSES ==");
using (var cmd = new SqlCommand("SELECT Id, ClassName, ClassCode, InviteCode, Status, OwnerId, CreatedAt FROM [Classes]", conn))
using (var rd = cmd.ExecuteReader())
    while (rd.Read()) Console.WriteLine("Id=" + rd[0] + " | " + rd[1] + " | " + rd[2] + " | " + rd[3] + " | " + rd[4] + " | owner=" + rd[5] + " | " + rd[6]);

Console.WriteLine("== LEARNING PATHS ==");
using (var cmd = new SqlCommand("SELECT Id, Title, Status, IsPublic, CreatedBy FROM [LearningPaths] ORDER BY Id", conn))
using (var rd = cmd.ExecuteReader())
    while (rd.Read()) Console.WriteLine("Id=" + rd[0].ToString().PadLeft(3) + " | " + rd[1] + " | " + rd[2] + " | pub=" + rd[3] + " | by=" + rd[4]);