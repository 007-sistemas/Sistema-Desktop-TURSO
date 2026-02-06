using DPFP;
using DPFP.Capture;
using DPFP.Processing;
using DPFP.Verification;
using System;
using System.IO;

namespace BiometricSystem.Services
{
    /// <summary>
    /// Serviço de biometria usando DigitalPersona One Touch SDK
    /// Implementação direta com as DLLs do SDK do DigitalPersona
    /// </summary>
    public partial class FingerprintService : IDisposable
    {
        private DPFP.Capture.Capture? _capturer;
        private DPFP.Processing.Enrollment? _enroller;
        private DPFP.Verification.Verification? _verificator;
        private bool _isCapturing = false;
        private int _enrollmentSampleCount = 0;
        private bool _isEnrollmentMode = false;
        private DPFP.FeatureSet? _capturedFeatures = null;
        private object _capturerLock = new object();

        public event EventHandler<byte[]>? OnFingerprintCaptured;
        public event EventHandler<string>? OnStatusChanged;
        public event EventHandler<int>? OnEnrollmentProgress;

        public FingerprintService(bool autoInitialize = true)
        {
            if (!autoInitialize)
                return;

            try
            {
                OnStatusChanged?.Invoke(this, "🔄 Inicializando capturador biométrico...");
                InitializeCapturer();
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke(this, $"❌ Erro ao inicializar: {ex.Message}");
            }
        }

        public void InitializeSdk()
        {
            try
            {
                OnStatusChanged?.Invoke(this, "🔄 Inicializando capturador biométrico...");
                InitializeCapturer();
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke(this, $"❌ Erro ao inicializar: {ex.Message}");
            }
        }

        private void InitializeCapturer()
        {
            try
            {
                // Criar instância do capturador do SDK
                _capturer = new DPFP.Capture.Capture();
                _verificator = new DPFP.Verification.Verification();
                
                if (_capturer != null)
                {
                    // Subscrever aos eventos do capturador
                    _capturer.EventHandler = new CaptureEventHandler(this);
                    OnStatusChanged?.Invoke(this, "✅ DigitalPersona SDK integrado com sucesso!");
                    OnStatusChanged?.Invoke(this, "📌 Conecte o leitor biométrico");
                }
                else
                {
                    OnStatusChanged?.Invoke(this, "❌ Falha ao inicializar o capturador");
                }
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke(this, $"❌ Erro na integração SDK: {ex.Message}");
                _capturer = null;
            }
        }

        public bool InitializeReader()
        {
            try
            {
                if (_capturer != null)
                {
                    // Iniciar a captura contínua para detectar quando o leitor é conectado
                    try
                    {
                        _capturer.StartCapture();
                        OnStatusChanged?.Invoke(this, "✅ Leitor biométrico inicializado. Aguardando conexão...");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        // Se a captura falhar, pode ser que o leitor não esteja conectado ainda
                        OnStatusChanged?.Invoke(this, $"⚠️ Leitor inicializado mas não conectado: {ex.Message}");
                        // Retorna true mesmo assim, porque o handler de eventos vai notificar quando conectar
                        return true;
                    }
                }
                OnStatusChanged?.Invoke(this, "❌ Leitor não inicializado - SDK não carregado");
                return false;
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke(this, $"Erro ao inicializar leitor: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> StartCapture()
        {
            try
            {
                lock (_capturerLock)
                {
                    if (_isCapturing)
                        return true;
                    _isCapturing = true;
                }

                if (_capturer == null)
                {
                    OnStatusChanged?.Invoke(this, "❌ Capturador não está inicializado");
                    return false;
                }

                try
                {
                    _capturer.StartCapture();
                    OnStatusChanged?.Invoke(this, "⏳ Posicione o dedo no leitor...");
                    return true;
                }
                catch (Exception ex)
                {
                    OnStatusChanged?.Invoke(this, $"❌ Erro na captura: {ex.Message}");
                    lock (_capturerLock)
                    {
                        _isCapturing = false;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke(this, $"Erro: {ex.Message}");
                return false;
            }
        }

        public void StopCapture()
        {
            lock (_capturerLock)
            {
                _isCapturing = false;
            }

            try
            {
                if (_capturer != null)
                {
                    _capturer.StopCapture();
                }
            }
            catch { }
        }

        public bool StartEnrollment()
        {
            try
            {
                _enrollmentSampleCount = 0;
                _isEnrollmentMode = true;
                _enroller = new DPFP.Processing.Enrollment();
                
                OnStatusChanged?.Invoke(this, "🔄 Modo de Registro Iniciado!");
                OnStatusChanged?.Invoke(this, "📋 Capturando 1/3 amostras...");
                return true;
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke(this, $"Erro ao iniciar registro: {ex.Message}");
                return false;
            }
        }

        public void StopEnrollment()
        {
            _isEnrollmentMode = false;
            _enrollmentSampleCount = 0;
            if (_enroller != null)
            {
                _enroller.Clear();
            }
        }

        public void AddEnrollmentSample(byte[] templateBytes)
        {
            // Este método é chamado pelo event handler
        }

        public byte[]? GetEnrollmentTemplate()
        {
            return null;
        }

        public bool CompareFingerprints(byte[]? template1, byte[]? template2)
        {
            if (template1 == null || template2 == null)
            {
                OnStatusChanged?.Invoke(this, "❌ Templates inválidos");
                return false;
            }

            try
            {
                // Comparação direta: templates devem ser iguais
                if (template1.SequenceEqual(template2))
                {
                    OnStatusChanged?.Invoke(this, "✅ Similaridade: 100% - Correspondência Exata (SDK)");
                    return true;
                }
                else
                {
                    OnStatusChanged?.Invoke(this, "❌ Similaridade: 0% - Não correspondência");
                    return false;
                }
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke(this, $"Erro na comparação: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verifica as features capturadas contra um template armazenado usando o verificador nativo
        /// Implementação baseada no repositório: Scanning1102/Ejemplo (FrmChecador.cs)
        /// NÃO limpa as features capturadas
        /// </summary>
        public bool VerifyAgainstTemplate(byte[] templateBytes)
        {
            if (_capturedFeatures == null || templateBytes == null || templateBytes.Length == 0)
            {
                OnStatusChanged?.Invoke(this, "❌ Dados insuficientes para verificação");
                return false;
            }

            try
            {
                if (_verificator == null)
                {
                    _verificator = new DPFP.Verification.Verification();
                }

                // Desserializar o template armazenado (igual ao FrmChecador.cs)
                using (MemoryStream stream = new MemoryStream(templateBytes))
                {
                    DPFP.Template storedTemplate = new DPFP.Template(stream);
                    
                    // Criar resultado de verificação
                    DPFP.Verification.Verification.Result result = new DPFP.Verification.Verification.Result();

                    // Verificar as features contra o template com o verificador nativo
                    _verificator.Verify(_capturedFeatures, storedTemplate, ref result);

                    return result.Verified;
                }
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke(this, $"Erro na verificação: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Limpa as features capturadas após a verificação
        /// </summary>
        public void ClearCapturedFeatures()
        {
            _capturedFeatures = null;
        }

        public void Dispose()
        {
            StopCapture();
            
            try
            {
                if (_capturer != null)
                {
                    _capturer.StopCapture();
                    _capturer = null;
                }
            }
            catch { }
        }

        /// <summary>
        /// Manipulador de eventos do capturador do SDK
        /// </summary>
        private class CaptureEventHandler : DPFP.Capture.EventHandler
        {
            private FingerprintService _service;

            public CaptureEventHandler(FingerprintService service)
            {
                _service = service;
            }

            public void OnComplete(object Capture, string ReaderSerialNumber, DPFP.Sample Sample)
            {
                _service.OnStatusChanged?.Invoke(_service, "✓ Digital capturada");
                ProcessSample(Sample);
            }

            public void OnFingerGone(object Capture, string ReaderSerialNumber)
            {
                _service.OnStatusChanged?.Invoke(_service, "👆 Remova o dedo do leitor");
            }

            public void OnFingerTouch(object Capture, string ReaderSerialNumber)
            {
                _service.OnStatusChanged?.Invoke(_service, "👉 Dedo detectado");
            }

            public void OnReaderConnect(object Capture, string ReaderSerialNumber)
            {
                _service.OnStatusChanged?.Invoke(_service, $"✅ Leitor conectado");
            }

            public void OnReaderDisconnect(object Capture, string ReaderSerialNumber)
            {
                _service.OnStatusChanged?.Invoke(_service, $"❌ Leitor desconectado");
            }

            public void OnSampleQuality(object Capture, string ReaderSerialNumber, DPFP.Capture.CaptureFeedback CaptureFeedback)
            {
                if (CaptureFeedback == DPFP.Capture.CaptureFeedback.Good)
                    _service.OnStatusChanged?.Invoke(_service, "📸 Qualidade: Excelente");
                else
                    _service.OnStatusChanged?.Invoke(_service, $"⚠️ Qualidade baixa");
            }

            private void ProcessSample(DPFP.Sample Sample)
            {
                try
                {
                    // Extrair features com o propósito correto
                    DPFP.Processing.FeatureExtraction extractor = new DPFP.Processing.FeatureExtraction();
                    DPFP.Capture.CaptureFeedback feedback = DPFP.Capture.CaptureFeedback.None;
                    DPFP.FeatureSet features = new DPFP.FeatureSet();
                    
                    // Usar DataPurpose correto
                    DPFP.Processing.DataPurpose purpose = _service._isEnrollmentMode 
                        ? DPFP.Processing.DataPurpose.Enrollment 
                        : DPFP.Processing.DataPurpose.Verification;
                    
                    extractor.CreateFeatureSet(Sample, purpose, ref feedback, ref features);

                    if (feedback == DPFP.Capture.CaptureFeedback.Good && features != null)
                    {
                        // ENROLLMENT MODE - Coletar 3 amostras
                        if (_service._isEnrollmentMode && _service._enroller != null)
                        {
                            _service._enroller.AddFeatures(features);
                            _service._enrollmentSampleCount++;
                            
                            int progress = (_service._enrollmentSampleCount / 3) * 100;
                            _service.OnEnrollmentProgress?.Invoke(_service, progress);
                            _service.OnStatusChanged?.Invoke(_service, $"✓ Amostra {_service._enrollmentSampleCount}/3 capturada");

                            // Verificar status do enrollment
                            switch (_service._enroller.TemplateStatus)
                            {
                                case DPFP.Processing.Enrollment.Status.Ready:
                                    byte[] templateBytes = _service._enroller.Template.Bytes;
                                    _service.OnFingerprintCaptured?.Invoke(_service, templateBytes);
                                    _service.OnStatusChanged?.Invoke(_service, "✅ Digital registrada com sucesso!");
                                    _service._isEnrollmentMode = false;
                                    _service._enroller.Clear();
                                    _service._enrollmentSampleCount = 0;
                                    _service.StopCapture();
                                    break;

                                case DPFP.Processing.Enrollment.Status.Failed:
                                    _service.OnStatusChanged?.Invoke(_service, "❌ Falha no registro. Tente novamente");
                                    _service._enroller.Clear();
                                    _service._enrollmentSampleCount = 0;
                                    break;
                            }
                        }
                        // VERIFICATION MODE - Capturar features uma vez e armazenar
                        else if (!_service._isEnrollmentMode)
                        {
                            _service._capturedFeatures = features;
                            _service.OnStatusChanged?.Invoke(_service, "✅ Digital capturada! Comparando...");
                            _service.OnFingerprintCaptured?.Invoke(_service, new byte[0]); // Sinal para prosseguir
                            _service.StopCapture();
                        }
                    }
                    else
                    {
                        _service.OnStatusChanged?.Invoke(_service, "⚠️ Qualidade baixa. Tente novamente");
                    }
                }
                catch (Exception ex)
                {
                    _service.OnStatusChanged?.Invoke(_service, $"Erro: {ex.Message}");
                }
            }
        }
    }
}
