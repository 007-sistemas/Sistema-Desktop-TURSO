using System;
using System.Threading.Tasks;

namespace BiometricSystem.Services
{
    /// <summary>
    /// Extensão do FingerprintService para integração com API web
    /// Conecta automaticamente com: https://bypass-lime.vercel.app
    /// </summary>
    public partial class FingerprintService
    {
        private ApiService _apiService;

        /// <summary>
        /// Inicializa o serviço de API
        /// </summary>
        public void InitializeApiService(string apiBaseUrl = "https://bypass-lime.vercel.app")
        {
            _apiService = new ApiService(apiBaseUrl);
            OnStatusChanged?.Invoke(this, $"✓ Conectado ao servidor: {apiBaseUrl}");
        }

        /// <summary>
        /// Registra biometria de um usuário no sistema web
        /// </summary>
        public async Task<bool> RegisterBiometricOnWebAsync(int userId)
        {
            try
            {
                if (_apiService == null)
                {
                    OnStatusChanged?.Invoke(this, "❌ Serviço de API não inicializado");
                    return false;
                }

                if (_capturedFeatures == null)
                {
                    OnStatusChanged?.Invoke(this, "❌ Nenhuma biometria capturada");
                    return false;
                }

                OnStatusChanged?.Invoke(this, "📤 Enviando biometria para servidor...");

                var biometricData = SerializeFeatures(_capturedFeatures);
                var result = await _apiService.RegisterBiometricAsync(userId, biometricData, "fingerprint");

                if (result)
                {
                    OnStatusChanged?.Invoke(this, $"✓ Biometria registrada com sucesso!");
                }
                else
                {
                    OnStatusChanged?.Invoke(this, "❌ Falha ao registrar biometria");
                }

                return result;
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke(this, $"❌ Erro: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verifica e registra producao do usuario no sistema web
        /// </summary>
        public async Task<TimeRecordDto> RegisterTimeRecordOnWebAsync(int userId, string recordType = "entrada")
        {
            try
            {
                if (_apiService == null)
                {
                    OnStatusChanged?.Invoke(this, "❌ Serviço de API não inicializado");
                    return null;
                }

                if (_capturedFeatures == null)
                {
                    OnStatusChanged?.Invoke(this, "❌ Nenhuma biometria capturada");
                    return null;
                }

                OnStatusChanged?.Invoke(this, "🔍 Verificando e registrando produção...");

                var biometricData = SerializeFeatures(_capturedFeatures);
                var result = await _apiService.RegisterTimeRecordAsync(userId, biometricData, recordType);

                if (result != null && result.VerificationSuccess)
                {
                    OnStatusChanged?.Invoke(this, $"✓ Produção registrada: {result.RecordedAt:HH:mm:ss}");
                }
                else
                {
                    OnStatusChanged?.Invoke(this, $"⚠ {result?.Message ?? "Falha ao registrar produção"}");
                }

                return result;
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke(this, $"❌ Erro: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Obtém a lista de usuários cadastrados do servidor web
        /// </summary>
        public async Task<System.Collections.Generic.List<UserDto>> GetUsersFromWebAsync()
        {
            try
            {
                if (_apiService == null)
                {
                    OnStatusChanged?.Invoke(this, "❌ Serviço de API não inicializado");
                    return null;
                }

                OnStatusChanged?.Invoke(this, "📥 Obtendo lista de usuários...");

                var users = await _apiService.GetUsersAsync();

                if (users != null)
                {
                    OnStatusChanged?.Invoke(this, $"✓ {users.Count} usuários obtidos");
                }

                return users;
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke(this, $"❌ Erro: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Verifica se o servidor web está disponível
        /// </summary>
        public async Task<bool> CheckWebServerAvailabilityAsync()
        {
            try
            {
                if (_apiService == null)
                {
                    OnStatusChanged?.Invoke(this, "❌ Serviço de API não inicializado");
                    return false;
                }

                OnStatusChanged?.Invoke(this, "🌐 Verificando servidor...");

                var isAvailable = await _apiService.CheckHealthAsync();

                if (isAvailable)
                {
                    OnStatusChanged?.Invoke(this, "✓ Servidor disponível");
                }
                else
                {
                    OnStatusChanged?.Invoke(this, "⚠ Servidor indisponível");
                }

                return isAvailable;
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke(this, $"❌ Erro: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Serializa as features capturadas para array de bytes
        /// </summary>
        private byte[] SerializeFeatures(DPFP.FeatureSet features)
        {
            try
            {
                if (features == null)
                    return new byte[0];

                using (var stream = new System.IO.MemoryStream())
                {
                    // Serializar features biométricas
                    if (features != null)
                    {
                        // Converter features para bytes usando reflexão
                        var serialized = features.GetType().GetMethod("Serialize", new Type[0])?.Invoke(features, null);
                        return serialized as byte[] ?? new byte[0];
                    }
                    return new byte[0];
                }
            }
            catch
            {
                return new byte[0];
            }
        }
    }
}
