using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Linq;
using System.Globalization;

namespace BiometricSystem.Database
{
    /// <summary>
    /// Gerenciador de conexão com Turso (libSQL/SQLite Cloud) via HTTP API
    /// Documentação: https://docs.turso.tech/
    /// </summary>
    public class TursoDbConnection
    {
        private readonly string _tursoUrl;
        private readonly string _authToken;
        private readonly HttpClient _httpClient;
        private const int MaxRetries = 3;
        private const int RetryDelayMs = 1000;

        public TursoDbConnection(string tursoUrl, string authToken)
        {
            // Converter libsql:// URL para https:// para API HTTP
            _tursoUrl = ConvertTursoUrl(tursoUrl);
            _authToken = authToken ?? throw new ArgumentNullException(nameof(authToken));
            
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        }

        /// <summary>
        /// Converte URL libsql:// para formato https:// para API
        /// </summary>
        private string ConvertTursoUrl(string libsqlUrl)
        {
            if (string.IsNullOrEmpty(libsqlUrl))
                throw new ArgumentNullException(nameof(libsqlUrl));

            // Exemplo: libsql://idev-bd-007-sistemas.aws-us-east-1.turso.io 
            // Resultado: https://idev-bd-007-sistemas.aws-us-east-1.turso.io
            if (libsqlUrl.StartsWith("libsql://"))
            {
                return "https://" + libsqlUrl.Substring(9);
            }
            return libsqlUrl;
        }

        /// <summary>
        /// Executa uma query SQL no Turso com retry automático
        /// </summary>
        public async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string sql, object[]? parameters = null)
        {
            if (string.IsNullOrEmpty(sql))
                throw new ArgumentNullException(nameof(sql));

            int attempts = 0;
            Exception? lastException = null;

            while (attempts < MaxRetries)
            {
                try
                {
                    // Formato correto para API libSQL/Turso: requests/execute/close
                    var payload = new
                    {
                        requests = new object[]
                        {
                            new { type = "execute", stmt = new { sql = sql, args = BuildArgs(parameters) } },
                            new { type = "close" }
                        }
                    };

                    var jsonContent = JsonSerializer.Serialize(payload);
                    System.Diagnostics.Debug.WriteLine($"[TursoDbConnection] Enviando: {jsonContent}");
                    
                    var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                    // Endpoint correto para libSQL via HTTP
                    var requestUrl = $"{_tursoUrl}/v2/pipeline";
                    System.Diagnostics.Debug.WriteLine($"[TursoDbConnection] URL: {requestUrl}");
                    
                    var response = await _httpClient.PostAsync(requestUrl, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine(
                            $"[TursoDbConnection] HTTP Error {response.StatusCode}: {errorContent}");
                        throw new InvalidOperationException(
                            $"Turso API retornou erro HTTP {response.StatusCode}: {errorContent}");
                    }

                    var responseContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[TursoDbConnection] Resposta: {responseContent}");
                    return ParseQueryResults(responseContent);
                }
                catch (HttpRequestException httpEx)
                {
                    // Erro de conectividade
                    lastException = httpEx;
                    attempts++;
                    
                    System.Diagnostics.Debug.WriteLine(
                        $"[TursoDbConnection] Erro HTTP na tentativa {attempts}/{MaxRetries}: {httpEx.Message}");

                    if (attempts < MaxRetries)
                    {
                        await Task.Delay(RetryDelayMs * attempts);
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    attempts++;
                    
                    System.Diagnostics.Debug.WriteLine(
                        $"[TursoDbConnection] Erro na tentativa {attempts}/{MaxRetries}: {ex.Message}");

                    if (attempts < MaxRetries)
                    {
                        await Task.Delay(RetryDelayMs * attempts);
                    }
                }
            }

            throw new InvalidOperationException(
                $"Não foi possível conectar ao Turso após {MaxRetries} tentativas. " +
                $"Verifique:\n" +
                $"1. Token de autenticação está correto?\n" +
                $"2. URL do Turso está correta? ({_tursoUrl})\n" +
                $"3. Há conectividade com a internet?\n" +
                $"4. O banco de dados existe no Turso?\n\n" +
                $"Erro: {lastException?.Message}", 
                lastException);
        }

        /// <summary>
        /// Executa um comando INSERT/UPDATE/DELETE no Turso
        /// </summary>
        public async Task<int> ExecuteNonQueryAsync(string sql, object[]? parameters = null)
        {
            if (string.IsNullOrEmpty(sql))
                throw new ArgumentNullException(nameof(sql));

            int attempts = 0;
            Exception? lastException = null;

            while (attempts < MaxRetries)
            {
                try
                {
                    var payload = new
                    {
                        requests = new object[]
                        {
                            new { type = "execute", stmt = new { sql = sql, args = BuildArgs(parameters) } },
                            new { type = "close" }
                        }
                    };

                    var jsonContent = JsonSerializer.Serialize(payload);
                    var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                    var requestUrl = $"{_tursoUrl}/v2/pipeline";
                    var response = await _httpClient.PostAsync(requestUrl, content);

                    var responseContent = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException($"Turso API error: {response.StatusCode} - {responseContent}");
                    }

                    return ParseAffectedRowCount(responseContent);
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    attempts++;

                    System.Diagnostics.Debug.WriteLine(
                        $"[TursoDbConnection] ExecuteNonQuery Tentativa {attempts}/{MaxRetries} falhou: {ex.Message}");

                    if (attempts < MaxRetries)
                    {
                        await Task.Delay(RetryDelayMs * attempts);
                    }
                }
            }

            var lastMessage = lastException?.Message ?? "(sem detalhes)";
            throw new InvalidOperationException(
                $"Falha ao executar comando após {MaxRetries} tentativas. {lastMessage}",
                lastException);
        }

        /// <summary>
        /// Testa a conexão com Turso
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var results = await ExecuteQueryAsync("SELECT 1 as test");
                return results != null && results.Count > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TursoDbConnection] Teste de conexão falhou: {ex.Message}");
                return false;
            }
        }

        private static List<object> BuildArgs(object[]? parameters)
        {
            var args = new List<object>();
            if (parameters == null || parameters.Length == 0)
                return args;

            foreach (var param in parameters)
            {
                args.Add(BuildArgValue(param));
            }

            return args;
        }

        private static object BuildArgValue(object value)
        {
            if (value == null || value is DBNull)
                return new { type = "null" };

            if (value is byte[] bytes)
                return new { type = "blob", base64 = Convert.ToBase64String(bytes) };

            if (value is bool boolVal)
                return new { type = "integer", value = boolVal ? "1" : "0" };

            if (value is sbyte || value is byte || value is short || value is ushort || value is int || value is uint || value is long || value is ulong)
                return new { type = "integer", value = Convert.ToString(value, CultureInfo.InvariantCulture) };

            if (value is float || value is double || value is decimal)
                return new { type = "float", value = Convert.ToString(value, CultureInfo.InvariantCulture) };

            return new { type = "text", value = value.ToString() ?? string.Empty };
        }

        private static int ParseAffectedRowCount(string jsonResponse)
        {
            if (string.IsNullOrWhiteSpace(jsonResponse))
                return 0;

            try
            {
                using var doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;
                if (root.TryGetProperty("results", out var resultsArray) && resultsArray.ValueKind == JsonValueKind.Array)
                {
                    var first = resultsArray.EnumerateArray().FirstOrDefault();
                    if (first.ValueKind == JsonValueKind.Undefined)
                        return 0;

                    var resultRoot = first;
                    if (first.TryGetProperty("response", out var responseObj) && responseObj.ValueKind == JsonValueKind.Object)
                    {
                        if (responseObj.TryGetProperty("result", out var resultObj) && resultObj.ValueKind == JsonValueKind.Object)
                            resultRoot = resultObj;
                    }

                    if (resultRoot.TryGetProperty("affected_row_count", out var affected))
                    {
                        if (affected.ValueKind == JsonValueKind.Number && affected.TryGetInt32(out var count))
                            return count;
                        if (affected.ValueKind == JsonValueKind.String && int.TryParse(affected.GetString(), out count))
                            return count;
                    }
                }
            }
            catch
            {
                // Ignorar parsing e retornar 0
            }

            return 0;
        }

        /// <summary>
        /// Parseia resultados de query JSON da API do Turso
        /// </summary>
        private List<Dictionary<string, object>> ParseQueryResults(string jsonResponse)
        {
            var results = new List<Dictionary<string, object>>();
            
            if (string.IsNullOrEmpty(jsonResponse))
                return results;

            try
            {
                using var doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;

                // Tenta diferentes formatos de resposta do Turso
                // Formato 1: { "results": [ { "rows": [...] } ] }
                if (root.TryGetProperty("results", out var resultsArray) && resultsArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var statement in resultsArray.EnumerateArray())
                    {
                        var statementRoot = statement;
                        if (statement.TryGetProperty("response", out var responseObj) && responseObj.ValueKind == JsonValueKind.Object)
                        {
                            if (responseObj.TryGetProperty("result", out var resultObj) && resultObj.ValueKind == JsonValueKind.Object)
                            {
                                statementRoot = resultObj;
                            }
                        }

                        if (statementRoot.TryGetProperty("rows", out var rows) && rows.ValueKind == JsonValueKind.Array)
                        {
                            // Tentar obter colunas também
                            var columns = new List<string>();
                            if (statementRoot.TryGetProperty("cols", out var colArray) && colArray.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var col in colArray.EnumerateArray())
                                {
                                    if (col.ValueKind == JsonValueKind.Object && col.TryGetProperty("name", out var nameProp))
                                        columns.Add(nameProp.GetString() ?? string.Empty);
                                    else if (col.ValueKind == JsonValueKind.String)
                                        columns.Add(col.GetString() ?? string.Empty);
                                }
                            }
                            else if (statementRoot.TryGetProperty("columns", out colArray) && colArray.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var col in colArray.EnumerateArray())
                                {
                                    if (col.ValueKind == JsonValueKind.Object && col.TryGetProperty("name", out var nameProp))
                                    {
                                        var name = nameProp.GetString();
                                        if (!string.IsNullOrWhiteSpace(name))
                                            columns.Add(name);
                                    }
                                    else if (col.ValueKind == JsonValueKind.String)
                                    {
                                        var name = col.GetString();
                                        if (!string.IsNullOrWhiteSpace(name))
                                            columns.Add(name);
                                    }
                                }
                            }

                            foreach (var row in rows.EnumerateArray())
                            {
                                var dict = new Dictionary<string, object>();

                                if (row.ValueKind == JsonValueKind.Array)
                                {
                                    // Rows como arrays (correspondência com columns)
                                    var rowArray = row.EnumerateArray().ToList();
                                    if (columns.Count == 0)
                                    {
                                        for (int i = 0; i < rowArray.Count; i++)
                                        {
                                            var value = ConvertJsonValue(rowArray[i]);
                                            if (value != null)
                                                dict[$"col_{i}"] = value;
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < columns.Count && i < rowArray.Count; i++)
                                        {
                                            var colName = columns[i];
                                            if (string.IsNullOrWhiteSpace(colName))
                                                colName = $"col_{i}";
                                            var value = ConvertJsonValue(rowArray[i]);
                                            if (value != null)
                                                dict[colName] = value;
                                        }
                                    }
                                }
                                else if (row.ValueKind == JsonValueKind.Object)
                                {
                                    // Rows como objetos
                                    foreach (var prop in row.EnumerateObject())
                                    {
                                        var value = ConvertJsonValue(prop.Value);
                                        if (value != null)
                                            dict[prop.Name] = value;
                                    }
                                }

                                if (dict.Count > 0)
                                    results.Add(dict);
                            }
                        }
                    }
                }
                // Formato 2: Resposta direta como array
                else if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in root.EnumerateArray())
                    {
                        var dict = new Dictionary<string, object>();
                        foreach (var prop in item.EnumerateObject())
                        {
                            var value = ConvertJsonValue(prop.Value);
                            if (value != null)
                                dict[prop.Name] = value;
                        }
                        if (dict.Count > 0)
                            results.Add(dict);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TursoDbConnection] Erro ao parsear resposta JSON: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Resposta: {jsonResponse}");
            }

            return results;
        }

        /// <summary>
        /// Converte JsonElement para tipo apropriado
        /// </summary>
        private object? ConvertJsonValue(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty("type", out _))
            {
                return ConvertLibsqlValue(element);
            }

            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString() ?? "",
                JsonValueKind.Number => element.TryGetInt32(out int intVal) ? intVal : element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => element.GetRawText()
            };
        }

        private object? ConvertLibsqlValue(JsonElement element)
        {
            var type = element.GetProperty("type").GetString() ?? "";

            switch (type)
            {
                case "null":
                    return null;
                case "integer":
                    if (element.TryGetProperty("value", out var intVal))
                    {
                        if (intVal.ValueKind == JsonValueKind.String && long.TryParse(intVal.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var l))
                            return l;
                        if (intVal.ValueKind == JsonValueKind.Number && intVal.TryGetInt64(out var l2))
                            return l2;
                    }
                    return 0L;
                case "float":
                    if (element.TryGetProperty("value", out var floatVal))
                    {
                        if (floatVal.ValueKind == JsonValueKind.String && double.TryParse(floatVal.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                            return d;
                        if (floatVal.ValueKind == JsonValueKind.Number)
                            return floatVal.GetDouble();
                    }
                    return 0d;
                case "text":
                    return element.TryGetProperty("value", out var textVal) ? textVal.GetString() ?? string.Empty : string.Empty;
                case "blob":
                    if (element.TryGetProperty("base64", out var blobVal))
                    {
                        var b64 = blobVal.GetString();
                        if (!string.IsNullOrEmpty(b64))
                            return Convert.FromBase64String(b64);
                    }
                    return Array.Empty<byte>();
                default:
                    return element.GetRawText();
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
