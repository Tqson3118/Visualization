#:package Microsoft.Data.SqlClient@6.0.2
#:property PublishAot=false
using Microsoft.Data.SqlClient;
var cs = "Server=db65198.public.databaseasp.net; Database=db65198; User Id=db65198; Password=4Rn+#6EoB!a8; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True; Connect Timeout=20;";
using var conn = new SqlConnection(cs);
conn.Open();
var sql = "SELECT rc.DELETE_RULE, OBJECT_NAME(fkc.parent_object_id) AS child, OBJECT_NAME(fkc.referenced_object_id) AS parent FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc JOIN sys.foreign_keys fk ON fk.name = rc.CONSTRAINT_NAME JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id ORDER BY 3, 2";
using (var cmd = new SqlCommand(sql, conn))
using (var rd = cmd.ExecuteReader())
    while (rd.Read()) Console.WriteLine(rd.GetString(1).PadRight(24) + " -> " + rd.GetString(2).PadRight(24) + " rule=" + rd.GetString(0));