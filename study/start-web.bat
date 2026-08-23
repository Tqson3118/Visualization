@echo off
title Visualization DSA - Study Web App
cd /d "%~dp0\web"
echo ========================================================
echo   KHOI CHAY STUDY WEB PORTAL (MOBILE ^& DESKTOP)
echo ========================================================
node build-data.js
node server.js
pause
