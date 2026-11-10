@echo off
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "$desktop = [Environment]::GetFolderPath('Desktop'); & '.\scripts\integrate-entrypoints.ps1' -Action Install -AppRoot '.' -SearchRoots @($desktop) -NoCreateDefaultLinks"
pause
