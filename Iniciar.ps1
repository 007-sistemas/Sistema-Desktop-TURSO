#!/usr/bin/env pwsh
# Script para iniciar o Sistema Biométrico
# Executa o .exe Release com suporte para RollForward

$exePath = "bin\Release\net8.0-windows\win-x64\BiometricSystem.exe"
$workingDir = Get-Location

# Verifica se o executável existe
if (-Not (Test-Path $exePath)) {
    Write-Host "Erro: Executável não encontrado em $exePath" -ForegroundColor Red
    Write-Host "Compilando projeto..."
    dotnet build -c Release
}

# Executa o .exe
Write-Host "Iniciando Sistema Biométrico..." -ForegroundColor Green
& $exePath
