# reset-db.ps1 — Reset DB DsaVisual về seed sạch (mục tiêu < 30 giây) — Trục 17 ROUND 2
# Yêu cầu: docker sqlserver (neww-sqlserver-1) đang chạy + backend đã build sẵn (bin/ tồn tại).
# Sau khi chạy: DB được migrate + seed lại từ đầu (80 users, 8 lessons, 29 exercises, 3 classes — Seed V2).
# ⚠️ Chạy khi KHÔNG có ai đang thao tác trên app (drop DB sẽ ngắt kết nối đang mở).
# Cách dùng: powershell -ExecutionPolicy Bypass -File frontend/scripts/reset-db.ps1
$ErrorActionPreference = 'Stop'

$saPass = if ($env:MSSQL_SA_PASSWORD) { $env:MSSQL_SA_PASSWORD } else { 'DsaVisual@Dev123' }
$env:DSA__Jwt__Secret = if ($env:DSA__Jwt__Secret) { $env:DSA__Jwt__Secret } else { 'dev-secret-32-ky-tu-toi-thieu-0123456789abcdef' }
$env:ConnectionStrings__Default = "Server=localhost;Database=DsaVisual;User Id=sa;Password=$saPass;TrustServerCertificate=True"

$sw = [System.Diagnostics.Stopwatch]::StartNew()
Write-Host '[1/2] Drop database DsaVisual...'
docker exec neww-sqlserver-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $saPass -C -Q "IF DB_ID('DsaVisual') IS NOT NULL BEGIN ALTER DATABASE [DsaVisual] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [DsaVisual]; END"
if ($LASTEXITCODE -ne 0) { throw 'DROP FAILED' }

Write-Host '[2/2] Migrate + seed (DLL trực tiếp — nhanh hơn dotnet run)...'
Push-Location 'D:\FPT\neww\backend\src\DsaVisual.Api'
try {
  dotnet 'D:\FPT\neww\backend\src\DsaVisual.Api\bin\Debug\net10.0\DsaVisual.Api.dll' --seed 2>&1 | Select-Object -Last 8
  if ($LASTEXITCODE -ne 0) { throw ("SEED FAILED exit=" + $LASTEXITCODE) }
} finally { Pop-Location }

$sw.Stop()
Write-Output ("RESET OK in " + [Math]::Round($sw.Elapsed.TotalSeconds, 1) + "s")
