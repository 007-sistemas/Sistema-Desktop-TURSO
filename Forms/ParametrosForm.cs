using Microsoft.Extensions.Configuration;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace BiometricSystem.Forms
{
    public partial class ParametrosForm : Form
    {
        private readonly IConfiguration _config;
        private CheckBox chkIntervaloAtivo;
        private Label lblMinutos;
        private NumericUpDown numMinutos;
        private Button btnSalvar;
        private Button btnCancelar;

        public ParametrosForm(IConfiguration config)
        {
            _config = config;
            InitializeComponent();
            // Carregar valores atuais
            var parametros = Database.ParametrosHelper.GetParametros();
            chkIntervaloAtivo.Checked = parametros.IntervaloAtivo;
            numMinutos.Value = parametros.MinutosIntervalo;
            // Garantir que o formulário fique por cima
            this.TopMost = true;
            this.BringToFront();
            this.Activate();
        }

        private void InitializeComponent()
        {
            this.Text = "Parâmetros do Sistema";
            this.Size = new Size(400, 250);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(240, 242, 245);

            chkIntervaloAtivo = new CheckBox
            {
                Text = "Ativar intervalo entre registros",
                Location = new Point(30, 30),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                AutoSize = true
            };
            chkIntervaloAtivo.CheckedChanged += ChkIntervaloAtivo_CheckedChanged;

            lblMinutos = new Label
            {
                Text = "Minutos de intervalo:",
                Location = new Point(30, 80),
                Font = new Font("Segoe UI", 11F),
                AutoSize = true
            };

            numMinutos = new NumericUpDown
            {
                Location = new Point(200, 78),
                Minimum = 1,
                Maximum = 240,
                Value = 60,
                Font = new Font("Segoe UI", 11F),
                Width = 80
            };

            btnSalvar = new Button
            {
                Text = "Salvar",
                Location = new Point(70, 150),
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(34, 139, 87),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnSalvar.Click += BtnSalvar_Click;

            btnCancelar = new Button
            {
                Text = "Cancelar",
                Location = new Point(220, 150),
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnCancelar.Click += (s, e) => this.Close();

            this.Controls.Add(chkIntervaloAtivo);
            this.Controls.Add(lblMinutos);
            this.Controls.Add(numMinutos);
            this.Controls.Add(btnSalvar);
            this.Controls.Add(btnCancelar);

            // Inicializar visibilidade
            ChkIntervaloAtivo_CheckedChanged(null, null);
        }

        private void ChkIntervaloAtivo_CheckedChanged(object sender, EventArgs e)
        {
            lblMinutos.Visible = chkIntervaloAtivo.Checked;
            numMinutos.Visible = chkIntervaloAtivo.Checked;
        }

        private void BtnSalvar_Click(object sender, EventArgs e)
        {
            // Salvar no banco
            Database.ParametrosHelper.SalvarParametros(
                chkIntervaloAtivo.Checked,
                (int)numMinutos.Value
            );
            MessageBox.Show("Parâmetros salvos com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}
