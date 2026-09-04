#:package Microsoft.Data.SqlClient@6.0.2
#:property PublishAot=false
using Microsoft.Data.SqlClient;
var cs = "Server=db65198.public.databaseasp.net; Database=db65198; User Id=db65198; Password=4Rn+#6EoB!a8; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True; Connect Timeout=20;";
using var conn = new SqlConnection(cs);
conn.Open();
using (var cmd = new SqlCommand("SELECT COUNT(*) FROM sys.foreign_keys", conn))
    Console.WriteLine("FK COUNT: " + cmd.ExecuteScalar());
using (var cmd = new SqlCommand("SELECT COUNT(*) FROM sys.indexes WHERE is_primary_key = 1", conn))
    Console.WriteLine("PK COUNT: " + cmd.ExecuteScalar());
using (var cmd = new SqlCommand("SELECT COUNT(*) FROM sys.tables", conn))
    Console.WriteLine("TABLE COUNT: " + cmd.ExecuteScalar());
using (var cmd = new SqlCommand("SELECT name FROM sys.databases WHERE database_id = DB_ID()", conn))
    Console.WriteLine("DB: " + cmd.ExecuteScalar());