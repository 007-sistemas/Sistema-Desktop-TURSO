using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BiometricSystem.Models;

namespace BiometricSystem.Database
{
    public class TursoCooperadoHelper
    {
        private readonly TursoDbConnection _tursoConnection;
        private Dictionary<string, (string Name, string Type)>? _biometriasColumnsCache;
        private DateTime _biometriasColumnsCacheAt = DateTime.MinValue;
        private static readonly TimeSpan BiometriasColumnsCacheTtl = TimeSpan.FromMinutes(10);

        public TursoCooperadoHelper(string tursoUrl, string authToken)
        {
            _tursoConnection = new TursoDbConnection(tursoUrl, authToken);
        }

        public class Cooperado
        {
            public string Id { get; set; } = string.Empty;
            public string Nome { get; set; } = string.Empty;
            public string? Cpf { get; set; }
            public string? Email { get; set; }
            public string? Telefone { get; set; }
            public DateTime? CriadoEm { get; set; }
            public bool Ativo { get; set; } = true;
            public string? ProducaoPorCpf { get; set; }

            public override string ToString() => $"{Nome}";
        }

        public class Hospital
        {
            public string Id { get; set; } = string.Empty;
            public string Nome { get; set; } = string.Empty;
            public string Codigo { get; set; } = string.Empty;

            public override string ToString() => $"{Codigo} - {Nome}";
        }

        public class SetorInfo
        {
            public int Id { get; set; }
            public string Nome { get; set; } = string.Empty;

            public override string ToString() => Nome;
        }

        public class RegistroPonto
        {
            public string Id { get; set; } = string.Empty;
            public string Codigo { get; set; } = string.Empty;
            public string CooperadoId { get; set; } = string.Empty;
            public string CooperadoNome { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; }
            public string Tipo { get; set; } = string.Empty;
            public string? Entrada { get; set; }
            public string? Saida { get; set; }
            public string? HospitalId { get; set; }
            public string? SetorId { get; set; }
            public string? BiometriaEntradaHash { get; set; }
            public string? BiometriaSaidaHash { get; set; }
            public string? RelatedId { get; set; }
            public string? Status { get; set; }
            public bool IsManual { get; set; }
            public string? Local { get; set; }
            public string? ValidadoPor { get; set; }
            public string? RejeitadoPor { get; set; }
            public string? MotivoRejeicao { get; set; }
            public string? Observacao { get; set; }
        }

        public async Task<bool> TestConnectionAsync()
        {
            return await _tursoConnection.TestConnectionAsync();
        }

        public async Task<List<Cooperado>> GetCooperadosAsync()
        {
            try
            {
                // Tenta com colunas específicas usando os nomes CORRETOS da tabela
                var sql = "SELECT id, name, cpf, email, phone, created_at, status, producao_por_cpf FROM cooperados WHERE status != 'inactive' ORDER BY name";
                var results = await _tursoConnection.ExecuteQueryAsync(sql);

                if (results != null && results.Count > 0)
                {
                    var cooperados = new List<Cooperado>();
                    foreach (var row in results)
                    {
                        cooperados.Add(new Cooperado
                        {
                            Id = GetString(row, "id"),
                            Nome = GetString(row, "name", "nome", "cooperado_nome", "CooperadoNome"),  // Fallback para "nome" se existir
                            Cpf = GetNullableString(row, "cpf", "CPF"),
                            Email = GetNullableString(row, "email", "Email"),
                            Telefone = GetNullableString(row, "phone", "telefone", "Telefone"),  // "phone" é o correto
                            CriadoEm = GetNullableDate(row, "created_at", "criado_em"),
                            Ativo = GetNullableBool(row, "status", "ativo") ?? true,
                            ProducaoPorCpf = GetNullableString(row, "producao_por_cpf", "ProducaoPorCpf")
                        });
                    }

                    System.Diagnostics.Debug.WriteLine($"[TursoCooperadoHelper] GetCooperadosAsync: Retornando {cooperados.Count} cooperados");
                    return cooperados;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TursoCooperadoHelper] GetCooperadosAsync com SELECT específico falhou: {ex.Message}");
            }

            // Fallback: SELECT * com parsing flexível
            try
            {
                var sqlFallback = "SELECT * FROM cooperados ORDER BY name";
                var resultsFallback = await _tursoConnection.ExecuteQueryAsync(sqlFallback);

                if (resultsFallback != null && resultsFallback.Count > 0)
                {
                    var cooperados = new List<Cooperado>();
                    foreach (var row in resultsFallback)
                    {
                        // Log das colunas disponíveis (apenas primeira linha)
                        if (cooperados.Count == 0)
                        {
                            var colNames = string.Join(", ", row.Keys);
                            System.Diagnostics.Debug.WriteLine($"[TursoCooperadoHelper] Colunas disponíveis: {colNames}");
                        }

                        cooperados.Add(new Cooperado
                        {
                            Id = GetString(row, "id"),
                            Nome = GetString(row, "name", "nome", "cooperado_nome", "CooperadoNome"),
                            Cpf = GetNullableString(row, "cpf", "CPF"),
                            Email = GetNullableString(row, "email", "Email"),
                            Telefone = GetNullableString(row, "phone", "telefone", "Telefone"),
                            CriadoEm = GetNullableDate(row, "created_at", "criado_em"),
                            Ativo = GetNullableBool(row, "status", "ativo") ?? true,
                            ProducaoPorCpf = GetNullableString(row, "producao_por_cpf", "ProducaoPorCpf")
                        });
                    }

                    System.Diagnostics.Debug.WriteLine($"[TursoCooperadoHelper] GetCooperadosAsync (fallback): Retornando {cooperados.Count} cooperados");
                    return cooperados;
                }
            }
            catch (Exception exFallback)
            {
                System.Diagnostics.Debug.WriteLine($"[TursoCooperadoHelper] GetCooperadosAsync fallback falhou: {exFallback.Message}");
            }

            System.Diagnostics.Debug.WriteLine("[TursoCooperadoHelper] GetCooperadosAsync: Nenhum cooperado encontrado!");
            return new List<Cooperado>();
        }

        public async Task<List<Hospital>> GetHospitaisAsync()
        {
            var sql = "SELECT id, nome, slug FROM hospitals ORDER BY nome";
            var results = await _tursoConnection.ExecuteQueryAsync(sql);

            var hospitais = new List<Hospital>();
            foreach (var row in results)
            {
                var id = GetString(row, "id");
                var nome = GetString(row, "nome");
                var codigo = GetString(row, "slug");

                if (string.IsNullOrEmpty(codigo))
                    codigo = id;

                hospitais.Add(new Hospital
                {
                    Id = id,
                    Nome = nome,
                    Codigo = codigo
                });
            }

            return hospitais;
        }

        public async Task<List<SetorInfo>> GetSetoresDoHospitalAsync(string hospitalId)
        {
            var setores = new List<SetorInfo>();

            try
            {
                var sql = @"SELECT s.id, s.nome
                            FROM setores s
                            INNER JOIN hospital_setores hs ON hs.setor_id = s.id
                            WHERE hs.hospital_id = ?
                            ORDER BY s.nome";

                var results = await _tursoConnection.ExecuteQueryAsync(sql, new object[] { hospitalId });
                foreach (var row in results)
                {
                    setores.Add(new SetorInfo
                    {
                        Id = GetInt(row, "id"),
                        Nome = GetString(row, "nome")
                    });
                }

                if (setores.Count > 0)
                    return setores;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TursoCooperadoHelper] Falha ao buscar setores por unidade: {ex.Message}");
            }

            // Fallback: todos os setores
            var fallbackSql = "SELECT id, nome FROM setores ORDER BY nome";
            var fallbackResults = await _tursoConnection.ExecuteQueryAsync(fallbackSql);
            foreach (var row in fallbackResults)
            {
                setores.Add(new SetorInfo
                {
                    Id = GetInt(row, "id"),
                    Nome = GetString(row, "nome")
                });
            }

            return setores;
        }

        public async Task<bool> TemBiometriaAsync(string cooperadoId)
        {
            var sql = "SELECT 1 FROM biometrias WHERE cooperado_id = ? LIMIT 1";
            var results = await _tursoConnection.ExecuteQueryAsync(sql, new object[] { cooperadoId });
            return results != null && results.Count > 0;
        }

        public async Task<Cooperado?> GetCooperadoPorCpfAsync(string cpf)
        {
            var result = await TryGetCooperadoPorCpfAsync(cpf);
            return result.Success ? result.Cooperado : null;
        }

        public async Task<(bool Success, Cooperado? Cooperado)> TryGetCooperadoPorCpfAsync(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return (true, null);

            string cpfNormalizado = NormalizarCpf(cpf);
            if (string.IsNullOrWhiteSpace(cpfNormalizado))
                return (true, null);

            try
            {
                var sql = @"SELECT id, name, cpf, email, phone, created_at, status, producao_por_cpf
                            FROM cooperados
                            WHERE REPLACE(REPLACE(REPLACE(cpf, '.', ''), '-', ''), ' ', '') = ?
                            LIMIT 1";
                var results = await _tursoConnection.ExecuteQueryAsync(sql, new object[] { cpfNormalizado });
                if (results != null && results.Count > 0)
                {
                    var row = results[0];
                    var cooperado = new Cooperado
                    {
                        Id = GetString(row, "id"),
                        Nome = GetString(row, "name", "nome", "cooperado_nome", "CooperadoNome"),
                        Cpf = GetNullableString(row, "cpf", "CPF"),
                        Email = GetNullableString(row, "email", "Email"),
                        Telefone = GetNullableString(row, "phone", "telefone", "Telefone"),
                        CriadoEm = GetNullableDate(row, "created_at", "criado_em"),
                        Ativo = GetNullableBool(row, "status", "ativo") ?? true,
                        ProducaoPorCpf = GetNullableString(row, "producao_por_cpf", "ProducaoPorCpf")
                    };
                    return (true, cooperado);
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TursoCooperadoHelper] TryGetCooperadoPorCpfAsync falhou: {ex.Message}");
                return (false, null);
            }
        }

        private static string NormalizarCpf(string cpf)
        {
            var digits = cpf.Where(char.IsDigit).ToArray();
            return new string(digits);
        }

        public async Task<bool> SalvarBiometriaAsync(string cooperadoId, byte[] template, int fingerIndex = 0, string cooperadoNome = null)
        {
            if (template == null || template.Length == 0)
                throw new ArgumentException("Template de biometria vazio.", nameof(template));

            var id = Guid.NewGuid().ToString();
            var hash = GerarHashSha256(template);
            var criadoEm = DateTime.UtcNow.ToString("O");

            var columns = await GetBiometriasColumnsAsync();
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

                AddIfExists("id", id);
                AddIfExists("cooperado_id", cooperadoId);
                AddIfExists("cooperadoid", cooperadoId);

                if (!string.IsNullOrWhiteSpace(cooperadoNome))
                {
                    AddIfExists("cooperado_nome", cooperadoNome);
                    AddIfExists("cooperadonome", cooperadoNome);
                    AddIfExists("name", cooperadoNome);
                }

                AddIfExists("finger_index", fingerIndex);
                AddIfExists("fingerindex", fingerIndex);
                AddIfExists("finger_id", fingerIndex);

                AddIfExists("hash", hash);

                AddIfExists("created_at", criadoEm);
                AddIfExists("criado_em", criadoEm);
                AddIfExists("created_at_db", criadoEm);
                AddIfExists("criado_em_db", criadoEm);
                AddIfExists("sincronizado_em", criadoEm);

                AddIfExists("tipo_impressao", "Todos");

                string? templateColumnKey = null;
                foreach (var candidate in new[] { "template", "template_bytes", "fingerprint_template", "biometric_template", "template_base64", "templatebase64" })
                {
                    if (columns.ContainsKey(candidate))
                    {
                        templateColumnKey = candidate;
                        break;
                    }
                }

                if (templateColumnKey != null)
                {
                    var colType = columns[templateColumnKey].Type ?? string.Empty;
                    if (colType.Contains("CHAR", StringComparison.OrdinalIgnoreCase) || colType.Contains("TEXT", StringComparison.OrdinalIgnoreCase))
                    {
                        AddIfExists(templateColumnKey, Convert.ToBase64String(template));
                    }
                    else
                    {
                        AddIfExists(templateColumnKey, template);
                    }
                }

                if (insertColumns.Count > 0)
                {
                    var placeholders = string.Join(", ", insertColumns.Select(_ => "?"));
                    var sqlDynamic = $"INSERT INTO biometrias ({string.Join(", ", insertColumns)}) VALUES ({placeholders})";
                    try
                    {
                        await _tursoConnection.ExecuteNonQueryAsync(sqlDynamic, insertValues.ToArray());
                        return true;
                    }
                    catch (Exception exDynamic)
                    {
                        System.Diagnostics.Debug.WriteLine($"[TursoCooperadoHelper] Insert dinamico falhou: {exDynamic.Message}");
                    }
                }
            }

            var sqlA = @"INSERT INTO biometrias (id, cooperado_id, cooperado_nome, template, finger_index, hash, created_at)
                         VALUES (?, ?, ?, ?, ?, ?, ?)";

            var sqlB = @"INSERT INTO biometrias (id, cooperado_id, cooperado_nome, template_bytes, tipo_impressao, criado_em, sincronizado_em)
                         VALUES (?, ?, ?, ?, ?, ?, ?)";

            Exception? lastErrorA = null;
            Exception? lastErrorB = null;

            try
            {
                await _tursoConnection.ExecuteNonQueryAsync(sqlA, new object[]
                {
                    id,
                    cooperadoId,
                    cooperadoNome ?? string.Empty,
                    template,
                    fingerIndex,
                    hash,
                    criadoEm
                });
                return true;
            }
            catch (Exception exA)
            {
                lastErrorA = exA;
                System.Diagnostics.Debug.WriteLine($"[TursoCooperadoHelper] Insert A falhou: {exA.Message}");
            }

            try
            {
                await _tursoConnection.ExecuteNonQueryAsync(sqlB, new object[]
                {
                    id,
                    cooperadoId,
                    cooperadoNome ?? string.Empty,
                    template,
                    "Todos",
                    criadoEm,
                    criadoEm
                });
                return true;
            }
            catch (Exception exB)
            {
                lastErrorB = exB;
                System.Diagnostics.Debug.WriteLine($"[TursoCooperadoHelper] Insert B falhou: {exB.Message}");
                var details = $"Insert A falhou: {lastErrorA?.Message ?? "(sem detalhes)"} | " +
                              $"Insert B falhou: {lastErrorB?.Message ?? "(sem detalhes)"}";
                throw new InvalidOperationException($"Falha ao salvar biometria no Turso. {details}", exB);
            }
        }

        public async Task<int> RemoverBiometriasAsync(string cooperadoId)
        {
            var sql = "DELETE FROM biometrias WHERE cooperado_id = ?";
            return await _tursoConnection.ExecuteNonQueryAsync(sql, new object[] { cooperadoId });
        }

        public async Task<bool> RegistrarPontoAsync(RegistroPonto ponto)
        {
            var sql = @"INSERT INTO pontos (
                            id, codigo, cooperado_id, cooperado_nome, timestamp, tipo,
                            local, hospital_id, setor_id, status, is_manual, related_id,
                            biometria_entrada_hash, biometria_saida_hash, validado_por,
                            rejeitado_por, motivo_rejeicao, observacao
                        ) VALUES (
                            ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?
                        )";

            await _tursoConnection.ExecuteNonQueryAsync(sql, new object[]
            {
                string.IsNullOrEmpty(ponto.Id) ? Guid.NewGuid().ToString() : ponto.Id,
                ponto.Codigo,
                ponto.CooperadoId,
                ponto.CooperadoNome,
                ponto.Timestamp.ToString("O"),
                ponto.Tipo,
                ponto.Local,
                ponto.HospitalId,
                ponto.SetorId,
                ponto.Status,
                ponto.IsManual ? 1 : 0,
                ponto.RelatedId,
                ponto.BiometriaEntradaHash,
                ponto.BiometriaSaidaHash,
                ponto.ValidadoPor,
                ponto.RejeitadoPor,
                ponto.MotivoRejeicao,
                ponto.Observacao
            });

            return true;
        }

        public async Task<List<(string CooperadoId, string CooperadoNome, byte[] Template, int FingerIndex)>> BaixarTodasBiometriasParaSincAsync()
        {
            var results = new List<(string CooperadoId, string CooperadoNome, byte[] Template, int FingerIndex)>();

            await GetBiometriasDiagnosticsAsync();

            // Leitura genérica da tabela biometrias (schema real do Turso)
            try
            {
                var rowsAll = await _tursoConnection.ExecuteQueryAsync("SELECT * FROM biometrias");
                if (rowsAll != null && rowsAll.Count > 0)
                {
                    var firstRow = rowsAll[0];
                    var colNames = string.Join(", ", firstRow.Keys);
                    System.Diagnostics.Debug.WriteLine($"[TursoCooperadoHelper] biometrias colunas: {colNames}");

                    foreach (var row in rowsAll)
                    {
                        var template = GetBytes(row, "template", "template_bytes", "fingerprint_template", "biometric_template", "template_base64", "templateBase64");
                        if (template == null || template.Length == 0)
                            continue;

                        results.Add((
                            GetString(row, "cooperado_id", "cooperadoId", "CooperadoId", "employee_id", "employeeId", "user_id", "userId", "cpf"),
                            GetString(row, "CooperadoNome", "cooperado_nome", "cooperadoNome", "name", "employee_name", "employeeName"),
                            template,
                            GetInt(row, "finger_index", "fingerIndex", "finger", "finger_id")
                        ));
                    }
                }
            }
            catch (Exception exAll)
            {
                System.Diagnostics.Debug.WriteLine($"[TursoCooperadoHelper] Query biometrias (*) falhou: {exAll.Message}");
            }

            if (results.Count > 0)
                return results;

            var sqlA = "SELECT cooperado_id, CooperadoNome, template, finger_index FROM biometrias WHERE template IS NOT NULL AND length(template) > 0";
            var sqlB = "SELECT cooperado_id, CooperadoNome, template_bytes, 0 as finger_index FROM biometrias WHERE template_bytes IS NOT NULL AND length(template_bytes) > 0";
            var sqlEmployees = "SELECT cooperado_id, cooperado_nome, fingerprint_template, 0 as finger_index FROM employees WHERE fingerprint_template IS NOT NULL AND length(fingerprint_template) > 0";

            try
            {
                var rows = await _tursoConnection.ExecuteQueryAsync(sqlA);
                foreach (var row in rows)
                {
                    results.Add((
                        GetString(row, "cooperado_id", "CooperadoId"),
                        GetString(row, "CooperadoNome", "cooperado_nome", "cooperadoNome"),
                        GetBytes(row, "template"),
                        GetInt(row, "finger_index")
                    ));
                }
                return results;
            }
            catch (Exception exA)
            {
                System.Diagnostics.Debug.WriteLine($"[TursoCooperadoHelper] Query A falhou: {exA.Message}");
            }

            var rowsB = await _tursoConnection.ExecuteQueryAsync(sqlB);
            foreach (var row in rowsB)
            {
                results.Add((
                    GetString(row, "cooperado_id", "CooperadoId"),
                    GetString(row, "CooperadoNome", "cooperado_nome", "cooperadoNome"),
                    GetBytes(row, "template_bytes"),
                    GetInt(row, "finger_index")
                ));
            }

            if (results.Count > 0)
                return results;

            try
            {
                var rowsEmp = await _tursoConnection.ExecuteQueryAsync(sqlEmployees);
                foreach (var row in rowsEmp)
                {
                    results.Add((
                        GetString(row, "cooperado_id", "cooperadoId", "CooperadoId", "id"),
                        GetString(row, "CooperadoNome", "cooperado_nome", "name", "nome"),
                        GetBytes(row, "fingerprint_template"),
                        GetInt(row, "finger_index")
                    ));
                }
            }
            catch (Exception exEmp)
            {
                System.Diagnostics.Debug.WriteLine($"[TursoCooperadoHelper] Query employees falhou: {exEmp.Message}");
            }

            if (results.Count > 0)
                return results;

            // Fallback flexível para diferentes nomes de tabela/colunas
            var tableCandidates = new[] { "biometrias", "biometrics", "biometria", "fingerprints", "employees" };
            foreach (var table in tableCandidates)
            {
                var fallbackResults = await TryLoadBiometriasFromTableAsync(table);
                if (fallbackResults.Count > 0)
                    return fallbackResults;
            }

            return results;
        }

        public async Task<List<string>> GetBiometriasDiagnosticsAsync()
        {
            var lines = new List<string>();
            try
            {
                var tables = await _tursoConnection.ExecuteQueryAsync(
                    "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name");

                var names = new List<string>();
                foreach (var row in tables)
                {
                    var name = GetString(row, "name");
                    if (!string.IsNullOrWhiteSpace(name))
                        names.Add(name);
                }

                var header = $"[TursoCooperadoHelper] Tabelas no Turso: {string.Join(", ", names)}";
                System.Diagnostics.Debug.WriteLine(header);
                lines.Add(header);

                var candidates = new[] { "biometrias", "biometrics", "biometria", "fingerprints", "employees", "cooperados" };
                foreach (var table in candidates)
                {
                    try
                    {
                        var countRows = await _tursoConnection.ExecuteQueryAsync($"SELECT COUNT(*) as total FROM {table}");
                        var total = countRows.Count > 0 ? GetInt(countRows[0], "total") : 0;
                        var line = $"[TursoCooperadoHelper] {table}: {total} registros";
                        System.Diagnostics.Debug.WriteLine(line);
                        lines.Add(line);
                    }
                    catch (Exception ex)
                    {
                        var line = $"[TursoCooperadoHelper] {table}: erro ao contar ({ex.Message})";
                        System.Diagnostics.Debug.WriteLine(line);
                        lines.Add(line);
                    }
                }

                // Diagnóstico de colunas reais em biometrias
                try
                {
                    var sampleRows = await _tursoConnection.ExecuteQueryAsync("SELECT * FROM biometrias LIMIT 1");
                    if (sampleRows.Count > 0)
                    {
                        var colNames = string.Join(", ", sampleRows[0].Keys);
                        var line = $"[TursoCooperadoHelper] biometrias colunas: {colNames}";
                        System.Diagnostics.Debug.WriteLine(line);
                        lines.Add(line);
                    }
                    else
                    {
                        var line = "[TursoCooperadoHelper] biometrias colunas: (nenhuma linha retornada)";
                        System.Diagnostics.Debug.WriteLine(line);
                        lines.Add(line);
                    }
                }
                catch (Exception ex)
                {
                    var line = $"[TursoCooperadoHelper] biometrias colunas: erro ({ex.Message})";
                    System.Diagnostics.Debug.WriteLine(line);
                    lines.Add(line);
                }
            }
            catch (Exception ex)
            {
                var line = $"[TursoCooperadoHelper] Diagnóstico falhou: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(line);
                lines.Add(line);
            }

            return lines;
        }

        private async Task<List<(string CooperadoId, string CooperadoNome, byte[] Template, int FingerIndex)>> TryLoadBiometriasFromTableAsync(string tableName)
        {
            var results = new List<(string CooperadoId, string CooperadoNome, byte[] Template, int FingerIndex)>();
            try
            {
                var rows = await _tursoConnection.ExecuteQueryAsync($"SELECT * FROM {tableName}");
                if (rows == null || rows.Count == 0)
                    return results;

                // Log das colunas disponíveis (apenas primeira linha)
                var firstRow = rows[0];
                var colNames = string.Join(", ", firstRow.Keys);
                System.Diagnostics.Debug.WriteLine($"[TursoCooperadoHelper] {tableName} colunas: {colNames}");

                foreach (var row in rows)
                {
                    var template = GetBytes(row, "template", "template_bytes", "fingerprint_template", "biometric_template", "template_base64", "templateBase64");
                    if (template == null || template.Length == 0)
                        continue;

                    var cooperadoId = GetString(row, "cooperado_id", "cooperadoId", "CooperadoId", "employee_id", "employeeId", "user_id", "userId", "cpf");
                    var cooperadoNome = GetString(row, "CooperadoNome", "cooperado_nome", "cooperadoNome", "name", "employee_name", "employeeName");
                    var fingerIndex = GetInt(row, "finger_index", "fingerIndex", "finger", "finger_id");

                    results.Add((cooperadoId, cooperadoNome, template, fingerIndex));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TursoCooperadoHelper] Fallback {tableName} falhou: {ex.Message}");
            }

            return results;
        }

        public async Task<bool> ValidarManagerByPasswordAsync(string password)
        {
            var sql = "SELECT 1 FROM managers WHERE senha = ? LIMIT 1";
            var results = await _tursoConnection.ExecuteQueryAsync(sql, new object[] { password });
            return results != null && results.Count > 0;
        }

        private static string GerarHashSha256(byte[] data)
        {
            using var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(data);
            var sb = new StringBuilder(hashBytes.Length * 2);
            foreach (var b in hashBytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        private static string GetString(Dictionary<string, object> row, string key)
        {
            return row.TryGetValue(key, out var val) ? val?.ToString() ?? string.Empty : string.Empty;
        }

        private static string GetString(Dictionary<string, object> row, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (row.TryGetValue(key, out var val) && val != null)
                    return val.ToString() ?? string.Empty;
            }
            return string.Empty;
        }

        private static string? GetNullableString(Dictionary<string, object> row, string key)
        {
            return row.TryGetValue(key, out var val) ? val?.ToString() : null;
        }

        private static string? GetNullableString(Dictionary<string, object> row, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (row.TryGetValue(key, out var val) && val != null)
                    return val.ToString();
            }
            return null;
        }

        private async Task<Dictionary<string, (string Name, string Type)>> GetBiometriasColumnsAsync()
        {
            if (_biometriasColumnsCache != null && (DateTime.UtcNow - _biometriasColumnsCacheAt) < BiometriasColumnsCacheTtl)
                return _biometriasColumnsCache;

            var columns = new Dictionary<string, (string Name, string Type)>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var rows = await _tursoConnection.ExecuteQueryAsync("PRAGMA table_info(biometrias)");
                foreach (var row in rows)
                {
                    var name = GetString(row, "name");
                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    var type = GetNullableString(row, "type") ?? string.Empty;
                    columns[name.ToLowerInvariant()] = (name, type);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TursoCooperadoHelper] Erro ao ler schema de biometrias: {ex.Message}");
            }

            _biometriasColumnsCache = columns;
            _biometriasColumnsCacheAt = DateTime.UtcNow;
            return columns;
        }

        private static int GetInt(Dictionary<string, object> row, string key)
        {
            if (!row.TryGetValue(key, out var val) || val == null)
                return 0;

            if (val is int i) return i;
            if (val is long l) return (int)l;
            if (int.TryParse(val.ToString(), out var parsed)) return parsed;
            return 0;
        }

        private static int GetInt(Dictionary<string, object> row, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (row.TryGetValue(key, out var val) && val != null)
                {
                    if (val is int i) return i;
                    if (val is long l) return (int)l;
                    if (int.TryParse(val.ToString(), out var parsed)) return parsed;
                }
            }
            return 0;
        }

        private static bool? GetNullableBool(Dictionary<string, object> row, string key)
        {
            if (!row.TryGetValue(key, out var val) || val == null)
                return null;

            if (val is bool b) return b;
            if (val is int i) return i != 0;
            if (val is long l) return l != 0;
            if (bool.TryParse(val.ToString(), out var parsed)) return parsed;
            return null;
        }

        private static bool? GetNullableBool(Dictionary<string, object> row, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (row.TryGetValue(key, out var val) && val != null)
                {
                    if (val is bool b) return b;
                    if (val is int i) return i != 0;
                    if (val is long l) return l != 0;
                    if (bool.TryParse(val.ToString(), out var parsed)) return parsed;
                }
            }
            return null;
        }

        private static DateTime? GetNullableDate(Dictionary<string, object> row, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (row.TryGetValue(key, out var val) && val != null)
                {
                    if (val is DateTime dt) return dt;
                    if (DateTime.TryParse(val.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
                        return parsed;
                }
            }
            return null;
        }

        private static byte[] GetBytes(Dictionary<string, object> row, string key)
        {
            if (!row.TryGetValue(key, out var val) || val == null)
                return Array.Empty<byte>();

            if (val is byte[] bytes) return bytes;
            var str = val.ToString();
            if (string.IsNullOrWhiteSpace(str)) return Array.Empty<byte>();

            try
            {
                return Convert.FromBase64String(str);
            }
            catch
            {
                        return TryParseHexBytes(str);
            }
        }

        private static byte[] GetBytes(Dictionary<string, object> row, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (row.TryGetValue(key, out var val) && val != null)
                {
                    if (val is byte[] bytes) return bytes;
                    var str = val.ToString();
                    if (string.IsNullOrWhiteSpace(str)) continue;
                    try
                    {
                        return Convert.FromBase64String(str);
                    }
                    catch
                    {
                                var hex = TryParseHexBytes(str);
                                if (hex.Length > 0)
                                    return hex;
                                return Array.Empty<byte>();
                    }
                }
            }
            return Array.Empty<byte>();
        }

        private static byte[] TryParseHexBytes(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Array.Empty<byte>();

            var hex = value.Trim();
            if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                hex = hex.Substring(2);
            if (hex.StartsWith("\\x", StringComparison.OrdinalIgnoreCase))
                hex = hex.Substring(2);

            if (hex.Length % 2 != 0)
                return Array.Empty<byte>();

            try
            {
                var bytes = new byte[hex.Length / 2];
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
                }
                return bytes;
            }
            catch
            {
                return Array.Empty<byte>();
            }
        }
    }
}