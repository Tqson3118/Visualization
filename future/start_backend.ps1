param([switch]$KillOnly)
$webApi = Get-Process -Name WebApi -ErrorAction SilentlyContinue
if ($webApi) { $webApi | Stop-Process -Force; Start-Sleep -Seconds 2 }
if ($KillOnly) { Write-Host 'killed'; exit }
$root = 'D:\FPT\neww'
$projectRoot = Join-Path $root 'source\VisualizationDSA'
$logRoot = Join-Path $root 'future'
$env:ConnectionStrings__DefaultConnection = 'Server=localhost,1433;Database=VisualizationDSA;User Id=sa;Password=Dsa!2026Pass;TrustServerCertificate=True;'
$env:Jwt__Key = 'real-data-local-development-key-0123456789'
Start-Process dotnet -ArgumentList 'run','--project','backend\src\WebApi\WebApi.csproj','--urls','http://127.0.0.1:5055' -WorkingDirectory $projectRoot -WindowStyle Hidden -RedirectStandardOutput (Join-Path $logRoot 'be.out.log') -RedirectStandardError (Join-Path $logRoot 'be.err.log')
for ($i=1; $i -le 12; $i++) {
  Start-Sleep -Seconds 5
  try { $h = Invoke-WebRequest -Uri 'http://127.0.0.1:5055/health' -UseBasicParsing -TimeoutSec 3; if ($h.StatusCode -eq 200) { Write-Host 'HEALTH_OK'; exit } } catch {}
}
Write-Host 'HEALTH_TIMEOUT'
