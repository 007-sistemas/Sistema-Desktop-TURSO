@echo off
REM Limpa o banco de dados local (exclui arquivo SQLite)
set DB_PATH="%~dp0Database\biometria.db"
if exist %DB_PATH% del /f /q %DB_PATH%
exit /b 0
