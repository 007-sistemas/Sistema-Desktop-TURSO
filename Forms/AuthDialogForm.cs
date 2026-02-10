using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BiometricSystem.Forms
{
    public partial class AuthDialogForm : Form
    {
        public string Password => txtPassword.Text;
        public bool AuthSuccess { get; private set; } = false;
        private readonly Func<string, Task<bool>> _authCallback;
        private System.Threading.Timer _inactivityTimer;
        private const int INACTIVITY_TIMEOUT_MS = 60000; // 60 segundos (1 minuto)
        private const int ERROR_TIMEOUT_MS = 3000; // 3 segundos para erro

        public AuthDialogForm(Func<string, Task<bool>> authCallback)
        {
            InitializeComponent();
            this.TopMost = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            _authCallback = authCallback;
            this.Shown += (s, e) => {
                this.BringToFront();
                this.Activate();
                this.Focus();
                this.txtPassword.Focus();
                IniciarTimerInatividade();
            };
            this.Activated += (s, e) => {
                this.BringToFront();
                this.Activate();
                this.Focus();
                this.txtPassword.Focus();
            };

            // Eventos do teclado virtual
            this.btnNum0.Click += (s, e) => { txtPassword.Text += "0"; ReiniciarTimerInatividade(); };
            this.btnNum1.Click += (s, e) => { txtPassword.Text += "1"; ReiniciarTimerInatividade(); };
            this.btnNum2.Click += (s, e) => { txtPassword.Text += "2"; ReiniciarTimerInatividade(); };
            this.btnNum3.Click += (s, e) => { txtPassword.Text += "3"; ReiniciarTimerInatividade(); };
            this.btnNum4.Click += (s, e) => { txtPassword.Text += "4"; ReiniciarTimerInatividade(); };
            this.btnNum5.Click += (s, e) => { txtPassword.Text += "5"; ReiniciarTimerInatividade(); };
            this.btnNum6.Click += (s, e) => { txtPassword.Text += "6"; ReiniciarTimerInatividade(); };
            this.btnNum7.Click += (s, e) => { txtPassword.Text += "7"; ReiniciarTimerInatividade(); };
            this.btnNum8.Click += (s, e) => { txtPassword.Text += "8"; ReiniciarTimerInatividade(); };
            this.btnNum9.Click += (s, e) => { txtPassword.Text += "9"; ReiniciarTimerInatividade(); };

            // Botão Limpar
            this.btnLimparPrincipal.Click += (s, e) => { txtPassword.Text = string.Empty; ReiniciarTimerInatividade(); };

            // Botão Cancelar fecha o modal
            this.btnCancel.Click += (s, e) =>
            {
                PararTimerInatividade();
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            this.btnOk.Click += async (s, e) =>
            {
                PararTimerInatividade();
                this.Enabled = false;
                try
                {
                    bool result = false;
                    try
                    {
                        result = await _authCallback(txtPassword.Text);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro ao autenticar: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        AuthSuccess = false;
                        txtPassword.Text = string.Empty;
                        this.DialogResult = DialogResult.Cancel;
                        this.Enabled = true;
                        IniciarTimerInatividade();
                        return;
                    }
                    if (result)
                    {
                        // Senha correta: fechar sem popup, apenas destrava a tela
                        AuthSuccess = true;
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        // Senha errada: mostrar "Senha Inválida" sem botões e fechar em 3 segundos
                        AuthSuccess = false;
                        txtPassword.Text = string.Empty;
                        this.DialogResult = DialogResult.Cancel;
                        
                        // Mostrar mensagem de erro sem botão OK
                        ExibirErroSemBotao();
                    }
                }
                finally
                {
                    this.Enabled = true;
                }
            };

            this.FormClosing += (s, e) =>
            {
                PararTimerInatividade();
            };
        }

        private void IniciarTimerInatividade()
        {
            PararTimerInatividade();
            _inactivityTimer = new System.Threading.Timer((state) =>
            {
                if (InvokeRequired)
                {
                    Invoke(() => FecharPorInatividade());
                }
                else
                {
                    FecharPorInatividade();
                }
            }, null, INACTIVITY_TIMEOUT_MS, System.Threading.Timeout.Infinite);
        }

        private void ReiniciarTimerInatividade()
        {
            IniciarTimerInatividade();
        }

        private void PararTimerInatividade()
        {
            if (_inactivityTimer != null)
            {
                _inactivityTimer.Dispose();
                _inactivityTimer = null;
            }
        }

        private void FecharPorInatividade()
        {
            if (this.Visible)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void ExibirErroSemBotao()
        {
            // Criar form de erro customizado sem botões
            Form erroForm = new Form();
            erroForm.Text = "Erro";
            erroForm.Size = new System.Drawing.Size(300, 150);
            erroForm.StartPosition = FormStartPosition.CenterParent;
            erroForm.TopMost = true;
            erroForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            erroForm.MaximizeBox = false;
            erroForm.MinimizeBox = false;
            erroForm.ControlBox = false;
            erroForm.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);

            // Label com mensagem
            Label lblMensagem = new Label();
            lblMensagem.Text = "Senha Inválida";
            lblMensagem.Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold);
            lblMensagem.ForeColor = System.Drawing.Color.FromArgb(200, 0, 0);
            lblMensagem.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblMensagem.Dock = DockStyle.Fill;
            erroForm.Controls.Add(lblMensagem);

            // Timer para fechar após 3 segundos
            System.Threading.Timer timer = null;
            timer = new System.Threading.Timer((state) =>
            {
                if (erroForm.InvokeRequired)
                {
                    erroForm.Invoke(() =>
                    {
                        timer?.Dispose();
                        erroForm.Close();
                    });
                }
                else
                {
                    timer?.Dispose();
                    erroForm.Close();
                }
            }, null, ERROR_TIMEOUT_MS, System.Threading.Timeout.Infinite);

            erroForm.ShowDialog(this);
            
            // Após fechar o erro, fechar o dialog de autenticação
            if (this.Visible)
            {
                this.Close();
            }
        }
    }
}
