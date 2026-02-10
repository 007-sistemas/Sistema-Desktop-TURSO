#!/usr/bin/env pwsh
# Script para compilar o Sistema Biometrico

# Adicionar dotnet ao PATH se não estiver
$dotnetPath = "C:\Program Files\dotnet"
if ($env:PATH -notlike "*$dotnetPath*") {
    $env:PATH += ";$dotnetPath"
}

Push-Location "c:\Users\Gabriel Gomes\Documents\Sistema Desktop"
Write-Host "Compilando projeto em Release..." -ForegroundColor Cyan
& dotnet build -c Release

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nCompilacao concluida com sucesso!" -ForegroundColor Green
    Write-Host "Agora execute Iniciar.bat ou clique no .exe" -ForegroundColor Green
} else {
    Write-Host "`nErro na compilacao!" -ForegroundColor Red
}

Pop-Location
