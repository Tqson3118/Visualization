# PowerShell launcher for Study Web Portal
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location "$scriptDir\web"

Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "  KHOI CHAY STUDY WEB PORTAL (MOBILE & DESKTOP)" -ForegroundColor Green
Write-Host "========================================================" -ForegroundColor Cyan

node build-data.js
node server.js
