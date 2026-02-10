@echo off
REM Script para compilar o Sistema Biometrico
cd /d "%~dp0"
echo Compilando projeto em Release...
dotnet build -c Release
if %errorlevel% neq 0 (
    echo.
    echo Erro na compilacao!
    pause
    exit /b 1
)
echo.
echo Compilacao concluida com sucesso!
pause
