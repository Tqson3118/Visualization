#:package Microsoft.Data.SqlClient@6.0.2
#:property PublishAot=false
using Microsoft.Data.SqlClient;
var cs = "Server=db65198.public.databaseasp.net; Database=db65198; User Id=db65198; Password=4Rn+#6EoB!a8; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True; Connect Timeout=20;";
using var conn = new SqlConnection(cs);
conn.Open();
var sql = "SELECT c.name FROM sys.columns c WHERE c.object_id = OBJECT_ID('sys.foreign_keys')";
using (var cmd = new SqlCommand(sql, conn))
using (var rd = cmd.ExecuteReader())
    while (rd.Read()) Console.WriteLine(rd.GetString(0));