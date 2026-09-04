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

Dump("COLUMNS UserQuests", "SELECT c.name FROM sys.columns c WHERE c.object_id = OBJECT_ID('UserQuests') ORDER BY c.column_id");
Dump("COLUMNS UserProgress", "SELECT c.name FROM sys.columns c WHERE c.object_id = OBJECT_ID('UserProgress') ORDER BY c.column_id");
Dump("COLUMNS NodeSessions", "SELECT c.name FROM sys.columns c WHERE c.object_id = OBJECT_ID('NodeSessions') ORDER BY c.column_id");

Dump("PATH1 NODES", "SELECT Id, Title, ItemType, LessonId, FinalTestId, LabExerciseId, ParentId, SortOrder FROM [LearningPathNodes] WHERE PathId = 1 ORDER BY SortOrder");
Dump("PATH4 NODES", "SELECT Id, Title, ItemType, LessonId, FinalTestId, LabExerciseId, ParentId, SortOrder FROM [LearningPathNodes] WHERE PathId = 20 ORDER BY SortOrder");
Dump("PATH BST34 NODES", "SELECT Id, Title, ItemType, LessonId, FinalTestId, LabExerciseId, ParentId, SortOrder FROM [LearningPathNodes] WHERE PathId = 34 ORDER BY SortOrder");

Dump("STUDENT PROGRESS PATHS", "SELECT * FROM [UserProgress] WHERE UserId = 3");
Dump("BUGREPORTS ALL", "SELECT Id, Status, LEFT(Description, 60), AssigneeId, ResolvedAt FROM [BugReports] ORDER BY Id");
Dump("TOP XP USERS", "SELECT TOP 6 Id, DisplayName, Xp, StreakDays FROM [Users] ORDER BY Xp DESC");
Dump("CLASSMEMBER COUNT BY CLASS", "SELECT ClassId, COUNT(*) FROM [ClassMembers] WHERE DeletedAt IS NULL GROUP BY ClassId");