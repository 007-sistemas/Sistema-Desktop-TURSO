

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using BiometricSystem.Models;

namespace BiometricSystem.Database
{
        public class DatabaseHelper
        {
            private readonly string connectionString;

            /// <summary>
            /// Retorna todas as biometrias cadastradas localmente
            /// </summary>
            public List<(string CooperadoId, string CooperadoNome, byte[] Template)> BuscarBiometriasLocais()
            {
                var biometrias = new List<(string CooperadoId, string CooperadoNome, byte[] Template)>();
                try
                {
                    using var connection = new SQLiteConnection(connectionString);
                    connection.Open();
                    string query = "SELECT CooperadoId, CooperadoNome, Template FROM Biometrias";
                    using var cmd = new SQLiteCommand(query, connection);
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        string cooperadoId = reader.GetString(0);
                        string cooperadoNome = reader.GetString(1);
                        byte[] template = (byte[])reader[2];
                        biometrias.Add((cooperadoId, cooperadoNome, template));
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao buscar biometrias locais: {ex.Message}");
                }
                return biometrias;
            }

            /// <summary>
            /// Salva um registro de producao local na tabela Pontos
            /// </summary>
            public bool SalvarPontoLocal(string cooperadoId, string cooperadoNome, string tipo, string local, string? hospitalId, int? setorId)
            {
                try
                {
                    using var connection = new SQLiteConnection(connectionString);
                    connection.Open();
                    string id = Guid.NewGuid().ToString();
                    string codigo = id.Substring(0, 8); // Código simplificado
                    string query = @"INSERT INTO Pontos (Id, Codigo, CooperadoId, CooperadoNome, Timestamp, Tipo, Local, HospitalId, SetorId, Status, IsManual, SyncedToTurso) 
                                    VALUES (@Id, @Codigo, @CooperadoId, @CooperadoNome, @Timestamp, @Tipo, @Local, @HospitalId, @SetorId, @Status, @IsManual, 0)";
                    using var cmd = new SQLiteCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Codigo", codigo);
                    cmd.Parameters.AddWithValue("@CooperadoId", cooperadoId);
                    cmd.Parameters.AddWithValue("@CooperadoNome", cooperadoNome);
                    cmd.Parameters.AddWithValue("@Timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@Tipo", tipo);
                    cmd.Parameters.AddWithValue("@Local", local ?? "");
                    cmd.Parameters.AddWithValue("@HospitalId", hospitalId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SetorId", setorId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", "Aberto");
                    cmd.Parameters.AddWithValue("@IsManual", 0);
                    return cmd.ExecuteNonQuery() > 0;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao salvar producao local: {ex.Message}");
                    return false;
                }
            }

        public DatabaseHelper()
        {
            // Inicialização segura do caminho do banco SQLite
            string? appDataRoot = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (string.IsNullOrEmpty(appDataRoot))
                appDataRoot = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (string.IsNullOrEmpty(appDataRoot))
                appDataRoot = @"C:\\Temp";

            // Fallback absoluto se ainda assim vier nulo ou vazio
            if (string.IsNullOrEmpty(appDataRoot))
                appDataRoot = @"C:\\Temp";

            string appDataDir = !string.IsNullOrEmpty(appDataRoot) ? Path.Combine(appDataRoot, "BiometricSystem") : @"C:\\Temp\\BiometricSystem";
            Directory.CreateDirectory(appDataDir);

            string dbPath = !string.IsNullOrEmpty(appDataDir) ? Path.Combine(appDataDir, "biometric.db") : @"C:\\Temp\\BiometricSystem\\biometric.db";
            string logFile = !string.IsNullOrEmpty(appDataDir) ? Path.Combine(appDataDir, "biometric_path_log.txt") : @"C:\\Temp\\BiometricSystem\\biometric_path_log.txt";

            void LogPath(string label, string? value)
            {
                try
                {
                    File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {label}: {value ?? "<null>"}\n");
                }
                catch { }
            }

            LogPath("APPDATA", appDataRoot);
            LogPath("appDataDir", appDataDir);
            LogPath("dbPath", dbPath);

            if (string.IsNullOrEmpty(dbPath))
            {
                LogPath("ERRO FATAL", "dbPath está nulo ou vazio! Encerrando aplicação.");
                System.Windows.Forms.MessageBox.Show("Erro crítico ao determinar o caminho do banco de dados. Encerrando aplicação.", "Erro", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                Environment.Exit(1);
            }

            connectionString = $"Data Source={dbPath};Version=3;";
            LogPath("connectionString", connectionString);

            // Criar tabelas se não existirem
            try
            {
                using var connection = new SQLiteConnection(connectionString);
                connection.Open();
                
                // Tabela de senha local
                string createPasswordTable = @"CREATE TABLE IF NOT EXISTS LocalPassword (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PasswordHash TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL
                )";
                using var cmdPassword = new SQLiteCommand(createPasswordTable, connection);
                cmdPassword.ExecuteNonQuery();

                // Tabela de Biometrias
                string createBiometriaTable = @"CREATE TABLE IF NOT EXISTS Biometrias (
                    Id TEXT PRIMARY KEY,
                    CooperadoId TEXT NOT NULL,
                    CooperadoNome TEXT NOT NULL,
                    FingerIndex INTEGER NOT NULL,
                    Hash TEXT,
                    Template BLOB NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    SyncedToTurso INTEGER DEFAULT 0,
                    LastSyncTime TEXT
                )";
                using var cmdBiometria = new SQLiteCommand(createBiometriaTable, connection);
                cmdBiometria.ExecuteNonQuery();

                // Tabela de Pontos
                string createPontosTable = @"CREATE TABLE IF NOT EXISTS Pontos (
                    Id TEXT PRIMARY KEY,
                    Codigo TEXT NOT NULL,
                    CooperadoId TEXT NOT NULL,
                    CooperadoNome TEXT NOT NULL,
                    Timestamp TEXT NOT NULL,
                    Tipo TEXT NOT NULL,
                    Local TEXT,
                    HospitalId TEXT,
                    SetorId INTEGER,
                    Status TEXT,
                    IsManual INTEGER DEFAULT 0,
                    SyncedToTurso INTEGER DEFAULT 0,
                    LastSyncTime TEXT
                )";
                using var cmdPontos = new SQLiteCommand(createPontosTable, connection);
                cmdPontos.ExecuteNonQuery();

                // Tabela de Setores (cache)
                string createSetoresTable = @"CREATE TABLE IF NOT EXISTS Setores (
                    Id INTEGER PRIMARY KEY,
                    Nome TEXT NOT NULL,
                    HospitalId TEXT,
                    LastSyncTime TEXT
                )";
                using var cmdSetores = new SQLiteCommand(createSetoresTable, connection);
                cmdSetores.ExecuteNonQuery();

                // Remover caches legados (agora le diretamente da tabela cooperados)
                DropTableIfExists(connection, "CooperadosCpfCache");
                DropTableIfExists(connection, "CooperadosStatusCache");

                // Tabela de Employees (legado/compatibilidade)
                string createEmployeesTable = @"CREATE TABLE IF NOT EXISTS Employees (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    CPF TEXT UNIQUE NOT NULL,
                    Email TEXT,
                    Position TEXT,
                    FingerprintTemplate BLOB,
                    CreatedAt TEXT NOT NULL,
                    IsActive INTEGER DEFAULT 1,
                    SyncedToCloud INTEGER DEFAULT 0,
                    LastSyncTime TEXT,
                    CloudId TEXT
                )";
                using var cmdEmployees = new SQLiteCommand(createEmployeesTable, connection);
                cmdEmployees.ExecuteNonQuery();

                // Tabela de TimeRecords (legado/compatibilidade)
                string createTimeRecordsTable = @"CREATE TABLE IF NOT EXISTS TimeRecords (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    EmployeeId INTEGER NOT NULL,
                    Timestamp TEXT NOT NULL,
                    Type TEXT NOT NULL,
                    Notes TEXT,
                    SyncedToCloud INTEGER DEFAULT 0,
                    LastSyncTime TEXT,
                    CloudId TEXT
                )";
                using var cmdTimeRecords = new SQLiteCommand(createTimeRecordsTable, connection);
                cmdTimeRecords.ExecuteNonQuery();

                // Garantir colunas em bases antigas
                EnsureColumnExists(connection, "Employees", "SyncedToCloud", "INTEGER DEFAULT 0");
                EnsureColumnExists(connection, "Employees", "LastSyncTime", "TEXT");
                EnsureColumnExists(connection, "Employees", "CloudId", "TEXT");
                EnsureColumnExists(connection, "TimeRecords", "SyncedToCloud", "INTEGER DEFAULT 0");
                EnsureColumnExists(connection, "TimeRecords", "LastSyncTime", "TEXT");
                EnsureColumnExists(connection, "TimeRecords", "CloudId", "TEXT");
                EnsureColumnExists(connection, "Biometrias", "SyncedToTurso", "INTEGER DEFAULT 0");
                EnsureColumnExists(connection, "Pontos", "SyncedToTurso", "INTEGER DEFAULT 0");

                TryMigrateSyncFlag(connection, "Biometrias", "SyncedToNeon", "SyncedToTurso");
                TryMigrateSyncFlag(connection, "Pontos", "SyncedToNeon", "SyncedToTurso");

                // Índices
                CreateIndexIfNotExists(connection, "idx_biometrias_cooperado", "Biometrias", "CooperadoId");
                DropIndexIfExists(connection, "idx_biometrias_synced");
                CreateIndexIfNotExists(connection, "idx_biometrias_synced", "Biometrias", "SyncedToTurso");
                CreateIndexIfNotExists(connection, "idx_pontos_cooperado", "Pontos", "CooperadoId");
                DropIndexIfExists(connection, "idx_pontos_synced");
                CreateIndexIfNotExists(connection, "idx_pontos_synced", "Pontos", "SyncedToTurso");
                CreateIndexIfNotExists(connection, "idx_pontos_timestamp", "Pontos", "Timestamp");
                CreateIndexIfNotExists(connection, "idx_setores_hospital", "Setores", "HospitalId");
                CreateIndexIfNotExists(connection, "idx_cooperados_cpf", "cooperados", "cpf");
                CreateIndexIfNotExists(connection, "idx_cooperados_id", "cooperados", "id");
                CreateIndexIfNotExists(connection, "idx_employees_active", "Employees", "IsActive");
                CreateIndexIfNotExists(connection, "idx_employees_cpf", "Employees", "CPF");
                CreateIndexIfNotExists(connection, "idx_employees_synced", "Employees", "SyncedToCloud");
                CreateIndexIfNotExists(connection, "idx_timerecords_employee", "TimeRecords", "EmployeeId");
                CreateIndexIfNotExists(connection, "idx_timerecords_synced", "TimeRecords", "SyncedToCloud");
                CreateIndexIfNotExists(connection, "idx_timerecords_timestamp", "TimeRecords", "Timestamp");

                System.Diagnostics.Debug.WriteLine("✅ Todas as tabelas foram criadas/verificadas com sucesso");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao criar tabelas: {ex.Message}");
            }
        }
                /// <summary>
                /// Salva a senha local (hash) no banco. Só pode haver uma senha.
                /// </summary>
                public bool SalvarSenhaLocal(string senha)
                {
                    try
                    {
                        string hash = GerarHashSenha(senha);
                        using var connection = new SQLiteConnection(connectionString);
                        connection.Open();
                        // Remove senha anterior, se existir
                        string delete = "DELETE FROM LocalPassword";
                        using (var delCmd = new SQLiteCommand(delete, connection))
                            delCmd.ExecuteNonQuery();
                        // Insere nova senha
                        string insert = "INSERT INTO LocalPassword (PasswordHash, CreatedAt) VALUES (@hash, @createdAt)";
                        using var cmd = new SQLiteCommand(insert, connection);
                        cmd.Parameters.AddWithValue("@hash", hash);
                        cmd.Parameters.AddWithValue("@createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        return cmd.ExecuteNonQuery() > 0;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Erro ao salvar senha local: {ex.Message}");
                        return false;
                    }
                }

                /// <summary>
                /// Verifica se já existe senha local cadastrada
                /// </summary>
                public bool ExisteSenhaLocal()
                {
                    try
                    {
                        using var connection = new SQLiteConnection(connectionString);
                        connection.Open();
                        string query = "SELECT COUNT(*) FROM LocalPassword";
                        using var cmd = new SQLiteCommand(query, connection);
                        var result = cmd.ExecuteScalar();
                        if (result != null && int.TryParse(result.ToString(), out int count))
                            return count > 0;
                        return false;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Erro ao verificar senha local: {ex.Message}");
                        return false;
                    }
                }

                /// <summary>
                /// Valida a senha local informada
                /// </summary>
                public bool ValidarSenhaLocal(string senha)
                {
                    try
                    {
                        string hash = GerarHashSenha(senha);
                        using var connection = new SQLiteConnection(connectionString);
                        connection.Open();
                        string query = "SELECT COUNT(*) FROM LocalPassword WHERE PasswordHash = @hash";
                        using var cmd = new SQLiteCommand(query, connection);
                        cmd.Parameters.AddWithValue("@hash", hash);
                        var result = cmd.ExecuteScalar();
                        if (result != null && int.TryParse(result.ToString(), out int count))
                            return count > 0;
                        return false;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Erro ao validar senha local: {ex.Message}");
                        return false;
                    }
                }

                /// <summary>
                /// Gera hash SHA256 da senha
                /// </summary>
                private string GerarHashSenha(string senha)
                {
                    using (var sha256 = System.Security.Cryptography.SHA256.Create())
                    {
                        byte[] hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(senha));
                        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                    }
                }


        /// <summary>
        /// Decide o tipo do próximo ponto (ENTRADA/SAIDA) considerando tolerância e plantão noturno.
        /// </summary>
        public string DecidirTipoProximoPonto(string cooperadoId, int toleranciaHorasMin = 14, int toleranciaHorasMax = 16)
        {
            try
            {
                using var connection = new SQLiteConnection(connectionString);
                connection.Open();
                string query = @"SELECT Tipo, Timestamp FROM Pontos WHERE CooperadoId = @CooperadoId ORDER BY datetime(Timestamp) DESC LIMIT 1";
                using var cmd = new SQLiteCommand(query, connection);
                cmd.Parameters.AddWithValue("@CooperadoId", cooperadoId);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string ultimoTipo = reader.GetString(0);
                    string timestampStr = reader.GetString(1);
                    DateTimeOffset ultimoRegistro;
                    // Tenta parsear considerando timezone
                    if (DateTimeOffset.TryParse(timestampStr, out ultimoRegistro))
                    {
                        var agora = DateTimeOffset.Now;
                        var diffHoras = (agora - ultimoRegistro).TotalHours;
                        // Se não há registro ou o último é SAIDA, sempre força ENTRADA
                        if (string.IsNullOrEmpty(ultimoTipo) || ultimoTipo.Equals("SAIDA", StringComparison.OrdinalIgnoreCase))
                            return "ENTRADA";
                        // Se o último é ENTRADA
                        if (ultimoTipo.Equals("ENTRADA", StringComparison.OrdinalIgnoreCase))
                        {
                            // Se excedeu tolerância, força ENTRADA
                            if (diffHoras >= toleranciaHorasMin)
                                return "ENTRADA";
                            // Caso contrário, alterna normalmente
                            return "SAIDA";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao decidir tipo da proxima producao: {ex.Message}");
            }
            // Se não encontrou registro, sempre retorna ENTRADA
            return "ENTRADA";
        }

        private void CreateIndexIfNotExists(SQLiteConnection connection, string indexName, string tableName, string columnName)
        {
            string query = $"CREATE INDEX IF NOT EXISTS {indexName} ON {tableName}({columnName})";
            using var cmd = new SQLiteCommand(query, connection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch { /* Índice já existe */ }
        }

        private void DropIndexIfExists(SQLiteConnection connection, string indexName)
        {
            try
            {
                string query = $"DROP INDEX IF EXISTS {indexName}";
                using var cmd = new SQLiteCommand(query, connection);
                cmd.ExecuteNonQuery();
            }
            catch { /* Ignorar */ }
        }

        private void DropTableIfExists(SQLiteConnection connection, string tableName)
        {
            try
            {
                string query = $"DROP TABLE IF EXISTS {tableName}";
                using var cmd = new SQLiteCommand(query, connection);
                cmd.ExecuteNonQuery();
            }
            catch { /* Ignorar */ }
        }

        private void TryMigrateSyncFlag(SQLiteConnection connection, string tableName, string oldColumn, string newColumn)
        {
            try
            {
                string query = $"UPDATE {tableName} SET {newColumn} = {oldColumn} WHERE {oldColumn} IS NOT NULL";
                using var cmd = new SQLiteCommand(query, connection);
                cmd.ExecuteNonQuery();
            }
            catch
            {
                // Coluna antiga pode não existir
            }
        }

        public bool AddEmployee(Employee employee)
        {
            try
            {
                using var connection = new SQLiteConnection(connectionString);
                connection.Open();

                string query = @"INSERT INTO Employees (Name, CPF, Email, Position, FingerprintTemplate, CreatedAt, IsActive) 
                                VALUES (@Name, @CPF, @Email, @Position, @Template, @CreatedAt, @IsActive)";

                using var cmd = new SQLiteCommand(query, connection);
                cmd.Parameters.AddWithValue("@Name", employee.Name);
                cmd.Parameters.AddWithValue("@CPF", employee.CPF);
                cmd.Parameters.AddWithValue("@Email", employee.Email ?? "");
                cmd.Parameters.AddWithValue("@Position", employee.Position ?? "");
                cmd.Parameters.AddWithValue("@Template", employee.FingerprintTemplate ?? Array.Empty<byte>());
                cmd.Parameters.AddWithValue("@CreatedAt", employee.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@IsActive", employee.IsActive ? 1 : 0);

                return cmd.ExecuteNonQuery() > 0;
            }
            catch
            {
                return false;
            }
        }

        private void EnsureColumnExists(SQLiteConnection connection, string tableName, string columnName, string columnDefinition)
        {
            try
            {
                string checkQuery = $"PRAGMA table_info({tableName})";
                using var checkCmd = new SQLiteCommand(checkQuery, connection);
                using var reader = checkCmd.ExecuteReader();

                bool exists = false;
                while (reader.Read())
                {
                    if (reader[1].ToString()?.Equals(columnName, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    string alter = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDefinition}";
                    using var alterCmd = new SQLiteCommand(alter, connection);
                    alterCmd.ExecuteNonQuery();
                }
            }
            catch
            {
                // Silenciar para ambientes com SQLite antigo; não é crítico se já existir
            }
        }

        public List<Employee> GetAllEmployees()
        {
            var employees = new List<Employee>();

            using var connection = new SQLiteConnection(connectionString);
            connection.Open();

            string query = "SELECT * FROM Employees WHERE IsActive = 1 ORDER BY Name";
            using var cmd = new SQLiteCommand(query, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                employees.Add(new Employee
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    CPF = reader.GetString(2),
                    Email = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Position = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    FingerprintTemplate = reader.IsDBNull(5) ? null : (byte[])reader[5],
                    CreatedAt = DateTime.Parse(reader.GetString(6)),
                    IsActive = reader.GetInt32(7) == 1
                });
            }

            return employees;
        }

        public Employee? GetEmployeeById(int employeeId)
        {
            try
            {
                using var connection = new SQLiteConnection(connectionString);
                connection.Open();

                string query = "SELECT * FROM Employees WHERE Id = @Id AND IsActive = 1";
                using var cmd = new SQLiteCommand(query, connection);
                cmd.Parameters.AddWithValue("@Id", employeeId);
                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new Employee
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        CPF = reader.GetString(2),
                        Email = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        Position = reader.IsDBNull(4) ? "" : reader.GetString(4),
                        FingerprintTemplate = reader.IsDBNull(5) ? null : (byte[])reader[5],
                        CreatedAt = DateTime.Parse(reader.GetString(6)),
                        IsActive = reader.GetInt32(7) == 1
                    };
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public Employee? FindEmployeeByFingerprint(byte[] template)
        {
            // Implementação simplificada - em produção, usar comparação de templates
            return null;
        }

        public bool RegisterTimeRecord(TimeRecord record)
        {
            try
            {
                using var connection = new SQLiteConnection(connectionString);
                connection.Open();

                string query = @"INSERT INTO TimeRecords (EmployeeId, Timestamp, Type, Notes, SyncedToCloud) 
                                VALUES (@EmployeeId, @Timestamp, @Type, @Notes, 0)";

                using var cmd = new SQLiteCommand(query, connection);
                cmd.Parameters.AddWithValue("@EmployeeId", record.EmployeeId);
                cmd.Parameters.AddWithValue("@Timestamp", record.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@Type", record.Type);
                cmd.Parameters.AddWithValue("@Notes", record.Notes ?? "");

                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao registrar producao: {ex.Message}");
                return false;
            }
        }

        public List<TimeRecord> GetEmployeeTimeRecords(int employeeId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var records = new List<TimeRecord>();

            using var connection = new SQLiteConnection(connectionString);
            connection.Open();

            string query = "SELECT * FROM TimeRecords WHERE EmployeeId = @EmployeeId";
            if (startDate.HasValue)
                query += " AND Timestamp >= @StartDate";
            if (endDate.HasValue)
                query += " AND Timestamp <= @EndDate";
            query += " ORDER BY Timestamp DESC";

            using var cmd = new SQLiteCommand(query, connection);
            cmd.Parameters.AddWithValue("@EmployeeId", employeeId);
            if (startDate.HasValue)
                cmd.Parameters.AddWithValue("@StartDate", startDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            if (endDate.HasValue)
                cmd.Parameters.AddWithValue("@EndDate", endDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                records.Add(new TimeRecord
                {
                    Id = reader.GetInt32(0),
                    EmployeeId = reader.GetInt32(1),
                    Timestamp = DateTime.Parse(reader.GetString(2)),
                    Type = reader.GetString(3),
                    Notes = reader.IsDBNull(4) ? null : reader.GetString(4)
                });
            }

            return records;
        }

        /// <summary>
        /// Obtém todos os funcionários não sincronizados com a nuvem
        /// </summary>
        public List<Employee> GetUnsyncedEmployees()
        {
            var employees = new List<Employee>();

            using var connection = new SQLiteConnection(connectionString);
            connection.Open();

            string query = "SELECT * FROM Employees WHERE SyncedToCloud = 0 AND IsActive = 1 ORDER BY CreatedAt DESC";
            using var cmd = new SQLiteCommand(query, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                employees.Add(new Employee
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    CPF = reader.GetString(2),
                    Email = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Position = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    FingerprintTemplate = reader.IsDBNull(5) ? null : (byte[])reader[5],
                    CreatedAt = DateTime.Parse(reader.GetString(6)),
                    IsActive = reader.GetInt32(7) == 1
                });
            }

            return employees;
        }

        /// <summary>
        /// Obtém todos os registros de ponto não sincronizados com a nuvem
        /// </summary>
        public List<TimeRecord> GetUnsyncedTimeRecords()
        {
            var records = new List<TimeRecord>();

            using var connection = new SQLiteConnection(connectionString);
            connection.Open();

            string query = @"
                SELECT tr.* FROM TimeRecords tr
                WHERE tr.SyncedToCloud = 0
                ORDER BY tr.Timestamp DESC";
            
            using var cmd = new SQLiteCommand(query, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                records.Add(new TimeRecord
                {
                    Id = reader.GetInt32(0),
                    EmployeeId = reader.GetInt32(1),
                    Timestamp = DateTime.Parse(reader.GetString(2)),
                    Type = reader.GetString(3),
                    Notes = reader.IsDBNull(4) ? null : reader.GetString(4)
                });
            }

            return records;
        }

        /// <summary>
        /// Marca um funcionário como sincronizado
        /// </summary>
        public bool MarkEmployeeAsSynced(int employeeId, string cloudId = null)
        {
            try
            {
                using var connection = new SQLiteConnection(connectionString);
                connection.Open();

                string query = @"UPDATE Employees 
                                SET SyncedToCloud = 1, LastSyncTime = @LastSyncTime, CloudId = @CloudId
                                WHERE Id = @Id";

                using var cmd = new SQLiteCommand(query, connection);
                cmd.Parameters.AddWithValue("@Id", employeeId);
                cmd.Parameters.AddWithValue("@LastSyncTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@CloudId", cloudId ?? (object)DBNull.Value);

                return cmd.ExecuteNonQuery() > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Marca um registro de ponto como sincronizado
        /// </summary>
        public bool MarkTimeRecordAsSynced(int timeRecordId, string cloudId = null)
        {
            try
            {
                using var connection = new SQLiteConnection(connectionString);
                connection.Open();

                string query = @"UPDATE TimeRecords 
                                SET SyncedToCloud = 1, LastSyncTime = @LastSyncTime, CloudId = @CloudId
                                WHERE Id = @Id";

                using var cmd = new SQLiteCommand(query, connection);
                cmd.Parameters.AddWithValue("@Id", timeRecordId);
                cmd.Parameters.AddWithValue("@LastSyncTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@CloudId", cloudId ?? (object)DBNull.Value);

                return cmd.ExecuteNonQuery() > 0;
            }
            catch
            {
                return false;
            }
        }

        // ==================== MÉTODOS PARA BIOMETRIAS ====================

        public bool SalvarBiometriaLocal(string cooperadoId, string cooperadoNome, byte[] template, out string errorMessage, string hash = "")
        {
            errorMessage = string.Empty;
            try
            {
                using var connection = new SQLiteConnection(connectionString);
                connection.Open();

                string id = Guid.NewGuid().ToString();
                string query = @"INSERT INTO Biometrias (Id, CooperadoId, CooperadoNome, FingerIndex, Hash, Template, CreatedAt, SyncedToTurso) 
                                VALUES (@Id, @CooperadoId, @CooperadoNome, @FingerIndex, @Hash, @Template, @CreatedAt, 0)";

                using var cmd = new SQLiteCommand(query, connection);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@CooperadoId", cooperadoId);
                cmd.Parameters.AddWithValue("@CooperadoNome", cooperadoNome);
                cmd.Parameters.AddWithValue("@FingerIndex", 0);
                cmd.Parameters.AddWithValue("@Hash", hash ?? "");
                cmd.Parameters.AddWithValue("@Template", template);
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                int rows = cmd.ExecuteNonQuery();
                System.Diagnostics.Debug.WriteLine($"✅ Biometria salva localmente: {cooperadoNome} (ID: {id}, Rows: {rows})");
                return rows > 0;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao salvar biometria local: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"   Stack Trace: {ex.StackTrace}");
                System.Diagnostics.Debug.WriteLine($"   CooperadoId: {cooperadoId}, Nome: {cooperadoNome}, Template size: {template?.Length ?? 0}");
                return false;
            }
        }

        // Sobrecarga para compatibilidade com código existente
        public bool SalvarBiometriaLocal(string cooperadoId, string cooperadoNome, byte[] template, string hash = "")
        {
            return SalvarBiometriaLocal(cooperadoId, cooperadoNome, template, out _, hash);
        }

        /// <summary>
        /// Verifica se o cooperado possui biometria cadastrada localmente
        /// </summary>
        public bool TemBiometriaLocal(string cooperadoId)
        {
            try
            {
                using var connection = new SQLiteConnection(connectionString);
                connection.Open();

                string query = "SELECT COUNT(*) FROM Biometrias WHERE CooperadoId = @CooperadoId";
                using var cmd = new SQLiteCommand(query, connection);
                cmd.Parameters.AddWithValue("@CooperadoId", cooperadoId);
                var result = cmd.ExecuteScalar();

                if (result != null && int.TryParse(result.ToString(), out int count))
                {
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao verificar biometria local: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Remove biometrias locais de um cooperado (apaga templates e hashes)
        /// </summary>
        public int RemoverBiometriasLocal(string cooperadoId, out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                using var connection = new SQLiteConnection(connectionString);
                connection.Open();

                string query = "DELETE FROM Biometrias WHERE CooperadoId = @CooperadoId";
                using var cmd = new SQLiteCommand(query, connection);
                cmd.Parameters.AddWithValue("@CooperadoId", cooperadoId);
                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                System.Diagnostics.Debug.WriteLine($"Erro ao remover biometria local: {ex.Message}");
                return 0;
            }
        }

        public bool TemRegistroRecente(string cooperadoId, int segundosMinimos = 30)
        {
            try
            {
                using var connection = new SQLiteConnection(connectionString);
                connection.Open();

                // Buscar o último registro deste cooperado
                string query = @"SELECT Timestamp FROM Pontos 
                               WHERE CooperadoId = @CooperadoId 
                               ORDER BY Timestamp DESC LIMIT 1";
                
                using var cmd = new SQLiteCommand(query, connection);
                cmd.Parameters.AddWithValue("@CooperadoId", cooperadoId);
                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    string timestampStr = reader.GetString(0);
                    if (DateTime.TryParse(timestampStr, out DateTime ultimoRegistro))
                    {
                        var diferencaSegundos = (DateTime.Now - ultimoRegistro).TotalSeconds;
                        return diferencaSegundos < segundosMinimos;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao verificar registro recente: {ex.Message}");
            }

            return false;
        }

        public (string Id, string Codigo)? ObterUltimaEntrada(string cooperadoId)
        {
            try
            {
                using var connection = new SQLiteConnection(connectionString);
                connection.Open();

                string query = @"SELECT Id, Codigo
                                FROM Pontos
                                WHERE CooperadoId = @CooperadoId AND Tipo = 'ENTRADA'
                                ORDER BY datetime(Timestamp) DESC
                                LIMIT 1";

                using var cmd = new SQLiteCommand(query, connection);
                cmd.Parameters.AddWithValue("@CooperadoId", cooperadoId);
                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return (reader.GetString(0), reader.GetString(1));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao buscar última entrada: {ex.Message}");
            }

            return null;
        }

        // Retorna o DateTimeOffset da última ENTRADA do cooperado
        public DateTimeOffset? ObterTimestampUltimaEntrada(string cooperadoId)
        {
            try
            {
                using var connection = new SQLiteConnection(connectionString);
                connection.Open();
                string query = @"SELECT Timestamp FROM Pontos WHERE CooperadoId = @CooperadoId AND Tipo = 'ENTRADA' ORDER BY datetime(Timestamp) DESC LIMIT 1";
                using var cmd = new SQLiteCommand(query, connection);
                cmd.Parameters.AddWithValue("@CooperadoId", cooperadoId);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string timestampStr = reader.GetString(0);
                    if (DateTimeOffset.TryParse(timestampStr, out var ultimaEntradaDt))
                        return ultimaEntradaDt;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao buscar timestamp da última entrada: {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Busca biometrias que ainda não foram sincronizadas com Turso
        /// </summary>
        public List<(string Id, string CooperadoId, string CooperadoNome, byte[] Template, int FingerIndex)> BuscarBiometriasNaoSincronizadas() {
            var biometrias = new List<(string Id, string CooperadoId, string CooperadoNome, byte[] Template, int FingerIndex)>();
            try
            {
                using var connection = new SQLiteConnection(connectionString);
                connection.Open();

                string query = "SELECT Id, CooperadoId, CooperadoNome, Template, FingerIndex FROM Biometrias WHERE SyncedToTurso = 0 LIMIT 500";
                using var cmd = new SQLiteCommand(query, connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string id = reader.GetString(0);
                    string cooperadoId = reader.GetString(1);
                    string cooperadoNome = reader.GetString(2);
                    byte[] template = (byte[])reader[3];
                    int fingerIndex = reader.GetInt32(4);

                    biometrias.Add((id, cooperadoId, cooperadoNome, template, fingerIndex));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao buscar biometrias não sincronizadas: {ex.Message}");
            }

            return biometrias;
        }

        /// <summary>
        /// Marca uma biometria como sincronizada com Turso
        /// </summary>
        public bool MarcabiometriaComoSincronizada(string biometriaId)
        {
            try
            {
                using var connection = new SQLiteConnection(connectionString);
                connection.Open();

                string query = "UPDATE Biometrias SET SyncedToTurso = 1, LastSyncTime = @LastSyncTime WHERE Id = @Id";
                using var cmd = new SQLiteCommand(query, connection);
                cmd.Parameters.AddWithValue("@Id", biometriaId);
                cmd.Parameters.AddWithValue("@LastSyncTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao marcar biometria como sincronizada: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Busca pontos que ainda não foram sincronizados com Turso
        /// </summary>
        public class PontoLocal
        {
            public string Id { get; set; } = string.Empty;
            public string Codigo { get; set; } = string.Empty;
            public string CooperadoId { get; set; } = string.Empty;
            public string CooperadoNome { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public string Local { get; set; } = string.Empty;
            public string? HospitalId { get; set; }
            public int? SetorId { get; set; }
            public string? Status { get; set; }
            public bool IsManual { get; set; }
            public string? RelatedId { get; set; }
            public string? Date { get; set; }
            public string? Entrada { get; set; }
            public string? Saida { get; set; }
            public string? Observacao { get; set; }
            public string? BiometriaEntradaHash { get; set; }
            public string? BiometriaSaidaHash { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public List<PontoLocal> BuscarPontosNaoSincronizados()
        {
            var pontos = new List<PontoLocal>();
            try
            {
                using var connection = new SQLiteConnection(connectionString);
                connection.Open();

                string query = @"SELECT Id, Codigo, CooperadoId, CooperadoNome, Tipo, Local, HospitalId, SetorId, Status, IsManual, Timestamp
                        FROM Pontos WHERE SyncedToTurso = 0 LIMIT 500";
                using var cmd = new SQLiteCommand(query, connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var ponto = new PontoLocal
                    {
                        Id = reader.GetString(0),
                        Codigo = reader.GetString(1),
                        CooperadoId = reader.GetString(2),
                        CooperadoNome = reader.GetString(3),
                        Tipo = reader.GetString(4),
                        Local = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                        HospitalId = reader.IsDBNull(6) ? null : reader.GetString(6),
                        SetorId = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                        Status = reader.IsDBNull(8) ? null : reader.GetString(8),
                        IsManual = !reader.IsDBNull(9) && reader.GetInt32(9) == 1,
                        RelatedId = null,
                        Date = null,
                        Entrada = null,
                        Saida = null,
                        Observacao = null,
                        BiometriaEntradaHash = null,
                        BiometriaSaidaHash = null,
                        Timestamp = DateTime.TryParse(reader.GetString(10), out var ts) ? ts : DateTime.Now
                    };

                    pontos.Add(ponto);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao buscar registros de producao nao sincronizados: {ex.Message}");
            }

            return pontos;
        }

        /// <summary>
        /// Marca um ponto como sincronizado com Turso
        /// </summary>
        public bool MarcaPontoComoSincronizado(string pontoId)
        {
            try
            {
                using var connection = new SQLiteConnection(connectionString);
                connection.Open();

                string query = "UPDATE Pontos SET SyncedToTurso = 1, LastSyncTime = @LastSyncTime WHERE Id = @Id";
                using var cmd = new SQLiteCommand(query, connection);
                cmd.Parameters.AddWithValue("@Id", pontoId);
                cmd.Parameters.AddWithValue("@LastSyncTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao marcar producao como sincronizada: {ex.Message}");
                return false;
            }
        }

        #region Setores Cache

        /// <summary>
        /// Salva/atualiza setores de um hospital no cache local
        /// </summary>
        public void SalvarSetoresLocal(string hospitalId, List<(int Id, string Nome)> setores)
        {
            try
            {
                using var connection = new SQLiteConnection(connectionString);
                connection.Open();

                // Limpar setores antigos deste hospital
                string deleteQuery = "DELETE FROM Setores WHERE HospitalId = @HospitalId";
                using (var deleteCmd = new SQLiteCommand(deleteQuery, connection))
                {
                    deleteCmd.Parameters.AddWithValue("@HospitalId", hospitalId);
                    deleteCmd.ExecuteNonQuery();
                }

                // Inserir novos setores
                string insertQuery = @"
                    INSERT INTO Setores (Id, Nome, HospitalId, LastSyncTime)
                    VALUES (@Id, @Nome, @HospitalId, @LastSyncTime)";

                string syncTime = DateTime.UtcNow.ToString("O");

                foreach (var setor in setores)
                {
                    using var insertCmd = new SQLiteCommand(insertQuery, connection);
                    insertCmd.Parameters.AddWithValue("@Id", setor.Id);
                    insertCmd.Parameters.AddWithValue("@Nome", setor.Nome);
                    insertCmd.Parameters.AddWithValue("@HospitalId", hospitalId);
                    insertCmd.Parameters.AddWithValue("@LastSyncTime", syncTime);
                    insertCmd.ExecuteNonQuery();
                }

                System.Diagnostics.Debug.WriteLine($"✅ {setores.Count} setores salvos localmente para hospital {hospitalId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao salvar setores localmente: {ex.Message}");
            }
        }

        /// <summary>
        /// Busca setores de um hospital do cache local
        /// </summary>
        public List<(int Id, string Nome)> BuscarSetoresLocal(string hospitalId)
        {
            var setores = new List<(int Id, string Nome)>();

            try
            {
                using var connection = new SQLiteConnection(connectionString);
                connection.Open();

                string query = "SELECT Id, Nome FROM Setores WHERE HospitalId = @HospitalId ORDER BY Nome";
                using var cmd = new SQLiteCommand(query, connection);
                cmd.Parameters.AddWithValue("@HospitalId", hospitalId);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    setores.Add((
                        Id: reader.GetInt32(0),
                        Nome: reader.GetString(1)
                    ));
                }

                System.Diagnostics.Debug.WriteLine($"📂 {setores.Count} setores carregados do cache local");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao buscar setores locais: {ex.Message}");
            }

            return setores;
        }

        /// <summary>
        /// Verifica se existem setores no cache local para um hospital
        /// </summary>
        public bool TemSetoresLocal(string hospitalId)
        {
            try
            {
                using var connection = new SQLiteConnection(connectionString);
                connection.Open();

                string query = "SELECT COUNT(*) FROM Setores WHERE HospitalId = @HospitalId";
                using var cmd = new SQLiteCommand(query, connection);
                cmd.Parameters.AddWithValue("@HospitalId", hospitalId);

                long count = (long)cmd.ExecuteScalar();
                return count > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Salva múltiplas biometrias em lote (usado para download inicial do Turso)
        /// Todas são marcadas como SyncedToTurso = 1
        /// </summary>
        public async Task<int> SalvarBiometriasEmLoteAsync(List<(string CooperadoId, string CooperadoNome, byte[] Template, int FingerIndex)> biometrias)
        {
            int totalInseridas = 0;
            try
            {
                using var connection = new SQLiteConnection(connectionString);
                connection.Open();

                using var transaction = connection.BeginTransaction();
                try
                {
                    foreach (var bio in biometrias)
                    {
                        string id = Guid.NewGuid().ToString();
                        string query = @"INSERT INTO Biometrias (Id, CooperadoId, CooperadoNome, FingerIndex, Hash, Template, CreatedAt, SyncedToTurso) 
                                        VALUES (@Id, @CooperadoId, @CooperadoNome, @FingerIndex, @Hash, @Template, @CreatedAt, 1)";

                        using var cmd = new SQLiteCommand(query, connection);
                        cmd.Transaction = transaction;
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.Parameters.AddWithValue("@CooperadoId", bio.CooperadoId);
                        cmd.Parameters.AddWithValue("@CooperadoNome", bio.CooperadoNome);
                        cmd.Parameters.AddWithValue("@FingerIndex", bio.FingerIndex);
                        cmd.Parameters.AddWithValue("@Hash", "");
                        cmd.Parameters.AddWithValue("@Template", bio.Template);
                        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0) totalInseridas++;
                    }

                    transaction.Commit();
                    System.Diagnostics.Debug.WriteLine($"✅ {totalInseridas} biometrias salvas em lote (SyncedToTurso = 1)");
                }
                catch (Exception txEx)
                {
                    transaction.Rollback();
                    System.Diagnostics.Debug.WriteLine($"❌ Erro na transação de biometrias: {txEx.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao salvar biometrias em lote: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"   Stack Trace: {ex.StackTrace}");
            }

            return totalInseridas;
        }

        #endregion

        #region Cooperados Local

        public (string CooperadoId, string Nome, string ProducaoPorCpf, string Status)? BuscarCooperadoLocalPorCpf(string cpf)
        {
            try
            {
                var cpfNormalizado = NormalizarCpf(cpf);
                if (string.IsNullOrWhiteSpace(cpfNormalizado))
                    return null;

                using var connection = new SQLiteConnection(connectionString);
                connection.Open();

                string query = @"SELECT id, name, producao_por_cpf, status FROM cooperados
                                 WHERE REPLACE(REPLACE(REPLACE(cpf, '.', ''), '-', ''), ' ', '') = @Cpf
                                 LIMIT 1";
                using var cmd = new SQLiteCommand(query, connection);
                cmd.Parameters.AddWithValue("@Cpf", cpfNormalizado);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string cooperadoId = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                    string nome = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                    string producaoPorCpf = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                    string status = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                    return (cooperadoId, nome, producaoPorCpf, status);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao buscar cooperado local por CPF: {ex.Message}");
            }

            return null;
        }

        public string? BuscarStatusCooperadoLocal(string cooperadoId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cooperadoId))
                    return null;

                using var connection = new SQLiteConnection(connectionString);
                connection.Open();

                string query = "SELECT status FROM cooperados WHERE id = @Id LIMIT 1";
                using var cmd = new SQLiteCommand(query, connection);
                cmd.Parameters.AddWithValue("@Id", cooperadoId);

                var result = cmd.ExecuteScalar();
                return result?.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao buscar status local do cooperado: {ex.Message}");
                return null;
            }
        }

        private static string NormalizarCpf(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return string.Empty;

            var digits = cpf.Where(char.IsDigit).ToArray();
            return new string(digits);
        }

        #endregion

        #region Cooperados Local

        public int SalvarCooperadosEmLote(List<Dictionary<string, object>> rows, List<(string Name, string Type, bool IsPrimaryKey)> columns)
        {
            var columnsClean = columns
                .Where(c => !string.IsNullOrWhiteSpace(c.Name))
                .ToList();

            if (columnsClean.Count == 0)
                return 0;

            try
            {
                using var connection = new SQLiteConnection(connectionString);
                connection.Open();

                CriarOuAtualizarTabelaCooperados(connection, columnsClean);

                if (rows == null || rows.Count == 0)
                    return 0;

                var columnNames = columnsClean.Select(c => c.Name).ToList();
                var insertColumnsSql = string.Join(", ", columnNames.Select(QuoteIdentifier));
                var placeholders = string.Join(", ", columnNames.Select((_, i) => $"@p{i}"));

                var pkColumns = columnsClean.Where(c => c.IsPrimaryKey).Select(c => c.Name).ToList();
                var nonPkColumns = columnNames.Where(c => !pkColumns.Contains(c, StringComparer.OrdinalIgnoreCase)).ToList();

                string upsertSql = string.Empty;
                string insertPrefix = "INSERT INTO";
                if (pkColumns.Count > 0 && nonPkColumns.Count > 0)
                {
                    var conflictCols = string.Join(", ", pkColumns.Select(QuoteIdentifier));
                    var updateSet = string.Join(", ", nonPkColumns.Select(c => $"{QuoteIdentifier(c)} = excluded.{QuoteIdentifier(c)}"));
                    upsertSql = $" ON CONFLICT({conflictCols}) DO UPDATE SET {updateSet}";
                }
                else
                {
                    insertPrefix = "INSERT OR REPLACE INTO";
                }

                int total = 0;
                using var transaction = connection.BeginTransaction();
                foreach (var row in rows)
                {
                    // Busca registro local pelo id (ou cpf)
                    object? idValue = row.ContainsKey("id") ? row["id"] : null;
                    object? cpfValue = row.ContainsKey("cpf") ? row["cpf"] : null;
                    Dictionary<string, object>? local = null;
                    if (idValue != null)
                    {
                        using var cmdLocal = new SQLiteCommand("SELECT * FROM cooperados WHERE id = @id LIMIT 1", connection, transaction);
                        cmdLocal.Parameters.AddWithValue("@id", idValue);
                        using var reader = cmdLocal.ExecuteReader();
                        if (reader.Read())
                        {
                            local = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                                local[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        }
                    }
                    else if (cpfValue != null)
                    {
                        using var cmdLocal = new SQLiteCommand("SELECT * FROM cooperados WHERE cpf = @cpf LIMIT 1", connection, transaction);
                        cmdLocal.Parameters.AddWithValue("@cpf", cpfValue);
                        using var reader = cmdLocal.ExecuteReader();
                        if (reader.Read())
                        {
                            local = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                                local[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        }
                    }

                    bool precisaAtualizar = false;
                    if (local != null)
                    {
                        // Compara campos relevantes
                        foreach (var colName in columnNames)
                        {
                            var novo = row.ContainsKey(colName) ? row[colName]?.ToString() ?? string.Empty : string.Empty;
                            var atual = local.ContainsKey(colName) ? local[colName]?.ToString() ?? string.Empty : string.Empty;
                            if (novo != atual)
                            {
                                precisaAtualizar = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        // Não existe local, precisa inserir
                        precisaAtualizar = true;
                    }

                    if (precisaAtualizar)
                    {
                        var sql = $"{insertPrefix} cooperados ({insertColumnsSql}) VALUES ({placeholders}){upsertSql}";
                        using var cmd = new SQLiteCommand(sql, connection, transaction);
                        for (int i = 0; i < columnNames.Count; i++)
                        {
                            var colName = columnNames[i];
                            if (row != null && row.TryGetValue(colName, out var value))
                                cmd.Parameters.AddWithValue($"@p{i}", NormalizarValorSql(value));
                            else
                                cmd.Parameters.AddWithValue($"@p{i}", DBNull.Value);
                        }
                        cmd.ExecuteNonQuery();
                        total++;
                    }
                }
                transaction.Commit();
                return total;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao salvar cooperados localmente: {ex.Message}");
                return 0;
            }
        }

        private void CriarOuAtualizarTabelaCooperados(SQLiteConnection connection, List<(string Name, string Type, bool IsPrimaryKey)> columns)
        {
            if (!TabelaExiste(connection, "cooperados"))
            {
                var colDefs = columns.Select(c =>
                {
                    var type = NormalizarTipoSql(c.Type);
                    var pk = c.IsPrimaryKey ? " PRIMARY KEY" : string.Empty;
                    return $"{QuoteIdentifier(c.Name)} {type}{pk}";
                });

                var createSql = $"CREATE TABLE IF NOT EXISTS cooperados ({string.Join(", ", colDefs)})";
                using var cmd = new SQLiteCommand(createSql, connection);
                cmd.ExecuteNonQuery();
                DeduplicarCooperadosPorId(connection);
                CreateUniqueIndexIfPossible(connection, "cooperados", "id", "idx_cooperados_id_unique");
                return;
            }

            foreach (var column in columns)
            {
                var type = NormalizarTipoSql(column.Type);
                EnsureColumnExists(connection, "cooperados", column.Name, type);
            }

            DeduplicarCooperadosPorId(connection);
            CreateUniqueIndexIfPossible(connection, "cooperados", "id", "idx_cooperados_id_unique");
        }

        private bool TabelaExiste(SQLiteConnection connection, string tableName)
        {
            var sql = "SELECT name FROM sqlite_master WHERE type='table' AND name=@name";
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@name", tableName);
            var result = cmd.ExecuteScalar();
            return result != null && !string.IsNullOrWhiteSpace(result.ToString());
        }

        private static string NormalizarTipoSql(string? type)
        {
            return string.IsNullOrWhiteSpace(type) ? "TEXT" : type.Trim();
        }

        private static string QuoteIdentifier(string identifier)
        {
            return "\"" + identifier.Replace("\"", "\"\"") + "\"";
        }

        private void CreateUniqueIndexIfPossible(SQLiteConnection connection, string tableName, string columnName, string indexName)
        {
            try
            {
                if (!ColumnExists(connection, tableName, columnName))
                    return;

                string query = $"CREATE UNIQUE INDEX IF NOT EXISTS {indexName} ON {tableName}({columnName})";
                using var cmd = new SQLiteCommand(query, connection);
                cmd.ExecuteNonQuery();
            }
            catch { /* Ignorar */ }
        }

        private void DeduplicarCooperadosPorId(SQLiteConnection connection)
        {
            try
            {
                if (!TabelaExiste(connection, "cooperados") || !ColumnExists(connection, "cooperados", "id"))
                    return;

                string query = @"
                    DELETE FROM cooperados
                    WHERE rowid NOT IN (
                        SELECT MIN(rowid) FROM cooperados
                        WHERE id IS NOT NULL AND id != ''
                        GROUP BY id
                    )
                    AND id IS NOT NULL AND id != ''
                ";
                using var cmd = new SQLiteCommand(query, connection);
                cmd.ExecuteNonQuery();
            }
            catch { /* Ignorar */ }
        }

        private bool ColumnExists(SQLiteConnection connection, string tableName, string columnName)
        {
            try
            {
                string checkQuery = $"PRAGMA table_info({tableName})";
                using var checkCmd = new SQLiteCommand(checkQuery, connection);
                using var reader = checkCmd.ExecuteReader();

                while (reader.Read())
                {
                    if (reader[1].ToString()?.Equals(columnName, StringComparison.OrdinalIgnoreCase) == true)
                        return true;
                }
            }
            catch { /* Ignorar */ }

            return false;
        }

        private static object NormalizarValorSql(object? value)
        {
            if (value == null)
                return DBNull.Value;

            if (value is bool b)
                return b ? 1 : 0;

            if (value is DateTime dt)
                return dt.ToString("O");

            return value;
        }

        #endregion

        /// <summary>
        /// Verifica se é a primeira instalação (não há biometrias sincronizadas com Turso)
        /// Usa a presença de registros com SyncedToTurso = 1 como marcador de primeira sincronização realizada
        /// </summary>
        public bool EhPrimeiraInstalacao()
        {
            try
            {
                using var connection = new SQLiteConnection(connectionString);
                connection.Open();

                // Verificar se existe alguma biometria já sincronizada (marcas de primeira sincronização realizada)
                string query = "SELECT COUNT(*) FROM Biometrias WHERE SyncedToTurso = 1";
                using var cmd = new SQLiteCommand(query, connection);
                long count = (long)cmd.ExecuteScalar();

                System.Diagnostics.Debug.WriteLine($"[DB] EhPrimeiraInstalacao: Biometrias com SyncedToTurso=1: {count}");

                // Se houver biometrias sincronizadas, não é primeira instalação
                if (count > 0)
                {
                    System.Diagnostics.Debug.WriteLine("[DB] ℹ️ NÃO é primeira instalação (há biometrias sincronizadas)");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine("[DB] ✅ É PRIMEIRA INSTALAÇÃO (nenhuma biometria sincronizada encontrada)");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB] ❌ Erro em EhPrimeiraInstalacao: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[DB]    Stack: {ex.StackTrace}");
                // Em caso de erro, assume que é primeira instalação
                return true;
            }
        }

        // =============== Métodos Assíncronos para Turso ===============

        /// <summary>
        /// Obtém registros de tempo não sincronizados (async)
        /// </summary>
        public async Task<List<TimeRecord>> GetUnsyncedTimeRecordsAsync()
        {
            return await Task.FromResult(GetUnsyncedTimeRecords());
        }

        /// <summary>
        /// Marca um registro de tempo como sincronizado (async)
        /// </summary>
        public async Task<bool> MarkTimeRecordAsSyncedAsync(int id)
        {
            return await Task.FromResult(MarkTimeRecordAsSynced(id));
        }

        /// <summary>
        /// Insere um registro de tempo (wrapper do RegisterTimeRecord)
        /// </summary>
        public bool InsertTimeRecord(TimeRecord record)
        {
            return RegisterTimeRecord(record);
        }

        /// <summary>
        /// Sincroniza biometrias com o banco local a partir de uma lista de empregados
        /// </summary>
        public int SyncBiometries(List<Employee> employees)
        {
            int syncedCount = 0;
            try
            {
                using var connection = new SQLiteConnection(connectionString);
                connection.Open();

                foreach (var employee in employees)
                {
                    if (employee.FingerprintTemplate != null && employee.FingerprintTemplate.Length > 0)
                    {
                        try
                        {
                            string query = @"INSERT OR REPLACE INTO Biometrias (CooperadoId, CooperadoNome, Template, FingerIndex, CreatedAt, SyncedToTurso) 
                                            VALUES (@CooperadoId, @CooperadoNome, @Template, @FingerIndex, @CreatedAt, 1)";
                            
                            using var cmd = new SQLiteCommand(query, connection);
                            cmd.Parameters.AddWithValue("@CooperadoId", employee.Id.ToString());
                            cmd.Parameters.AddWithValue("@CooperadoNome", employee.Name ?? "");
                            cmd.Parameters.AddWithValue("@Template", employee.FingerprintTemplate);
                            cmd.Parameters.AddWithValue("@FingerIndex", 0);
                            cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            
                            if (cmd.ExecuteNonQuery() > 0)
                                syncedCount++;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Erro ao sincronizar biometria do empregado {employee.Name}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao sincronizar biometrias: {ex.Message}");
            }
            return syncedCount;
        }

        /// <summary>
        /// Insere um registro de tempo de forma assíncrona
        /// </summary>
        public async Task<bool> InsertTimeRecordAsync(TimeRecord record)
        {
            try
            {
                return await Task.FromResult(InsertTimeRecord(record));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao inserir registro: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sincroniza biometrias para o banco local de forma assíncrona
        /// </summary>
        public async Task<int> SyncBiometriesAsync(List<Employee> employees)
        {
            return await Task.FromResult(SyncBiometries(employees));
        }
    }
}

