#:package Microsoft.Data.SqlClient@6.0.2
#:property PublishAot=false
using Microsoft.Data.SqlClient;
var cs = "Server=db65198.public.databaseasp.net; Database=db65198; User Id=db65198; Password=4Rn+#6EoB!a8; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True; Connect Timeout=20;";
using var conn = new SqlConnection(cs);
conn.Open();
Console.WriteLine("== FKs (delete_rule) ==");
var sql = "SELECT fk.name AS fk_name, tp.name AS child, par.name AS parent, fk.delete_rule FROM sys.foreign_keys fk JOIN sys.tables tp ON tp.object_id = fk.parent_object_id JOIN sys.tables par ON par.object_id = fk.referenced_object_id ORDER BY par.name, tp.name";
using (var cmd = new SqlCommand(sql, conn))
using (var rd = cmd.ExecuteReader())
    while (rd.Read()) Console.WriteLine(rd.GetString(1).PadRight(24) + " -> " + rd.GetString(2).PadRight(24) + " rule=" + rd.GetString(3));