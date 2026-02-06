@echo off
cd /d "%~dp0"
start /B dotnet run --configuration Release
