@echo off
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\scripts\integrate-entrypoints.ps1" -Action Restore -AppRoot "."
pause
