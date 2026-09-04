#:package Microsoft.Data.SqlClient@6.0.2
#:property PublishAot=false
using Microsoft.Data.SqlClient;
var cs = "Server=db65198.public.databaseasp.net; Database=db65198; User Id=db65198; Password=4Rn+#6EoB!a8; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True; Connect Timeout=20;";
using var conn = new SqlConnection(cs);
conn.Open();
using (var cmd = new SqlCommand("SELECT COUNT(*) FROM [Lessons] l WHERE NOT EXISTS (SELECT 1 FROM [LearningPathNodes] n WHERE n.LessonId = l.Id)", conn))
    Console.WriteLine("ORPHAN LESSONS (khong thuoc node nao): " + cmd.ExecuteScalar());
using (var cmd = new SqlCommand("SELECT COUNT(*) FROM [Exercises] e WHERE NOT EXISTS (SELECT 1 FROM [LearningPathNodes] n WHERE n.LabExerciseId = e.Id) AND NOT EXISTS (SELECT 1 FROM [ClassAssignments] a WHERE a.ExerciseId = e.Id)", conn))
    Console.WriteLine("ORPHAN EXERCISES: " + cmd.ExecuteScalar());
using (var cmd = new SqlCommand("SELECT COUNT(*) FROM [UserNodeProgress] unp WHERE NOT EXISTS (SELECT 1 FROM [LearningPathNodes] n WHERE n.Id = unp.NodeId)", conn))
    Console.WriteLine("ORPHAN NODE PROGRESS: " + cmd.ExecuteScalar());
using (var cmd = new SqlCommand("SELECT COUNT(*) FROM [ExerciseSubmissions] es WHERE NOT EXISTS (SELECT 1 FROM [ClassAssignments] a WHERE a.Id = es.ClassAssignmentId)", conn))
    Console.WriteLine("ORPHAN EX SUBMISSIONS: " + cmd.ExecuteScalar());
using (var cmd = new SqlCommand("SELECT COUNT(*) FROM [RefreshTokens] WHERE ExpiresAt < SYSUTCDATETIME()", conn))
    Console.WriteLine("EXPIRED REFRESH TOKENS: " + cmd.ExecuteScalar());
using (var cmd = new SqlCommand("SELECT TopicId, COUNT(*) FROM [LearningPaths] GROUP BY TopicId", conn))
using (var rd = cmd.ExecuteReader()) while (rd.Read()) Console.WriteLine("Topic " + (rd.IsDBNull(0)?"NULL":rd[0]) + " = " + rd[1] + " paths");