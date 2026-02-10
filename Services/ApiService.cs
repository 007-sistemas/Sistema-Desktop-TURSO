using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BiometricSystem.Services
{
    public class ApiService
    {
        private readonly string _baseUrl;
        private readonly HttpClient _httpClient;

        public ApiService(string baseUrl = "https://bypass-lime.vercel.app")
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// Obtém lista de todos os usuários cadastrados do sistema web
        /// </summary>
        public async Task<List<UserDto>> GetUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/users");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<UserDto>>(json, options);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao obter lista de usuários: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtém um usuário específico pelo ID
        /// </summary>
        public async Task<UserDto> GetUserAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/users/{userId}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<UserDto>(json, options);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao obter usuário {userId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Registra biometria do usuário
        /// </summary>
        public async Task<bool> RegisterBiometricAsync(int userId, byte[] biometricData, string biometricType = "fingerprint")
        {
            try
            {
                var request = new BiometricRegistrationRequest
                {
                    UserId = userId,
                    BiometricData = Convert.ToBase64String(biometricData),
                    BiometricType = biometricType,
                    RegisteredAt = DateTime.UtcNow
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/users/{userId}/biometrics", content);
                response.EnsureSuccessStatusCode();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao registrar biometria para usuário {userId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Registra batida de producao
        /// </summary>
        public async Task<TimeRecordDto> RegisterTimeRecordAsync(int userId, byte[] biometricData, string recordType = "entrada")
        {
            try
            {
                var request = new TimeRecordRequest
                {
                    UserId = userId,
                    BiometricData = Convert.ToBase64String(biometricData),
                    RecordType = recordType,
                    RecordedAt = DateTime.UtcNow,
                    IPAddress = GetLocalIPAddress()
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/timerecords", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<TimeRecordDto>(responseJson, options);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao registrar produção para usuário {userId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Verifica se o servidor web está disponível
        /// </summary>
        public async Task<bool> CheckHealthAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private string GetLocalIPAddress()
        {
            try
            {
                return System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName())[0].ToString();
            }
            catch
            {
                return "127.0.0.1";
            }
        }
    }

    // DTOs para comunicação com a API
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Matricula { get; set; }
        public string Categoria { get; set; }
        public bool IsBiometricRegistered { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class BiometricRegistrationRequest
    {
        public int UserId { get; set; }
        public string BiometricData { get; set; }
        public string BiometricType { get; set; }
        public DateTime RegisteredAt { get; set; }
    }

    public class TimeRecordRequest
    {
        public int UserId { get; set; }
        public string BiometricData { get; set; }
        public string RecordType { get; set; }
        public DateTime RecordedAt { get; set; }
        public string IPAddress { get; set; }
    }

    public class TimeRecordDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string RecordType { get; set; }
        public DateTime RecordedAt { get; set; }
        public bool VerificationSuccess { get; set; }
        public string Message { get; set; }
    }
}
