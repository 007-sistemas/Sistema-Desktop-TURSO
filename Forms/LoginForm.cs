

using BiometricSystem.Database;
using BiometricSystem.Models;
using BiometricSystem.Services;
using System.Globalization;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace BiometricSystem.Forms
{
        public partial class LoginForm : Form
        {
        private readonly IConfiguration? _config;
            // Constantes para bloquear movimentação
            private const int WM_NCLBUTTONDOWN = 0xA1;
            private const int HTCAPTION = 0x2;
        // Guardar tamanhos originais para restaurar
        private float fonteOriginalHeader;
        private float fonteOriginalData;
        private float fonteOriginalTitulo;
        private float fonteOriginalInstrucao;
        private float fonteOriginalStatus;
        private Size tamanhoOriginalPanelHeader;
        private Size tamanhoOriginalPanelSimulador;
        private Size tamanhoOriginalPanelStatusBar;

        private readonly FingerprintService fingerprintService;
        private readonly DatabaseHelper database;
        private readonly TursoSyncService? syncService;
        private string? tursoConnectionString;
        private TursoDbConnection? _tursoConnection;
        private string? selectedSetor;
        private int? selectedSetorId;
        private bool isCapturing = false;
        private bool _readerInitialized = false;
        private bool _biometriaSyncDone = false;
        private bool _biometriaSyncInProgress = false;
        private bool _isVerifying = false;
        private bool? _setoresSyncOk = null;
        public bool VoltarDaProducao { get; set; } = false;
        private string? hospitalId;
        private string? hospitalNome;
        private string? hospitalCodigo;
        private System.Windows.Forms.Timer? clearPanelTimer; // Timer para limpar painel após registro
        public bool AllowClose { get; set; } = false; // Controla se pode fechar realmente
        private readonly object _statusLock = new object();
        private DateTime _lastStatusUpdate = DateTime.MinValue;
        private string? _lastStatusText = null;

        public LoginForm(IConfiguration? config = null)
        {
            _config = config;
            database = new DatabaseHelper();
            // Solicitar cadastro de senha local se ainda não existir
            if (!database.ExisteSenhaLocal())
            {
                using (var senhaForm = new CadastroSenhaLocalForm())
                {
                    while (true)
                    {
                        var result = senhaForm.ShowDialog();
                        if (result == DialogResult.OK)
                        {
                            if (string.IsNullOrWhiteSpace(senhaForm.Senha) || senhaForm.Senha.Length < 4)
                            {
                                MessageBox.Show("A senha deve ter pelo menos 4 caracteres.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                continue;
                            }
                            if (senhaForm.Senha != senhaForm.Confirmacao)
                            {
                                MessageBox.Show("As senhas não coincidem.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                continue;
                            }
                            if (database.SalvarSenhaLocal(senhaForm.Senha))
                            {
                                MessageBox.Show("Senha local cadastrada com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            }
                            else
                            {
                                MessageBox.Show("Erro ao salvar a senha local.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                continue;
                            }
                        }
                        else
                        {
                            MessageBox.Show("O cadastro da senha local é obrigatório para uso do sistema.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            InitializeComponent();
            // NÃO forçar FormBorderStyle=None aqui, para permitir o X
            this.StartPosition = FormStartPosition.Manual;
            this.Bounds = Screen.FromHandle(this.Handle).Bounds;
            this.TopMost = true;
            // Impede redimensionamento e mantém o X
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            // Salvar tamanhos e fontes originais após InitializeComponent
            fonteOriginalHeader = lblTime.Font.Size;
            fonteOriginalData = lblDate.Font.Size;
            fonteOriginalTitulo = lblLocalProducao.Font.Size;
            fonteOriginalInstrucao = lblInstrucao.Font.Size;
            fonteOriginalStatus = lblStatus.Font.Size;
            tamanhoOriginalPanelHeader = panelHeader.Size;
            tamanhoOriginalPanelSimulador = panelSimulador.Size;
            tamanhoOriginalPanelStatusBar = panelStatusBar.Size;

            // Adaptação dinâmica para telas pequenas
            this.Resize += (s, e) => AdaptarParaTelaPequena();
            AdaptarParaTelaPequena();

            fingerprintService = new FingerprintService(autoInitialize: false);
            database = new DatabaseHelper();
            
            // Inicializar timer para limpeza de painel
            clearPanelTimer = new System.Windows.Forms.Timer();
            clearPanelTimer.Tick += (sender, e) =>
            {
                try
                {
                    LogToFile($"⏰ Timer disparado - limpando painel");
                    clearPanelTimer.Stop();
                    
                    panelSimulador.BackColor = System.Drawing.Color.White;
                    lblSimulador.Text = "";
                    lblSimulador.Font = new System.Drawing.Font("Segoe UI", 12F);
                    lblSimulador.TextAlign = System.Drawing.ContentAlignment.TopLeft;
                    lblStatus.Text = "Selecione o setor para ativar o leitor";
                    
                    LogToFile($"⏰ Painel limpo com sucesso");
                }
                catch (Exception ex)
                {
                    LogToFile($"❌ Erro ao limpar painel: {ex.Message}");
                }
            };

            // Carregar configuração do hospital a partir da config
            if (config != null)
            {
                tursoConnectionString = config.GetConnectionString("TursoConnection");
                
                // Carregar configuração do hospital
                hospitalId = config["Hospital:Id"];
                hospitalNome = config["Hospital:Nome"];
                hospitalCodigo = config["Hospital:Codigo"];

                var tursoUrl = config["TursoDb:Url"] ?? string.Empty;
                var authToken = config["TursoDb:AuthToken"] ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(tursoUrl) && !string.IsNullOrWhiteSpace(authToken))
                {
                    _tursoConnection = new TursoDbConnection(tursoUrl, authToken);
                }
            }

            // Sincronização inicial de biometrias será acionada na primeira seleção de setor
            _biometriaSyncDone = !database.EhPrimeiraInstalacao();
            
            // Sincronização em background com Turso (pontos/biometrias)
            if (_config != null)
            {
                syncService = new TursoSyncService(database, _config);
                Task.Run(() => syncService.StartSync());
            }

            // Atualizar label com nome do hospital
            if (!string.IsNullOrEmpty(hospitalNome))
            {
                lblLocalProducao.Text = $"🏥 {hospitalNome}";
            }

            // Configurar eventos do serviço biométrico
            fingerprintService.OnStatusChanged += (sender, status) =>
            {
                try
                {
                    var now = DateTime.UtcNow;
                    lock (_statusLock)
                    {
                        if (_lastStatusText == status && (now - _lastStatusUpdate).TotalMilliseconds < 500)
                            return;
                        _lastStatusText = status;
                        _lastStatusUpdate = now;
                    }

                    if (InvokeRequired)
                    {
                        BeginInvoke(() => lblStatus.Text = status);
                    }
                    else
                    {
                        lblStatus.Text = status;
                    }
                }
                catch { /* ignorar */ }
            };

            fingerprintService.OnFingerprintCaptured += OnFingerprintCaptured;
            
            // Sincronizar setores do Turso ANTES de carregar (como GitHub)
            _ = Task.Run(async () =>
            {
                await SyncSetoresFromTursoAsync();
                await CarregarSetoresDoHospitalAsync();
            });
            
            // Sincronizacao automatica ao abrir pela primeira vez (depois de mostrar a tela)
            Shown += LoginForm_Shown;
            
            // Leitor será inicializado apenas após seleção do setor
            lblStatus.Text = "Carregando setores...";

            // Atualizar relógio
            UpdateClock();
            
            // Centralizar controles ao carregar
            CentralizarControles();
            
            // Aplicar bordas arredondadas
            AplicarBordasArredondadas();
        }

        private async void LoginForm_Shown(object? sender, EventArgs e)
        {
            await Task.Delay(500); // Aguardar interface carregar

            if (_biometriaSyncDone || _biometriaSyncInProgress)
                return;

            _biometriaSyncInProgress = true;
            cmbSetor.Enabled = false;
            lblStatus.Text = "Sincronizando biometrias...";

            try
            {
                await ExecutarSincronizacaoInicial();
                _biometriaSyncDone = true;
            }
            catch (Exception ex)
            {
                LogToFile($"[AUTO-SYNC] ❌ Erro na sincronização automática: {ex.Message}");
            }
            finally
            {
                _biometriaSyncInProgress = false;
                cmbSetor.Enabled = true;
                lblStatus.Text = "Selecione o setor para ativar o leitor";
            }
        }

        // Método para adaptar dinamicamente para telas pequenas
        private void AdaptarParaTelaPequena()
        {
            // Sempre adapta para a área útil da tela
            // Defina o limite de altura considerado "pequeno"
            int limiteAltura = 700;
            bool telaPequena = this.Height < limiteAltura;

            if (telaPequena)
            {
                // Reduzir fontes
                panelHeader.Font = new Font("Segoe UI", fonteOriginalHeader * 0.7f, FontStyle.Bold);
                lblTime.Font = new Font("Segoe UI", fonteOriginalHeader * 0.7f, FontStyle.Bold);
                lblDate.Font = new Font("Segoe UI", fonteOriginalData * 0.8f);
                lblLocalProducao.Font = new Font("Segoe UI", fonteOriginalTitulo * 0.9f, FontStyle.Bold);
                lblInstrucao.Font = new Font("Segoe UI", fonteOriginalInstrucao * 0.9f, FontStyle.Bold);
                lblStatus.Font = new Font("Segoe UI", fonteOriginalStatus * 0.9f);

                // Reduzir painéis
                panelHeader.Size = new Size(tamanhoOriginalPanelHeader.Width, (int)(tamanhoOriginalPanelHeader.Height * 0.7));
                panelSimulador.Size = new Size(tamanhoOriginalPanelSimulador.Width, (int)(tamanhoOriginalPanelSimulador.Height * 0.7));
                panelStatusBar.Size = new Size(tamanhoOriginalPanelStatusBar.Width, (int)(tamanhoOriginalPanelStatusBar.Height * 0.7));
            }
            else
            {
                // Restaurar fontes
                panelHeader.Font = new Font("Segoe UI", fonteOriginalHeader, FontStyle.Bold);
                lblTime.Font = new Font("Segoe UI", fonteOriginalHeader, FontStyle.Bold);
                lblDate.Font = new Font("Segoe UI", fonteOriginalData);
                lblLocalProducao.Font = new Font("Segoe UI", fonteOriginalTitulo, FontStyle.Bold);
                lblInstrucao.Font = new Font("Segoe UI", fonteOriginalInstrucao, FontStyle.Bold);
                lblStatus.Font = new Font("Segoe UI", fonteOriginalStatus);

                // Restaurar painéis
                panelHeader.Size = tamanhoOriginalPanelHeader;
                panelSimulador.Size = tamanhoOriginalPanelSimulador;
                panelStatusBar.Size = tamanhoOriginalPanelStatusBar;
            }
        }

        private void AplicarBordasArredondadas()
        {
            // Arredondar header
            panelHeader.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = GetRoundedRectangle(panelHeader.ClientRectangle, 20))
                {
                    panelHeader.Region = new Region(path);
                }
            };
            
            // Arredondar combobox
            cmbSetor.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            };
            
            // Arredondar painel simulador
            panelSimulador.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = GetRoundedRectangle(panelSimulador.ClientRectangle, 15))
                {
                    panelSimulador.Region = new Region(path);
                }
            };
        }

        private async Task CarregarSetoresDoHospitalAsync()
        {
            var setores = new List<(int Id, string Nome)>();
            string cacheHospitalId = string.IsNullOrEmpty(hospitalId) ? "DEFAULT" : hospitalId;

            try
            {
                // Prioridade 1: Buscar do cache (após sincronização)
                var setoresCache = await Task.Run(() => database.BuscarSetoresLocal(cacheHospitalId));
                if (setoresCache.Any())
                {
                    setores = setoresCache.Cast<(int, string)>().ToList();
                    string statusText = _setoresSyncOk == false
                        ? "📂 Setores carregados do cache (offline)."
                        : "📂 Setores carregados do cache.";
                    if (InvokeRequired)
                    {
                        BeginInvoke(() =>
                        {
                            lblStatus.Text = statusText;
                            ExibirSetores(setores);
                        });
                    }
                    else
                    {
                        lblStatus.Text = statusText;
                        ExibirSetores(setores);
                    }
                    return;
                }

                // Prioridade 2: Setores padrão (fallback)
                var setoresPadrao = new List<(int, string)>
                {
                    (1, "CENTRO CIRÚRGICO"),
                    (2, "EMERGÊNCIA"),
                    (3, "UTI"),
                    (4, "ENFERMARIA"),
                    (5, "LABORATÓRIO"),
                    (6, "RADIOLOGIA"),
                    (7, "FARMÁCIA"),
                    (8, "RECEPÇÃO"),
                    (9, "ADMINISTRATIVO")
                };

                await Task.Run(() => database.SalvarSetoresLocal(cacheHospitalId, setoresPadrao));
                setores = setoresPadrao;
                
                if (InvokeRequired)
                {
                    BeginInvoke(() =>
                    {
                        lblStatus.Text = "📂 Setores padrão carregados.";
                        ExibirSetores(setores);
                    });
                }
                else
                {
                    lblStatus.Text = "📂 Setores padrão carregados.";
                    ExibirSetores(setores);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao carregar setores: {ex.Message}");
                
                if (InvokeRequired)
                {
                    BeginInvoke(() =>
                    {
                        cmbSetor.Items.Clear();
                        cmbSetor.Items.AddRange(new string[] 
                        {
                            "CENTRO CIRÚRGICO",
                            "EMERGÊNCIA",
"UTI",
                            "ENFERMARIA",
                            "LABORATÓRIO",
                            "RADIOLOGIA",
                            "FARMÁCIA",
                            "RECEPÇÃO",
                            "ADMINISTRATIVO"
                        });
                        cmbSetor.SelectedIndex = -1;
                        lblStatus.Text = "📂 Setores padrão (modo emergência).";
                    });
                }
                else
                {
                    cmbSetor.Items.Clear();
                    cmbSetor.Items.AddRange(new string[] 
                    {
                        "CENTRO CIRÚRGICO",
                        "EMERGÊNCIA",
                        "UTI",
                        "ENFERMARIA",
                        "LABORATÓRIO",
                        "RADIOLOGIA",
                        "FARMÁCIA",
                        "RECEPÇÃO",
                        "ADMINISTRATIVO"
                    });
                    cmbSetor.SelectedIndex = -1;
                    lblStatus.Text = "📂 Setores padrão (modo emergência).";
                }
            }
        }

        private async Task SyncSetoresFromTursoAsync()
        {
            try
            {
                if (_tursoConnection == null || string.IsNullOrWhiteSpace(hospitalId))
                    return;

                string cacheHospitalId = string.IsNullOrEmpty(hospitalId) ? "DEFAULT" : hospitalId;

                if (InvokeRequired)
                {
                    BeginInvoke(() => lblStatus.Text = "🔄 Sincronizando setores para o banco local...");
                }
                else
                {
                    lblStatus.Text = "🔄 Sincronizando setores para o banco local...";
                }

                // Timeout de 5 segundos para não travar na primeira instalação
                var timeoutTask = Task.Delay(5000);
                var connectionTask = _tursoConnection.TestConnectionAsync();
                var completedTask = await Task.WhenAny(connectionTask, timeoutTask);
                
                if (completedTask == timeoutTask || !(await connectionTask))
                {
                    Debug.WriteLine("[Sync] Turso não disponível ou timeout - usando cache");
                    _setoresSyncOk = false;

                    if (database.TemSetoresLocal(cacheHospitalId))
                    {
                        if (InvokeRequired)
                        {
                            BeginInvoke(() => lblStatus.Text = "📂 Setores carregados do cache (offline)." );
                        }
                        else
                        {
                            lblStatus.Text = "📂 Setores carregados do cache (offline).";
                        }
                    }
                    return;
                }

                var sql = @"SELECT s.id, s.nome
                            FROM setores s
                            INNER JOIN hospital_setores hs ON hs.setor_id = s.id
                            WHERE hs.hospital_id = ?
                            ORDER BY s.nome";

                var result = await _tursoConnection.ExecuteQueryAsync(sql, new object[] { hospitalId });
                if (result == null || result.Count == 0)
                    return;

                var setoresTurso = new List<(int Id, string Nome)>();
                foreach (var row in result)
                {
                    setoresTurso.Add((
                        Id: int.TryParse(row["id"]?.ToString(), out var idVal) ? idVal : 0,
                        Nome: row["nome"]?.ToString() ?? string.Empty
                    ));
                }

                if (setoresTurso.Count == 0)
                    return;

                // ✅ COMPARAR com cache local e detectar diferenças
                var setoresCache = await Task.Run(() => database.BuscarSetoresLocal(cacheHospitalId));
                
                var idsCache = setoresCache.Select(s => s.Item1).ToHashSet();
                var idsTurso = setoresTurso.Select(s => s.Id).ToHashSet();

                var novos = setoresTurso.Where(s => !idsCache.Contains(s.Id)).ToList();
                var removidos = setoresCache.Where(s => !idsTurso.Contains(s.Item1)).ToList();

                if (novos.Any() || removidos.Any())
                {
                    Debug.WriteLine($"🔄 Diferenças detectadas:");
                    if (novos.Any())
                        Debug.WriteLine($"   ➕ {novos.Count} setores adicionados: {string.Join(", ", novos.Select(s => s.Nome))}");
                    if (removidos.Any())
                        Debug.WriteLine($"   ➖ {removidos.Count} setores removidos: {string.Join(", ", removidos.Select(s => s.Item2))}");
                }

                // ✅ ATUALIZAR cache com setores online
                await Task.Run(() => database.SalvarSetoresLocal(cacheHospitalId, setoresTurso));
                Debug.WriteLine($"✅ Cache atualizado com {setoresTurso.Count} setores do Turso");
                _setoresSyncOk = true;

                if (InvokeRequired)
                {
                    BeginInvoke(() => lblStatus.Text = "✅ Sincronização dos setores concluída!");
                }
                else
                {
                    lblStatus.Text = "✅ Sincronização dos setores concluída!";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Falha ao sincronizar setores do Turso: {ex.Message}");
                _setoresSyncOk = false;
            }
        }

        private void ExibirSetores(List<(int Id, string Nome)> setores)
        {
            try
            {
                cmbSetor.Items.Clear();
                foreach (var setor in setores)
                {
                    cmbSetor.Items.Add(new { Id = setor.Id, Nome = setor.Nome });
                }
                cmbSetor.DisplayMember = "Nome";
                cmbSetor.ValueMember = "Id";
                cmbSetor.SelectedIndex = -1;
                Debug.WriteLine($"✅ Dropdown exibindo {setores.Count} setores");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Erro ao exibir setores: {ex.Message}");
            }
        }

        private GraphicsPath GetRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            
            return path;
        }
        
        private void CentralizarControles()
        {
            int centerX = this.ClientSize.Width / 2;
            
            // Centralizar header
            panelHeader.Left = centerX - (panelHeader.Width / 2);
            
            // Centralizar labels e combobox
            lblLocalProducao.Left = centerX - 350;
            lblSetorAla.Left = centerX - 350;
            cmbSetor.Left = centerX - 350;
            cmbSetor.Width = 700;
            
            // Centralizar instrução
            lblInstrucao.Left = centerX - 350;
            lblInstrucao.Width = 700;
            
            // Centralizar painel simulador
            panelSimulador.Left = centerX - (panelSimulador.Width / 2);
            
            // Não centralizar manualmente a barra de status, pois ela está dockada
        }

        private void LoginForm_Resize(object sender, EventArgs e)
        {
            CentralizarControles();
        }

        private void UpdateClock()
        {
            lblTime.Text = DateTime.Now.ToString("HH:mm:ss");
            
            // Formatar data em português
            var culture = new CultureInfo("pt-BR");
            lblDate.Text = DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy", culture);
        }

        private void timerClock_Tick(object sender, EventArgs e)
        {
            UpdateClock();
        }

        private async void cmbSetor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSetor.SelectedIndex == -1 || isCapturing)
                return;

            // Capturar setor e ID do setor selecionado
            var selectedItem = cmbSetor.SelectedItem;
            if (selectedItem != null && selectedItem.GetType().GetProperty("Id") != null)
            {
                // Tenta extrair propriedades do objeto dinâmico
                var idProp = selectedItem.GetType().GetProperty("Id");
                var nomeProp = selectedItem.GetType().GetProperty("Nome");
                
                if (idProp != null && nomeProp != null)
                {
                    var idValue = idProp.GetValue(selectedItem);
                    if (idValue != null && int.TryParse(idValue.ToString(), out int id))
                    {
                        selectedSetorId = id;
                    }
                    else
                    {
                        selectedSetorId = null;
                    }
                    selectedSetor = nomeProp.GetValue(selectedItem)?.ToString();
                }
                else
                {
                    selectedSetor = selectedItem.ToString();
                    selectedSetorId = null;
                }
            }
            else
            {
                // Fallback para string simples (lista padrão)
                selectedSetor = selectedItem?.ToString();
                selectedSetorId = null;
            }
            
            if (!string.IsNullOrEmpty(selectedSetor))
            {
                if (!_biometriaSyncDone && !_biometriaSyncInProgress)
                {
                    _biometriaSyncInProgress = true;
                    cmbSetor.Enabled = false;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await ExecutarSincronizacaoInicial();
                            _biometriaSyncDone = true;
                        }
                        finally
                        {
                            _biometriaSyncInProgress = false;
                            if (InvokeRequired)
                            {
                                BeginInvoke(() =>
                                {
                                    cmbSetor.Enabled = true;
                                    lblStatus.Text = "Selecione o setor para ativar o leitor";
                                });
                            }
                            else
                            {
                                cmbSetor.Enabled = true;
                                lblStatus.Text = "Selecione o setor para ativar o leitor";
                            }
                        }
                    });

                    return;
                }

                // Desabilitar combo durante captura
                cmbSetor.Enabled = false;
                isCapturing = true;

                // TODO: Sincronização com Turso será implementada na versão futura
                LogToFile("[SETOR-SELECIONADO] 🔍 Aguardando implementação de sincronização com Turso...");

                lblStatus.Text = $"⏳ Setor: {selectedSetor} - Posicione o dedo no leitor...";
                
                // Animar ícone de digital
                panelFingerprint.BackColor = System.Drawing.Color.FromArgb(230, 240, 255);
                
                // Iniciar captura automática
                _ = Task.Run(async () =>
                {
                    try
                    {
                        if (!_readerInitialized)
                        {
                            fingerprintService.InitializeSdk();
                            var ok = fingerprintService.InitializeReader();
                            _readerInitialized = true;

                            if (!ok)
                            {
                                LogToFile("⚠️ Leitor não encontrado. Verifique a conexão.");
                                if (InvokeRequired)
                                    BeginInvoke(() => lblStatus.Text = "⚠️ Leitor não encontrado. Verifique a conexão.");
                                else
                                    lblStatus.Text = "⚠️ Leitor não encontrado. Verifique a conexão.";
                            }
                        }

                        await fingerprintService.StartCapture();
                    }
                    catch (Exception ex)
                    {
                        LogToFile($"❌ Erro ao iniciar captura: {ex.Message}");
                    }
                    finally
                    {
                        try
                        {
                            if (InvokeRequired)
                                Invoke(() =>
                                {
                                    cmbSetor.Enabled = true;
                                    isCapturing = false;
                                    panelFingerprint.BackColor = System.Drawing.Color.White;
                                });
                            else
                            {
                                cmbSetor.Enabled = true;
                                isCapturing = false;
                                panelFingerprint.BackColor = System.Drawing.Color.White;
                            }
                        }
                        catch { /* ignorar */ }
                    }
                });
            }
        }

        private async Task ExecutarSincronizacaoInicial()
        {
            BiometriaSyncProgressForm? syncForm = null;
            try
            {
                // Criar form na thread UI ANTES de qualquer operação
                if (InvokeRequired)
                {
                    Invoke(() =>
                    {
                        syncForm = new BiometriaSyncProgressForm
                        {
                            StartPosition = FormStartPosition.CenterParent,
                            TopMost = true,
                            ShowInTaskbar = false
                        };
                        syncForm.Show(this);
                        syncForm.BringToFront();
                        syncForm.Activate();
                        syncForm.Refresh(); // Forca renderizacao imediata
                        Application.DoEvents(); // Processa eventos pendentes
                    });
                }
                else
                {
                    syncForm = new BiometriaSyncProgressForm
                    {
                        StartPosition = FormStartPosition.CenterParent,
                        TopMost = true,
                        ShowInTaskbar = false
                    };
                    syncForm.Show(this);
                    syncForm.BringToFront();
                    syncForm.Activate();
                    syncForm.Refresh();
                    Application.DoEvents();
                }

                // Aguardar form renderizar completamente
                await Task.Delay(300);

                LogToFile("[SINC-INICIAL] ⏳ Iniciando download de biometrias do Turso...");

                if (_config == null)
                {
                    LogToFile("[SINC-INICIAL] ❌ Configuração não carregada");
                    if (syncForm != null)
                    {
                        if (InvokeRequired)
                            Invoke(() => { syncForm.SetError("Configuração não carregada"); syncForm.Refresh(); });
                        else
                        { syncForm.SetError("Configuração não carregada"); syncForm.Refresh(); }
                    }
                    return;
                }

                var tursoUrl = _config["TursoDb:Url"] ?? string.Empty;
                var authToken = _config["TursoDb:AuthToken"] ?? string.Empty;

                if (string.IsNullOrWhiteSpace(tursoUrl) || string.IsNullOrWhiteSpace(authToken))
                {
                    LogToFile("[SINC-INICIAL] ❌ TursoUrl/AuthToken ausentes no appsettings");
                    if (syncForm != null)
                    {
                        if (InvokeRequired)
                            Invoke(() => { syncForm.SetError("TursoUrl/AuthToken ausentes"); syncForm.Refresh(); });
                        else
                        { syncForm.SetError("TursoUrl/AuthToken ausentes"); syncForm.Refresh(); }
                    }
                    return;
                }

                var tursoHelper = new TursoCooperadoHelper(tursoUrl, authToken);
                
                // ⏱️ Timeout de 5 segundos para diagnóstico
                var diagTask = tursoHelper.GetBiometriasDiagnosticsAsync();
                var diagTimeoutTask = Task.Delay(5000);
                var diagCompleted = await Task.WhenAny(diagTask, diagTimeoutTask);
                
                if (diagCompleted == diagTask)
                {
                    var diag = await diagTask;
                    foreach (var line in diag)
                    {
                        LogToFile(line);
                    }
                }
                else
                {
                    LogToFile("[SINC-INICIAL] ⚠️ Timeout ao obter diagnóstico do Turso (5s)");
                }
                
                // ⏱️ Timeout de 30 segundos para evitar travamento
                var downloadTask = tursoHelper.BaixarTodasBiometriasParaSincAsync();
                var timeoutTask = Task.Delay(30000);
                var completedTask = await Task.WhenAny(downloadTask, timeoutTask);
                
                if (completedTask == timeoutTask)
                {
                    LogToFile("[SINC-INICIAL] ⚠️ Timeout ao baixar biometrias (30s excedidos)");
                    if (syncForm != null)
                    {
                        if (InvokeRequired)
                            Invoke(() => { syncForm.SetWarning("Timeout ao baixar biometrias. Usando cache local."); syncForm.Refresh(); });
                        else
                        { syncForm.SetWarning("Timeout ao baixar biometrias. Usando cache local."); syncForm.Refresh(); }
                    }
                    return;
                }
                
                var biometrias = await downloadTask;

                if (biometrias == null || biometrias.Count == 0)
                {
                    LogToFile("[SINC-INICIAL] ⚠️ Nenhuma biometria encontrada no Turso");
                    if (syncForm != null)
                    {
                        if (InvokeRequired)
                            Invoke(() => { syncForm.SetWarning("Nenhuma biometria encontrada no Turso."); syncForm.Refresh(); });
                        else
                        { syncForm.SetWarning("Nenhuma biometria encontrada no Turso."); syncForm.Refresh(); }
                    }
                    return;
                }

                int total = await database.SalvarBiometriasEmLoteAsync(biometrias);
                LogToFile($"[SINC-INICIAL] ✅ {total} biometrias baixadas do Turso");
                if (syncForm != null)
                {
                    if (InvokeRequired)
                        Invoke(() => { syncForm.SetSuccess(total); syncForm.Refresh(); });
                    else
                    { syncForm.SetSuccess(total); syncForm.Refresh(); }
                }
            }
            catch (Exception ex)
            {
                LogToFile($"[SINC-INICIAL] ❌ Erro na sincronização: {ex.Message}");
                try
                {
                    var msg = string.IsNullOrWhiteSpace(ex.Message) ? "Erro desconhecido" : ex.Message;
                    if (syncForm != null)
                    {
                        if (InvokeRequired)
                            Invoke(() => { syncForm.SetError(msg); syncForm.Refresh(); });
                        else
                        { syncForm.SetError(msg); syncForm.Refresh(); }
                    }
                }
                catch { /* ignorar */ }
            }
            finally
            {
                try
                {
                    // Aguardar 2 segundos para usuário ver resultado
                    await Task.Delay(2000);
                    
                    if (syncForm != null)
                    {
                        if (InvokeRequired)
                            Invoke(() =>
                            {
                                try
                                {
                                    if (syncForm != null && !syncForm.IsDisposed)
                                    {
                                        syncForm.Close();
                                        syncForm.Dispose();
                                    }
                                }
                                catch { /* ignorar */ }
                            });
                        else
                        {
                            try
                            {
                                if (!syncForm.IsDisposed)
                                {
                                    syncForm.Close();
                                    syncForm.Dispose();
                                }
                            }
                            catch { /* ignorar */ }
                        }
                    }
                }
                catch { /* ignorar */ }
            }
        }

        private async void OnFingerprintCaptured(object? sender, byte[] template)
        {
            if (_isVerifying)
                return;

            _isVerifying = true;
            lblStatus.Text = "⏳ Verificando digital localmente...";
            Refresh();

            try
            {
                LogToFile("🔍 OnFingerprintCaptured - Iniciando verificação LOCAL");

                // Buscar biometrias do banco LOCAL (muito mais rápido)
                LogToFile("📡 Buscando biometrias do SQLite local...");
                var biometriasLocais = database.BuscarBiometriasLocais();
                
                LogToFile($"✅ Biometrias retornadas: {biometriasLocais.Count}");
                
                if (biometriasLocais.Count == 0)
                {
                    LogToFile("⚠️ Lista de biometrias está vazia");
                    lblStatus.Text = "⚠️ Nenhuma biometria cadastrada no sistema";
                    panelSimulador.BackColor = System.Drawing.Color.FromArgb(255, 245, 230);
                    lblSimulador.Text = "Nenhuma biometria cadastrada!\n\nCadastre biometrias primeiro.";
                    lblSimulador.ForeColor = System.Drawing.Color.FromArgb(200, 100, 0);
                    cmbSetor.SelectedIndex = -1;
                    AgendarLimpezaPainel();
                    return;
                }

                string? matchedCooperadoId = null;
                string? matchedCooperadoNome = null;

                LogToFile($"🔍 Verificando template capturado contra {biometriasLocais.Count} biometrias...");
                // Verificar contra cada biometria usando o verificador nativo do SDK
                int idx = 0;
                foreach (var biometria in biometriasLocais)
                {
                    idx++;
                    if (biometria.Template != null && biometria.Template.Length > 0)
                    {
                        LogToFile($"   Testando biometria {idx}: {biometria.CooperadoNome} ({biometria.Template.Length} bytes)");
                        if (fingerprintService.VerifyAgainstTemplate(biometria.Template))
                        {
                            LogToFile($"   ✅ MATCH! Cooperado: {biometria.CooperadoNome}");
                            matchedCooperadoId = biometria.CooperadoId;
                            matchedCooperadoNome = biometria.CooperadoNome;
                            break;
                        }
                    }
                    else
                    {
                        LogToFile($"   ⚠️ Biometria {idx} tem template nulo ou vazio");
                    }
                }

                // Limpar features capturadas após verificação completa
                fingerprintService.ClearCapturedFeatures();

                if (matchedCooperadoId != null)
                {
                    LogToFile($"✅ Digital identificada: {matchedCooperadoNome}");

                    // Decidir o tipo da proxima producao com base na tolerancia e plantao noturno
                    string tipoRegistro = database.DecidirTipoProximoPonto(matchedCooperadoId, 14, 16);
                    LogToFile($"   Tipo de registro: {tipoRegistro} (lógica tolerância/plantão)");

                    // Bloqueio: se última ENTRADA foi há menos de 1 hora, não permite SAÍDA
                    if (tipoRegistro == "SAIDA")
                    {
                        var ultimaEntradaDt = database.ObterTimestampUltimaEntrada(matchedCooperadoId);
                        if (ultimaEntradaDt != null)
                        {
                            var agora = DateTimeOffset.Now;
                            var diff = (agora - ultimaEntradaDt.Value).TotalMinutes;
                            if (diff <= 60)
                            {
                                LogToFile($"⚠️ SAIDA bloqueada: ENTRADA há {diff:F1} min (limite 60) - {matchedCooperadoNome}");
                                // Exibir alerta amarelo
                                panelSimulador.BackColor = System.Drawing.Color.FromArgb(255, 255, 200); // Amarelo claro
                                lblSimulador.Text = $"⚠️ {matchedCooperadoNome}, você já possui um registro de ENTRADA às {ultimaEntradaDt:HH:mm}.";
                                lblSimulador.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
                                lblSimulador.ForeColor = System.Drawing.Color.FromArgb(180, 120, 0); // Amarelo escuro
                                lblSimulador.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                                lblStatus.Text = $"⚠️ ENTRADA recente - {matchedCooperadoNome}";

                                selectedSetor = null;
                                selectedSetorId = null;
                                cmbSetor.SelectedIndex = -1;
                                cmbSetor.Text = string.Empty;
                                
                                // Agendar limpeza automática
                                AgendarLimpezaPainel();
                                return;
                            }
                        }
                    }

                    // Formatar local como no sistema web: "CODIGO_UNIDADE - SETOR"
                    string localFormatado = string.IsNullOrEmpty(hospitalCodigo)
                        ? (selectedSetor ?? "N/A")
                        : $"{hospitalCodigo} - {selectedSetor ?? "N/A"}";

                    // Registrar producao LOCAL (instantaneo)
                    bool sucessoLocal = database.SalvarPontoLocal(
                        matchedCooperadoId,
                        matchedCooperadoNome,
                        tipoRegistro,
                        localFormatado,
                        hospitalId,
                        selectedSetorId
                    );

                    if (sucessoLocal)
                    {
                        LogToFile("   ✅ Produção registrada localmente com sucesso!");
                        // Exibir informações no painel
                        ExibirRegistroPontoLocal(
                            matchedCooperadoNome,
                            tipoRegistro,
                            DateTime.Now
                        );

                        // Resetar seleção do setor
                        cmbSetor.SelectedIndex = -1;

                        // Sincronizar com TURSO em background (não bloqueia UI)
                        LogToFile("   ℹ️ Disparando sincronização em background...");
#pragma warning disable CS4014
                        Task.Run(async () => await SincronizarComTursoAsync());
#pragma warning restore CS4014
                        LogToFile("   ℹ️ Sincronização disparada (método async)");
                    }
                    else
                    {
                        LogToFile("   ❌ Erro ao registrar produção localmente");
                        lblStatus.Text = "❌ Erro ao registrar produção";
                        panelSimulador.BackColor = System.Drawing.Color.FromArgb(255, 230, 230);
                        lblSimulador.Text = "Erro ao registrar produção no banco de dados!";
                        lblSimulador.ForeColor = System.Drawing.Color.FromArgb(180, 0, 0);
                        AgendarLimpezaPainel();
                    }
                }
                else
                {
                    LogToFile("❌ Nenhuma biometria correspondente encontrada");
                    lblStatus.Text = "❌ Digital não reconhecida";
                    panelSimulador.BackColor = System.Drawing.Color.FromArgb(255, 200, 200); // Vermelho claro
                    lblSimulador.Text = "❌ Digital não reconhecida!\n\nCooperado não cadastrado no sistema.";
                    lblSimulador.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
                    lblSimulador.ForeColor = System.Drawing.Color.FromArgb(200, 0, 0); // Vermelho escuro
                    lblSimulador.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                    cmbSetor.SelectedIndex = -1;
                    AgendarLimpezaPainel();
                }
            }
            catch (Exception ex)
            {
                LogToFile($"❌ ERRO em OnFingerprintCaptured: {ex.Message}");
                LogToFile($"   Stack: {ex.StackTrace}");
                lblStatus.Text = $"❌ Erro: {ex.Message}";
            }
            finally
            {
                _isVerifying = false;

                // Rearmar captura para permitir novas leituras
                if (!IsDisposed && cmbSetor.SelectedIndex != -1)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await fingerprintService.StartCapture();
                        }
                        catch (Exception ex)
                        {
                            LogToFile($"❌ Erro ao rearmar captura: {ex.Message}");
                        }
                    });
                }
            }
        }

        private void LogToFile(string message)
        {
            try
            {
                string logRoot = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) ?? "C:\\Temp";
                if (string.IsNullOrEmpty(logRoot)) logRoot = "C:\\Temp";
                string logDir = System.IO.Path.Combine(logRoot, "BiometricSystem");
                System.IO.Directory.CreateDirectory(logDir);
                string logPath = System.IO.Path.Combine(logDir, "biometric_log.txt");
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
                File.AppendAllText(logPath, logMessage + Environment.NewLine);
                System.Diagnostics.Debug.WriteLine(logMessage);
            }
            catch { }
        }

        private void ExibirRegistroPontoLocal(string nomeCooperado, string tipoRegistro, DateTime horario)
        {
            System.Drawing.Color backgroundColor;
            System.Drawing.Color textColor;
            string emoji;
            string tipoExibicao;

            if (tipoRegistro == "ENTRADA")
            {
                backgroundColor = System.Drawing.Color.FromArgb(230, 255, 240); // Verde claro
                textColor = System.Drawing.Color.FromArgb(0, 120, 60);
                emoji = "➜";
                tipoExibicao = "ENTRADA";
            }
            else
            {
                backgroundColor = System.Drawing.Color.FromArgb(255, 235, 235); // Vermelho claro
                textColor = System.Drawing.Color.FromArgb(180, 30, 30);
                emoji = "⬅";
                tipoExibicao = "SAÍDA";
            }

            panelSimulador.BackColor = backgroundColor;

            // Montar texto formatado
            string textoExibicao = $"{emoji}  {tipoExibicao} REGISTRADA\n\n";
            textoExibicao += $"{nomeCooperado}\n";
            textoExibicao += "Cooperado\n\n";
            textoExibicao += $"📍 {selectedSetor}\n";
            textoExibicao += $"🕐 {horario:HH:mm:ss}";

            lblSimulador.Text = textoExibicao;
            lblSimulador.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            lblSimulador.ForeColor = textColor;
            lblSimulador.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            lblStatus.Text = $"✅ {tipoExibicao} registrada - {nomeCooperado}";

            // Agendar limpeza automática do painel
            AgendarLimpezaPainel();
        }

        /// <summary>
        /// Agenda a limpeza automática do painel após 5 segundos
        /// </summary>
        private void AgendarLimpezaPainel()
        {
            LogToFile($"⏰ Agendando limpeza do painel em 5 segundos...");
            
            // Usar thread separada para aguardar 5 segundos e depois limpar
            var cleanupThread = new Thread(() =>
            {
                try
                {
                    Thread.Sleep(5000);
                    
                    // Executar na thread UI
                    this.Invoke(new Action(() =>
                    {
                        try
                        {
                            LogToFile($"⏰ Limpando painel após 5 segundos");
                            LimparPainelSimulador();
                        }
                        catch (Exception ex)
                        {
                            LogToFile($"❌ Erro ao limpar painel: {ex.Message}");
                        }
                    }));
                }
                catch (Exception ex)
                {
                    LogToFile($"❌ Erro na thread de limpeza: {ex.Message}");
                }
            })
            {
                IsBackground = true
            };
            cleanupThread.Start();
        }

        private void LimparPainelSimulador()
        {
            try
            {
                LogToFile($"⏰ Limpando painel - início");
                
                panelSimulador.BackColor = System.Drawing.Color.White;
                lblSimulador.Text = "";
                lblSimulador.Font = new System.Drawing.Font("Segoe UI", 12F);
                lblSimulador.TextAlign = System.Drawing.ContentAlignment.TopLeft;
                lblStatus.Text = "Selecione o setor para ativar o leitor";

                selectedSetor = null;
                selectedSetorId = null;
                cmbSetor.SelectedIndex = -1;
                cmbSetor.Text = string.Empty;
                LogToFile("⏰ Setor/Ala limpo apos registro");
                
                LogToFile($"⏰ Limpando painel - concluído");
            }
            catch (Exception ex)
            {
                LogToFile($"❌ Erro em LimparPainelSimulador: {ex.Message}");
            }
        }

        protected override async void OnFormClosing(FormClosingEventArgs e)
        {

            // Se for retorno do menu de produção, só trava/maximiza e não pede autenticação
            if (VoltarDaProducao)
            {
                VoltarDaProducao = false;
                this.WindowState = FormWindowState.Maximized;
                this.TopMost = true;
                e.Cancel = true;
                // Aqui pode travar a tela se necessário
                return;
            }

            // Se AllowClose for true, permite fechar sem autenticação
            if (AllowClose)
            {
                if (clearPanelTimer != null)
                {
                    clearPanelTimer.Stop();
                    clearPanelTimer.Dispose();
                    clearPanelTimer = null;
                }
                syncService?.StopSync();
                fingerprintService.Dispose();
                base.OnFormClosing(e);
                return;
            }

            // Prompt de autenticação administrativa
            e.Cancel = true;
            var authDialog = new AuthDialogForm(async (pass) =>
            {
                // Autentica apenas com senha local
                if (database.ValidarSenhaLocal(pass))
                    return true;
                return false;
            });
            authDialog.TopMost = true;
            authDialog.BringToFront();
            this.TopMost = false; // Garante que o dialog fique acima
            authDialog.FormClosed += async (s, args) =>
            {
                this.TopMost = true; // Restaura prioridade
                if (authDialog.AuthSuccess)
                {
                    LogToFile("[SINC-INICIAL] ✅ Abrindo AccessMenuForm...");
                    if (_config == null)
                    {
                        MessageBox.Show("Configuração do Turso não carregada.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var menu = new AccessMenuForm(this, _config);
                    menu.TopMost = true;
                    menu.Show();
                    this.Hide();
                    menu.FormClosed += (ms, ma) => {
                        Application.Exit();
                    };
                }
                // Se não autenticou, volta para tela de produção (LoginForm permanece visível)
            };
            authDialog.Show();
        }

        /// <summary>
        /// Sincroniza registros locais (não sincronizados) com TURSO em background
        /// Executa de forma assíncrona e não bloqueia a UI
        /// </summary>
        private async Task SincronizarComTursoAsync()
        {
            try
            {
                await Task.Delay(100);

                if (syncService == null)
                {
                    LogToFile("⚠️ [SYNC] Serviço de sincronização não inicializado");
                    return;
                }

                LogToFile("🔄 [SYNC] Solicitando sincronização imediata com TURSO...");
                await syncService.SyncNowAsync();
                LogToFile("✅ [SYNC] Sincronização imediata concluída");
            }
            catch (Exception ex)
            {
                LogToFile($"❌ [SYNC] Erro ao sincronizar: {ex.Message}");
                LogToFile($"   Stack: {ex.StackTrace}");
            }
        }

        // Impede movimentação da janela
        protected override void WndProc(ref Message m)
        {
		if (m.Msg == WM_NCLBUTTONDOWN && m.WParam.ToInt32() == HTCAPTION)
		{
			// Bloqueia o arrastar da janela
			return;
		}
		base.WndProc(ref m);
	}
}
}

