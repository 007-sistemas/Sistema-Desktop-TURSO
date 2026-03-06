#!/usr/bin/env powershell
# Script para visualizar dados das biometrias armazenadas

$dbPath = "bin\Release\net8.0-windows\win-x64\biometric.db"

if (-not (Test-Path $dbPath)) {
    Write-Host "❌ Banco de dados não encontrado em: $dbPath" -ForegroundColor Red
    exit 1
}

Write-Host "📊 DADOS ARMAZENADOS NO SISTEMA BIOMÉTRICO" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Instalar SQLiteHelper se necessário
$assembly = [System.Reflection.Assembly]::LoadFrom("$PSScriptRoot\System.Data.SQLite.dll")

# Criar conexão
$connectionString = "Data Source=$dbPath;Version=3;"
$connection = New-Object System.Data.SQLite.SQLiteConnection $connectionString

try {
    $connection.Open()
    
    # 1. Biometrias cadastradas
    Write-Host "1️⃣  BIOMETRIAS CADASTRADAS:" -ForegroundColor Green
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = "SELECT COUNT(*) FROM Biometrias"
    $countBiometrias = $cmd.ExecuteScalar()
    
    $cmd.CommandText = "SELECT Id, CooperadoNome, CreatedAt, SyncedToTurso FROM Biometrias ORDER BY CreatedAt DESC"
    $reader = $cmd.ExecuteReader()
    
    if ($reader.HasRows) {
        $index = 1
        while ($reader.Read()) {
            $syncStatus = if ($reader.GetInt32(3) -eq 1) { "✅ SINCRONIZADO" } else { "⏳ Pendente" }
            Write-Host "  [$index] Cooperado: $($reader.GetString(1))"
            Write-Host "       Data: $($reader.GetString(2))"
            Write-Host "       Status: $syncStatus"
            $index++
        }
    } else {
        Write-Host "  ❌ Nenhuma biometria cadastrada"
    }
    
    Write-Host ""
    Write-Host "2️⃣  REGISTROS DE PRODUÇÃO (Entrada/Saída):" -ForegroundColor Green
    
    $cmd.CommandText = "SELECT COUNT(*) FROM Pontos"
    $countPontos = $cmd.ExecuteScalar()
    
    $cmd.CommandText = "SELECT Id, CooperadoNome, Tipo, Timestamp, Local, SyncedToTurso FROM Pontos ORDER BY Timestamp DESC LIMIT 10"
    $reader = $cmd.ExecuteReader()
    
    if ($reader.HasRows) {
        $index = 1
        while ($reader.Read()) {
            $syncStatus = if ($reader.GetInt32(5) -eq 1) { "✅" } else { "⏳" }
            Write-Host "  [$index] $($reader.GetString(1)) - $($reader.GetString(2)) $syncStatus"
            Write-Host "       Horário: $($reader.GetString(3))"
            Write-Host "       Local: $($reader.GetString(4))"
            $index++
        }
    } else {
        Write-Host "  ❌ Nenhum registro de produção"
    }
    
    Write-Host ""
    Write-Host "3️⃣  RESUMO:" -ForegroundColor Green
    Write-Host "  Total de biometrias: $countBiometrias" -ForegroundColor Yellow
    Write-Host "  Total de registros de produção: $countPontos" -ForegroundColor Yellow
    
}
catch {
    Write-Host "❌ Erro ao ler banco de dados: $_" -ForegroundColor Red
}
finally {
    $connection.Close()
}

Write-Host ""
Write-Host "📁 Caminho completo do banco:" -ForegroundColor Cyan
Write-Host "   $((Resolve-Path $dbPath).Path)" -ForegroundColor Yellow
