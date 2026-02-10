using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BiometricSystem.Database;
using BiometricSystem.Models;
using Microsoft.Extensions.Configuration;

namespace BiometricSystem.Services
{
    /// <summary>
    /// Serviço de sincronização com Turso (libSQL/SQLite Cloud)
    /// Substituiu NeonSyncService pela nova arquitetura
    /// </summary>
    public class SyncService
    {
        private readonly DatabaseHelper _localDb;
        private readonly IConfiguration _configuration;
        private System.Threading.Timer _syncTimer;
        private int _failureCount = 0;
        private bool _isRunning = false;
        private DateTime _lastSyncTime = DateTime.MinValue;

        private const int MIN_SYNC_INTERVAL_MS = 60000; // 60 segundos
        private const int MAX_RETRY_FAILURES = 5;
        private const int INITIAL_RETRY_DELAY_MS = 2000;

        public SyncService(DatabaseHelper localDb, IConfiguration configuration)
        {
            _localDb = localDb;
            _configuration = configuration;
        }

        /// <summary>
        /// Inicia o serviço de sincronização automática
        /// </summary>
        public void StartSync()
        {
            if (_isRunning)
                return;

            _isRunning = true;
            _failureCount = 0;

            // Sincroniza imediatamente na primeira execução
            _ = Task.Run(async () => await SyncAsync());

            // Timer para sincronização periódica (mínimo 60 segundos)
            _syncTimer = new System.Threading.Timer(async (state) =>
            {
                // Verifica intervalo mínimo entre sincronizações
                if ((DateTime.UtcNow - _lastSyncTime).TotalMilliseconds >= MIN_SYNC_INTERVAL_MS)
                {
                    await SyncAsync();
                }
            }, null, MIN_SYNC_INTERVAL_MS, MIN_SYNC_INTERVAL_MS);
        }

        /// <summary>
        /// Para o serviço de sincronização
        /// </summary>
        public void StopSync()
        {
            _isRunning = false;
            _syncTimer?.Dispose();
            _syncTimer = null;
        }

        /// <summary>
        /// Executa sincronização imediatamente
        /// </summary>
        public async Task SyncNowAsync()
        {
            if (_isRunning)
            {
                await SyncAsync();
            }
        }

        /// <summary>
        /// Sincroniza registros pendentes com Turso
        /// </summary>
        private async Task SyncAsync()
        {
            try
            {
                _lastSyncTime = DateTime.UtcNow;

                // Se muitas falhas, aguarda antes de tentar novamente
                if (_failureCount >= MAX_RETRY_FAILURES)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[Sync] Sincronização pausada. {_failureCount} falhas consecutivas.");
                    return;
                }

                // TODO: Implementar sincronização com TursoHelper quando disponível
                // Por enquanto, apenas usa DatabaseHelper local
                System.Diagnostics.Debug.WriteLine("[Sync] Usando banco de dados local (DatabaseHelper)");

                _failureCount = 0; // Reset contador de falhas

                // TODO: Implementar sincronização de biometrias não sincronizadas
                // var pendingBiometries = _localDb.GetUnsyncedBiometries();
                // if (pendingBiometries.Count > 0)
                // {
                //     System.Diagnostics.Debug.WriteLine(
                //         $"[Sync] {pendingBiometries.Count} biometrias aguardando sincronização");
                // }
            }
            catch (Exception ex)
            {
                _failureCount++;
                System.Diagnostics.Debug.WriteLine($"[Sync] Erro: {ex.Message}");

                // Aplica backoff exponencial
                if (_failureCount < MAX_RETRY_FAILURES)
                {
                    int delayMs = INITIAL_RETRY_DELAY_MS * (int)Math.Pow(2, _failureCount - 1);
                    await Task.Delay(delayMs);
                }
            }
        }

        /// <summary>
        /// Retorna status de sincronização
        /// </summary>
        public SyncStatus GetStatus()
        {
            return new SyncStatus
            {
                IsRunning = _isRunning,
                LastSyncTime = _lastSyncTime,
                FailureCount = _failureCount,
                IsHealthy = _failureCount < MAX_RETRY_FAILURES
            };
        }

        public void Dispose()
        {
            StopSync();
        }
    }

    /// <summary>
    /// Status atual da sincronização
    /// </summary>
    public class SyncStatus
    {
        public bool IsRunning { get; set; }
        public DateTime LastSyncTime { get; set; }
        public int FailureCount { get; set; }
        public bool IsHealthy { get; set; }
    }
}
