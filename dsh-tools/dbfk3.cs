#:package Microsoft.Data.SqlClient@6.0.2
#:property PublishAot=false
using Microsoft.Data.SqlClient;
var cs = "Server=db65198.public.databaseasp.net; Database=db65198; User Id=db65198; Password=4Rn+#6EoB!a8; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True; Connect Timeout=20;";
using var conn = new SqlConnection(cs);
conn.Open();
var sql = "SELECT OBJECT_NAME(fk.parent_object_id) AS child, OBJECT_NAME(fk.referenced_object_id) AS parent, fk.delete_rule FROM sys.foreign_keys fk ORDER BY 2, 1";
using (var cmd = new SqlCommand(sql, conn))
using (var rd = cmd.ExecuteReader())
    while (rd.Read()) Console.WriteLine((rd.IsDBNull(0)?"?":rd.GetString(0)).PadRight(24) + " -> " + (rd.IsDBNull(1)?"?":rd.GetString(1)).PadRight(24) + " rule=" + rd.GetString(2));