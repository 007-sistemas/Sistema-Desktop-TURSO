using System;
using System.Windows.Forms;

namespace BiometricSystem.Forms
{
    public partial class BiometriaSyncProgressForm : Form
    {
        private Label lblStatus;
        private Label lblMessage;
        private ProgressBar progressBar;

        public BiometriaSyncProgressForm()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ControlBox = false;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form
            this.ClientSize = new System.Drawing.Size(500, 220);
            this.Name = "BiometriaSyncProgressForm";
            this.Text = "Sincronização de Biometrias";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.BackColor = System.Drawing.Color.White;

            // lblStatus
            lblStatus = new Label();
            lblStatus.AutoSize = false;
            lblStatus.Location = new System.Drawing.Point(20, 20);
            lblStatus.Size = new System.Drawing.Size(460, 50);
            lblStatus.Text = "⏳ Aguarde a sincronização das biometrias...";
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            lblStatus.ForeColor = System.Drawing.Color.FromArgb(33, 150, 243);
            lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Controls.Add(lblStatus);

            // progressBar
            progressBar = new ProgressBar();
            progressBar.Location = new System.Drawing.Point(40, 90);
            progressBar.Size = new System.Drawing.Size(420, 30);
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.MarqueeAnimationSpeed = 30;
            this.Controls.Add(progressBar);

            // lblMessage
            lblMessage = new Label();
            lblMessage.AutoSize = false;
            lblMessage.Location = new System.Drawing.Point(20, 140);
            lblMessage.Size = new System.Drawing.Size(460, 60);
            lblMessage.Text = "";
            lblMessage.Font = new System.Drawing.Font("Segoe UI", 10F);
            lblMessage.ForeColor = System.Drawing.Color.FromArgb(66, 66, 66);
            lblMessage.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.Controls.Add(lblMessage);

            this.ResumeLayout(false);
        }

        public void SetSuccess(int biometriaCount)
        {
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Value = 100;

            lblStatus.Text = "✅ Biometrias Sincronizadas com Sucesso!";
            lblStatus.ForeColor = System.Drawing.Color.FromArgb(76, 175, 80);

            lblMessage.Text = $"{biometriaCount} biometria(s) foram sincronizadas e estão prontas para uso.";
            lblMessage.ForeColor = System.Drawing.Color.FromArgb(76, 175, 80);
        }

        public void SetError(string errorMessage)
        {
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Value = 0;

            lblStatus.Text = "❌ Erro na Sincronização";
            lblStatus.ForeColor = System.Drawing.Color.FromArgb(244, 67, 54);

            lblMessage.Text = $"Erro: {errorMessage}\n\nO sistema continuará funcionando com biometrias locais.";
            lblMessage.ForeColor = System.Drawing.Color.FromArgb(244, 67, 54);
        }

        public void SetWarning(string warningMessage)
        {
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Value = 100;

            lblStatus.Text = "⚠️ Aviso de Sincronização";
            lblStatus.ForeColor = System.Drawing.Color.FromArgb(255, 193, 7);

            lblMessage.Text = warningMessage;
            lblMessage.ForeColor = System.Drawing.Color.FromArgb(255, 193, 7);
        }
    }
}
