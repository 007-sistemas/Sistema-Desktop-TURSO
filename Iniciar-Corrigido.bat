@echo off
REM Iniciar Sistema Biometrico com suporte a RollForward

REM Obter diretorio do script (pasta Release)
for %%I in ("%~dp0.") do set "RELEASE_DIR=%%~fI"
for %%I in ("%RELEASE_DIR%..") do set "PROJECT_DIR=%%~fI"

REM Mudar para pasta do projeto
cd /d "%PROJECT_DIR%"

REM Executar via dotnet run (ignora o .runtimeconfig.json e usa o RollForward do .csproj)
echo Iniciando Sistema Biometrico...
start /B "BiometricSystem" dotnet run --configuration Release --no-build

REM Aguardar um pouco para a janela abrir
timeout /t 2 /nobreak

exit /b 0
