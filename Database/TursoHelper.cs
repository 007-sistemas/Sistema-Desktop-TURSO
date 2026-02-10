using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using BiometricSystem.Models;
using System.Data.SQLite;

namespace BiometricSystem.Database
{
    /// <summary>
    /// Gerencia sincronizacao de dados com Turso (libSQL/SQLite Cloud)
    /// Substitui NeonHelper.cs da arquitetura anterior
    /// </summary>
    public class TursoHelper
    {
        private readonly TursoDbConnection _tursoConnection;
        private readonly DatabaseHelper _localDb;
        private Dictionary<string, (string Name, string Type)>? _pontosColumnsCache;
        private DateTime _pontosColumnsCacheAt = DateTime.MinValue;
        private static readonly TimeSpan PontosColumnsCacheTtl = TimeSpan.FromMinutes(10);
        private static readonly object LogLock = new object();

        public TursoHelper(string tursoUrl, string authToken, DatabaseHelper localDb)
        {
            _tursoConnection = new TursoDbConnection(tursoUrl, authToken);
            _localDb = localDb;
        }

        /// <summary>
        /// Insere um registro de ponto (entrada/saida) no Turso
        /// </summary>
        public async Task<bool> InsertTimeRecordAsync(TimeRecord record)
        {
            try
            {
                var sql = @"
                    INSERT INTO pontos (
                        id, codigo, cooperado_id, cooperado_nome, 
                        timestamp, tipo, local, hospital_id, setor_id, 
                        status, biometria_entrada_hash, biometria_saida_hash,
                        validado_por, rejeitado_por, motivo_rejeicao, criado_em
                    ) VALUES (
                        @id, @codigo, @cooperado_id, @cooperado_nome,
                        @timestamp, @tipo, @local, @hospital_id, @setor_id,
                        @status, @biometria_entrada_hash, @biometria_saida_hash,
                        @validado_por, @rejeitado_por, @motivo_rejeicao, @criado_em
                    )";

                var parameters = new object[]
                {
                    Guid.NewGuid().ToString(), // id
                    "", // codigo
                    record.EmployeeId.ToString(), // cooperado_id
                    "", // cooperado_nome
                    record.Timestamp.ToString("O"), // ISO8601 format
                    record.Type, // tipo: ENTRADA, SAIDA
                    "", // local
                    "", // hospital_id
                    "", // setor_id
                    "Pendente", // status
                    "", // biometria_entrada_hash
                    "", // biometria_saida_hash
                    null, // validado_por
                    null, // rejeitado_por
                    null, // motivo_rejeicao
                    DateTime.UtcNow.ToString("O") // criado_em
                };

                await _tursoConnection.ExecuteNonQueryAsync(sql, parameters);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao inserir registro em Turso: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sincroniza registros pendentes do SQLite local com Turso
        /// </summary>
        public async Task<int> SyncPendingRecordsAsync()
        {
            try
            {
                // Busca registros locais nao sincronizados (Pontos)
                var pendingPontos = _localDb.BuscarPontosNaoSincronizados();
                int syncedCount = 0;

                System.Diagnostics.Debug.WriteLine($"[TursoHelper] Pontos pendentes: {pendingPontos.Count}");
                LogToFile($"[TursoHelper] Pontos pendentes: {pendingPontos.Count}");

                if (pendingPontos.Count == 0)
                {
                    LogToFile("[TursoHelper] Nenhum ponto pendente para sincronizar");
                }

                foreach (var ponto in pendingPontos)
                {
                    bool success = await InsertPontoAsync(ponto);
                    if (success)
                    {
                        _localDb.MarcaPontoComoSincronizado(ponto.Id);
                        syncedCount++;
                        LogToFile($"[TursoHelper] Ponto sincronizado: {ponto.Id} ({ponto.CooperadoId})");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[TursoHelper] Falha ao sincronizar ponto {ponto.Id}");
                        LogToFile($"[TursoHelper] Falha ao sincronizar ponto {ponto.Id} ({ponto.CooperadoId})");
                    }
                }

                return syncedCount;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao sincronizar registros: {ex.Message}");
                LogToFile($"[TursoHelper] Erro ao sincronizar registros: {ex.Message}");
                return 0;
            }
        }

        private async Task<bool> InsertPontoAsync(DatabaseHelper.PontoLocal ponto)
        {
            try
            {
                var columns = await GetPontosColumnsAsync();
                if (columns.Count > 0)
                {
                    var insertColumns = new List<string>();
                    var insertValues = new List<object>();

                    void AddIfExists(string columnKey, object value)
                    {
                        if (columns.TryGetValue(columnKey, out var col))
                        {
                            insertColumns.Add(col.Name);
                            insertValues.Add(value);
                        }
                    }

                    AddIfExists("id", string.IsNullOrEmpty(ponto.Id) ? Guid.NewGuid().ToString() : ponto.Id);
                    AddIfExists("codigo", ponto.Codigo ?? string.Empty);
                    AddIfExists("cooperado_id", ponto.CooperadoId ?? string.Empty);
                    AddIfExists("cooperado_nome", ponto.CooperadoNome ?? string.Empty);
                    AddIfExists("timestamp", ponto.Timestamp.ToString("O"));
                    AddIfExists("tipo", ponto.Tipo ?? string.Empty);
                    AddIfExists("local", ponto.Local ?? string.Empty);
                    AddIfExists("hospital_id", ponto.HospitalId ?? (object)DBNull.Value);
                    AddIfExists("setor_id", ponto.SetorId ?? (object)DBNull.Value);
                    AddIfExists("status", ponto.Status ?? "Pendente");
                    AddIfExists("is_manual", ponto.IsManual ? 1 : 0);
                    AddIfExists("related_id", ponto.RelatedId ?? (object)DBNull.Value);
                    AddIfExists("biometria_entrada_hash", ponto.BiometriaEntradaHash ?? (object)DBNull.Value);
                    AddIfExists("biometria_saida_hash", ponto.BiometriaSaidaHash ?? (object)DBNull.Value);
                    AddIfExists("observacao", ponto.Observacao ?? (object)DBNull.Value);

                    var dateValue = ponto.Date ?? ponto.Timestamp.ToString("yyyy-MM-dd");
                    AddIfExists("date", dateValue);
                    AddIfExists("entrada", ponto.Entrada ?? (object)DBNull.Value);
                    AddIfExists("saida", ponto.Saida ?? (object)DBNull.Value);

                    AddIfExists("created_at", ponto.Timestamp.ToString("O"));
                    AddIfExists("created_at_db", ponto.Timestamp.ToString("O"));
                    AddIfExists("criado_em", ponto.Timestamp.ToString("O"));
                    AddIfExists("sincronizado_em", ponto.Timestamp.ToString("O"));

                    if (insertColumns.Count > 0)
                    {
                        var placeholders = string.Join(", ", insertColumns.Select(_ => "?"));
                        var sqlDynamic = $"INSERT INTO pontos ({string.Join(", ", insertColumns)}) VALUES ({placeholders})";
                        var affected = await _tursoConnection.ExecuteNonQueryAsync(sqlDynamic, insertValues.ToArray());
                        if (affected > 0)
                            return true;

                        System.Diagnostics.Debug.WriteLine("[TursoHelper] Insert dinamico retornou 0 linhas afetadas");
                        LogToFile("[TursoHelper] Insert dinamico retornou 0 linhas afetadas");
                        return false;
                    }
                }

                var sql = @"
                    INSERT INTO pontos (
                        id, codigo, cooperado_id, cooperado_nome, timestamp, tipo,
                        local, hospital_id, setor_id, status, is_manual, related_id,
                        biometria_entrada_hash, biometria_saida_hash, validado_por,
                        rejeitado_por, motivo_rejeicao, observacao
                    ) VALUES (
                        ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?
                    )";

                var parameters = new object[]
                {
                    string.IsNullOrEmpty(ponto.Id) ? Guid.NewGuid().ToString() : ponto.Id,
                    ponto.Codigo ?? string.Empty,
                    ponto.CooperadoId ?? string.Empty,
                    ponto.CooperadoNome ?? string.Empty,
                    ponto.Timestamp.ToString("O"),
                    ponto.Tipo ?? string.Empty,
                    ponto.Local ?? string.Empty,
                    ponto.HospitalId ?? (object)DBNull.Value,
                    ponto.SetorId ?? (object)DBNull.Value,
                    ponto.Status ?? "Pendente",
                    ponto.IsManual ? 1 : 0,
                    ponto.RelatedId ?? (object)DBNull.Value,
                    ponto.BiometriaEntradaHash ?? (object)DBNull.Value,
                    ponto.BiometriaSaidaHash ?? (object)DBNull.Value,
                    DBNull.Value,
                    DBNull.Value,
                    DBNull.Value,
                    ponto.Observacao ?? (object)DBNull.Value
                };

                var affectedFallback = await _tursoConnection.ExecuteNonQueryAsync(sql, parameters);
                if (affectedFallback > 0)
                    return true;

                System.Diagnostics.Debug.WriteLine("[TursoHelper] Insert fallback retornou 0 linhas afetadas");
                LogToFile("[TursoHelper] Insert fallback retornou 0 linhas afetadas");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao inserir ponto em Turso: {ex.Message}");
                LogToFile($"[TursoHelper] Erro ao inserir ponto em Turso: {ex.Message}");
                return false;
            }
        }

        private static void LogToFile(string message)
        {
            try
            {
                var logRoot = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                    ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                    ?? "C:\\Temp";
                if (string.IsNullOrEmpty(logRoot))
                    logRoot = "C:\\Temp";

                var logDir = Path.Combine(logRoot, "BiometricSystem");
                Directory.CreateDirectory(logDir);
                var logPath = Path.Combine(logDir, "biometric_log.txt");
                var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";

                lock (LogLock)
                {
                    File.AppendAllText(logPath, logMessage + Environment.NewLine);
                }
            }
            catch
            {
                // Ignorar falhas de log
            }
        }

        private async Task<Dictionary<string, (string Name, string Type)>> GetPontosColumnsAsync()
        {
            if (_pontosColumnsCache != null && (DateTime.UtcNow - _pontosColumnsCacheAt) < PontosColumnsCacheTtl)
                return _pontosColumnsCache;

            var columns = new Dictionary<string, (string Name, string Type)>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var rows = await _tursoConnection.ExecuteQueryAsync("PRAGMA table_info(pontos)");
                foreach (var row in rows)
                {
                    var name = row.TryGetValue("name", out var val) ? val?.ToString() : null;
                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    var type = row.TryGetValue("type", out var tVal) ? tVal?.ToString() ?? string.Empty : string.Empty;
                    columns[name.ToLowerInvariant()] = (name, type);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TursoHelper] Erro ao ler schema de pontos: {ex.Message}");
            }

            _pontosColumnsCache = columns;
            _pontosColumnsCacheAt = DateTime.UtcNow;
            return columns;
        }

        /// <summary>
        /// Insere biometria no Turso
        /// </summary>
        public async Task<bool> InsertBiometryAsync(Employee employee)
        {
            try
            {
                var sql = @"
                    INSERT INTO biometrias (
                        id, cooperado_id, cooperado_nome, template_bytes,
                        tipo_impressao, criado_em, sincronizado_em
                    ) VALUES (
                        @id, @cooperado_id, @cooperado_nome, @template_bytes,
                        @tipo_impressao, @criado_em, @sincronizado_em
                    )";

                var parameters = new object[]
                {
                    Guid.NewGuid().ToString(),
                    employee.Id.ToString(),
                    employee.Name,
                    Convert.ToBase64String(employee.FingerprintTemplate ?? new byte[0]),
                    "Todos",
                    DateTime.UtcNow.ToString("O"),
                    DateTime.UtcNow.ToString("O")
                };

                await _tursoConnection.ExecuteNonQueryAsync(sql, parameters);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao inserir biometria em Turso: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Busca funcionarios do Turso
        /// </summary>
        public async Task<List<Employee>> GetEmployeesAsync()
        {
            try
            {
                var sql = "SELECT id, cooperado_nome as name, cooperado_id FROM biometrias LIMIT 1000";
                var results = await _tursoConnection.ExecuteQueryAsync(sql);

                var employees = new List<Employee>();
                foreach (var row in results)
                {
                    employees.Add(new Employee
                    {
                        Id = int.Parse(row["cooperado_id"].ToString()),
                        Name = row["name"].ToString()
                    });
                }

                return employees;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao buscar funcionarios: {ex.Message}");
                return new List<Employee>();
            }
        }

        /// <summary>
        /// Testa conexao com Turso
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            return await _tursoConnection.TestConnectionAsync();
        }
    }
}
