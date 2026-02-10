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
    /// Servico de sincronizacao com Turso (libSQL/SQLite Cloud)
    /// Substitui NeonSyncService pela nova arquitetura
    /// </summary>
    public class TursoSyncService
    {
        private readonly TursoHelper? _tursoHelper;
        private readonly DatabaseHelper _localDb;
        private readonly IConfiguration _configuration;
        private System.Threading.Timer _syncTimer;
        private readonly bool _canSync;
        private int _failureCount = 0;
        private bool _isRunning = false;
        private DateTime _lastSyncTime = DateTime.MinValue;

        private const int MIN_SYNC_INTERVAL_MS = 60000; // 60 segundos
        private const int MAX_RETRY_FAILURES = 5;
        private const int INITIAL_RETRY_DELAY_MS = 2000;

        public TursoSyncService(DatabaseHelper localDb, IConfiguration configuration)
        {
            _localDb = localDb;
            _configuration = configuration;

            var tursoUrl = configuration["TursoDb:Url"]
                ?? configuration["ConnectionStrings:TursoDb:Url"]
                ?? string.Empty;
            var authToken = configuration["TursoDb:AuthToken"]
                ?? configuration["ConnectionStrings:TursoDb:AuthToken"]
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(tursoUrl) || string.IsNullOrWhiteSpace(authToken))
            {
                _canSync = false;
                System.Diagnostics.Debug.WriteLine("[Turso Sync] Config ausente. Sincronizacao desativada.");
                return;
            }

            _tursoHelper = new TursoHelper(tursoUrl, authToken, localDb);
            _canSync = true;
        }

        /// <summary>
        /// Inicia o servico de sincronizacao automatica
        /// </summary>
        public void StartSync()
        {
            if (_isRunning || !_canSync)
                return;

            _isRunning = true;
            _failureCount = 0;

            // Sincroniza imediatamente na primeira execucao
            _ = Task.Run(async () => await SyncAsync());

            // Timer para sincronizacao periodica (minimo 60 segundos)
            _syncTimer = new System.Threading.Timer(async (state) =>
            {
                // Verifica intervalo minimo entre sincronizacoes
                if ((DateTime.UtcNow - _lastSyncTime).TotalMilliseconds >= MIN_SYNC_INTERVAL_MS)
                {
                    await SyncAsync();
                }
            }, null, MIN_SYNC_INTERVAL_MS, MIN_SYNC_INTERVAL_MS);
        }

        /// <summary>
        /// Para o servico de sincronizacao
        /// </summary>
        public void StopSync()
        {
            _isRunning = false;
            _syncTimer?.Dispose();
            _syncTimer = null;
        }

        /// <summary>
        /// Executa sincronizacao imediatamente
        /// </summary>
        public async Task SyncNowAsync()
        {
            if (!_canSync)
            {
                System.Diagnostics.Debug.WriteLine("[Turso Sync] SyncNow ignorado: configuracao ausente.");
                return;
            }

            if (!_isRunning)
            {
                System.Diagnostics.Debug.WriteLine("[Turso Sync] SyncNow executando mesmo com servico parado.");
            }

            await SyncAsync();
        }

        /// <summary>
        /// Sincroniza registros pendentes com Turso
        /// </summary>
        private async Task SyncAsync()
        {
            try
            {
                if (!_canSync || _tursoHelper == null)
                    return;

                _lastSyncTime = DateTime.UtcNow;

                // Se muitas falhas, aguarda antes de tentar novamente
                if (_failureCount >= MAX_RETRY_FAILURES)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[Turso Sync] Sincronizacao pausada. {_failureCount} falhas consecutivas.");
                    return;
                }

                // Verifica conexao com Turso
                bool connected = await _tursoHelper.TestConnectionAsync();
                if (!connected)
                {
                    _failureCount++;
                    System.Diagnostics.Debug.WriteLine(
                        $"[Turso Sync] Falha na conexao. Tentativa {_failureCount}/{MAX_RETRY_FAILURES}");
                    return;
                }

                _failureCount = 0; // Reset contador de falhas

                // Sincroniza registros pendentes
                int syncedRecords = await _tursoHelper.SyncPendingRecordsAsync();
                System.Diagnostics.Debug.WriteLine($"[Turso Sync] {syncedRecords} registros sincronizados");

            }
            catch (Exception ex)
            {
                _failureCount++;
                System.Diagnostics.Debug.WriteLine($"[Turso Sync] Erro: {ex.Message}");

                // Aplica backoff exponencial
                if (_failureCount < MAX_RETRY_FAILURES)
                {
                    int delayMs = INITIAL_RETRY_DELAY_MS * (int)Math.Pow(2, _failureCount - 1);
                    await Task.Delay(delayMs);
                }
            }
        }

        /// <summary>
        /// Retorna status de sincronizacao
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

}
