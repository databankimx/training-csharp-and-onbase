@echo off
REM Simple batch wrapper for the PowerShell sync script
REM Can be run from command line or integrated into VS as external tool

REM Get the directory where this script is located
setlocal enabledelayedexpansion
set scriptDir=%~dp0

REM Run PowerShell script
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%scriptDir%Sync-wwwroot.ps1" %*

pause
