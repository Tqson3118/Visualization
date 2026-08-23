# PowerShell Start Script for Study Web Portal
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "  KHOI CHAY STUDY WEB PORTAL (MOBILE & DESKTOP)" -ForegroundColor Yellow
Write-Host "========================================================" -ForegroundColor Cyan

Set-Location -Path "$PSScriptRoot\web"
node server.js
