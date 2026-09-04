#:package Microsoft.Data.SqlClient@6.0.2
#:property PublishAot=false
using Microsoft.Data.SqlClient;
var cs = "Server=db65198.public.databaseasp.net; Database=db65198; User Id=db65198; Password=4Rn+#6EoB!a8; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True; Connect Timeout=20;";
using var conn = new SqlConnection(cs);
conn.Open();
Console.WriteLine("== ALL TABLES + COUNTS ==");
var sb = new System.Text.StringBuilder();
using (var cmd = new SqlCommand("SELECT t.name, SUM(p.rows) FROM sys.tables t LEFT JOIN sys.partitions p ON p.object_id=t.object_id AND p.index_id IN (0,1) GROUP BY t.name ORDER BY t.name", conn))
using (var rd = cmd.ExecuteReader()) { int n = 0; while (rd.Read()) { n++; sb.AppendLine(rd.GetString(0) + " = " + (rd.IsDBNull(1) ? 0 : Convert.ToInt64(rd.GetValue(1)))); } sb.AppendLine("TOTAL = " + n); }
Console.WriteLine(sb.ToString());
Console.WriteLine("== PENDING TEACHERS ==");
using (var cmd = new SqlCommand("SELECT Id, Email, DisplayName, StaffCode, Department, AcademicDegree FROM [Users] WHERE Role = 2", conn))
using (var rd = cmd.ExecuteReader()) while (rd.Read()) Console.WriteLine(rd[0] + " | " + rd[1] + " | " + rd[2] + " | staff=" + (rd.IsDBNull(3) ? "NULL" : rd[3]) + " | dept=" + (rd.IsDBNull(4) ? "NULL" : rd[4]) + " | deg=" + (rd.IsDBNull(5) ? "NULL" : rd[5]));
Console.WriteLine("== LESSON STATUS DIST ==");
using (var cmd = new SqlCommand("SELECT Status, COUNT(*) FROM [Lessons] GROUP BY Status", conn))
using (var rd = cmd.ExecuteReader()) while (rd.Read()) Console.WriteLine("Status " + rd[0] + " = " + rd[1]);
Console.WriteLine("== EXERCISE TYPES ==");
using (var cmd = new SqlCommand("SELECT Type, COUNT(*) FROM [Exercises] GROUP BY Type", conn))
using (var rd = cmd.ExecuteReader()) while (rd.Read()) Console.WriteLine("Type " + rd[0] + " = " + rd[1]);
Console.WriteLine("== SIMKEY COUNT CHECK ==");
using (var cmd = new SqlCommand("SELECT COUNT(*) FROM [LessonSimulations]", conn))
Console.WriteLine("LessonSimulations = " + cmd.ExecuteScalar());
using (var cmd = new SqlCommand("SELECT COUNT(*) FROM [RefreshTokens]", conn))
Console.WriteLine("RefreshTokens = " + cmd.ExecuteScalar());
using (var cmd = new SqlCommand("SELECT COUNT(*) FROM [NodeSessions]", conn))
Console.WriteLine("NodeSessions = " + cmd.ExecuteScalar());