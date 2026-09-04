#:package Microsoft.Data.SqlClient@6.0.2
#:property PublishAot=false

using Microsoft.Data.SqlClient;

var cs = "Server=db65198.public.databaseasp.net; Database=db65198; User Id=db65198; Password=4Rn+#6EoB!a8; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True; Connect Timeout=20;";

using var conn = new SqlConnection(cs);
conn.Open();
Console.WriteLine("CONNECTED: " + conn.Database);

// 1) All tables + row counts
Console.WriteLine("\n=== TABLES + ROWCOUNT ===");
var tableCounts = new List<(string name, long rows)>();
using (var cmd = new SqlCommand(@"
SELECT t.name, SUM(p.rows) AS rowcnt
FROM sys.tables t
JOIN sys.schemas s ON t.schema_id = s.schema_id
LEFT JOIN sys.partitions p ON p.object_id = t.object_id AND p.index_id IN (0,1)
GROUP BY t.name
ORDER BY t.name", conn))
using (var rd = await cmd.ExecuteReaderAsync()) {
    while (await rd.ReadAsync()) {
        tableCounts.Add((rd.GetString(0), rd.IsDBNull(1) ? 0 : Convert.ToInt64(rd.GetValue(1))));
    }
}
int total = 0;
foreach (var (n, r) in tableCounts) { Console.WriteLine($"{n,-45} {r}"); total++; }
Console.WriteLine($"TOTAL TABLES: {total}");

// 2) users
Console.WriteLine("\n=== USERS (top 40) ===");
using (var cmd = new SqlCommand("SELECT TOP 40 Id, Email, Role, FullName, Status FROM [Users] ORDER BY Id", conn))
using (var rd = await cmd.ExecuteReaderAsync()) {
    while (await rd.ReadAsync()) Console.WriteLine($"Id={rd[0]} | {rd[1]} | {rd[2]} | {rd[3]} | {rd[4]}");
}
