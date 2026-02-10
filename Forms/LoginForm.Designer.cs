namespace BiometricSystem.Forms
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblTime = new System.Windows.Forms.Label();
            this.lblDate = new System.Windows.Forms.Label();
            
            this.lblLocalProducao = new System.Windows.Forms.Label();
            this.lblSetorAla = new System.Windows.Forms.Label();
            this.cmbSetor = new System.Windows.Forms.ComboBox();
            this.lblInstrucao = new System.Windows.Forms.Label();
            
            this.panelSimulador = new System.Windows.Forms.Panel();

            this.panelCpf = new System.Windows.Forms.Panel();
            this.lblCpfTitulo = new System.Windows.Forms.Label();
            this.txtCpf = new System.Windows.Forms.TextBox();
            this.tblCpfKeypad = new System.Windows.Forms.TableLayoutPanel();
            this.btnCpf1 = new System.Windows.Forms.Button();
            this.btnCpf2 = new System.Windows.Forms.Button();
            this.btnCpf3 = new System.Windows.Forms.Button();
            this.btnCpf4 = new System.Windows.Forms.Button();
            this.btnCpf5 = new System.Windows.Forms.Button();
            this.btnCpf6 = new System.Windows.Forms.Button();
            this.btnCpf7 = new System.Windows.Forms.Button();
            this.btnCpf8 = new System.Windows.Forms.Button();
            this.btnCpf9 = new System.Windows.Forms.Button();
            this.btnCpf0 = new System.Windows.Forms.Button();
            this.btnCpfLimpar = new System.Windows.Forms.Button();
            this.btnCpfRegistrar = new System.Windows.Forms.Button();

            // Arredondar bordas do panelSimulador
            this.panelSimulador.Paint += (s, e) =>
            {
                System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
                int radius = 24;
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(this.panelSimulador.Width - radius, 0, radius, radius, 270, 90);
                path.AddArc(this.panelSimulador.Width - radius, this.panelSimulador.Height - radius, radius, radius, 0, 90);
                path.AddArc(0, this.panelSimulador.Height - radius, radius, radius, 90, 90);
                path.CloseAllFigures();
                this.panelSimulador.Region = new System.Drawing.Region(path);
            };
            this.lblSimulador = new System.Windows.Forms.Label();
            this.btnSimular = new System.Windows.Forms.Button();
            
            this.panelFingerprint = new System.Windows.Forms.Panel();
            this.lblFingerprint = new System.Windows.Forms.Label();
            
            this.lblModoSimulacao = new System.Windows.Forms.Label();
            this.panelStatusBar = new System.Windows.Forms.Panel();
            this.lblStatus = new System.Windows.Forms.Label();
            
            this.timerClock = new System.Windows.Forms.Timer(this.components);
            
            this.panelHeader.SuspendLayout();
            this.panelSimulador.SuspendLayout();
            this.panelCpf.SuspendLayout();
            this.tblCpfKeypad.SuspendLayout();
            this.panelFingerprint.SuspendLayout();
            this.SuspendLayout();
            
            // panelHeader
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(34, 139, 87);
            this.panelHeader.Controls.Add(this.lblTime);
            this.panelHeader.Controls.Add(this.lblDate);
            this.panelHeader.Location = new System.Drawing.Point(60, 20);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Padding = new System.Windows.Forms.Padding(30, 20, 30, 20);
            this.panelHeader.Size = new System.Drawing.Size(780, 140);
            this.panelHeader.TabIndex = 0;
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(16, 118, 128);
            this.panelHeader.Anchor = System.Windows.Forms.AnchorStyles.Top;
            
            // lblTime
            this.lblTime.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTime.Font = new System.Drawing.Font("Segoe UI", 48F, System.Drawing.FontStyle.Bold);
            this.lblTime.ForeColor = System.Drawing.Color.White;
            this.lblTime.Location = new System.Drawing.Point(30, 20);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(720, 70);
            this.lblTime.TabIndex = 0;
            this.lblTime.Text = "00:00:00";
            this.lblTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            
            // lblDate
            this.lblDate.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblDate.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblDate.ForeColor = System.Drawing.Color.White;
            this.lblDate.Location = new System.Drawing.Point(30, 95);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(720, 25);
            this.lblDate.TabIndex = 1;
            this.lblDate.Text = "segunda-feira, 12 de janeiro de 2026";
            this.lblDate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblDate.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
            
            // lblLocalProducao
            this.lblLocalProducao.AutoSize = true;
            this.lblLocalProducao.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblLocalProducao.ForeColor = System.Drawing.Color.FromArgb(100, 50, 150);
            this.lblLocalProducao.Location = new System.Drawing.Point(100, 180);
            this.lblLocalProducao.Name = "lblLocalProducao";
            this.lblLocalProducao.Size = new System.Drawing.Size(180, 25);
            this.lblLocalProducao.TabIndex = 2;
            this.lblLocalProducao.Text = "📍 Local de Produção";
            this.lblLocalProducao.Anchor = System.Windows.Forms.AnchorStyles.Top;
            
            // lblSetorAla
            this.lblSetorAla.AutoSize = true;
            this.lblSetorAla.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblSetorAla.ForeColor = System.Drawing.Color.FromArgb(100, 100, 100);
            this.lblSetorAla.Location = new System.Drawing.Point(100, 220);
            this.lblSetorAla.Name = "lblSetorAla";
            this.lblSetorAla.Size = new System.Drawing.Size(90, 19);
            this.lblSetorAla.TabIndex = 3;
            this.lblSetorAla.Text = "🏢 SETOR / ALA";
            this.lblSetorAla.Anchor = System.Windows.Forms.AnchorStyles.Top;
            
            // cmbSetor
            this.cmbSetor.BackColor = System.Drawing.Color.White;
            this.cmbSetor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSetor.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.cmbSetor.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50);
            this.cmbSetor.FormattingEnabled = true;
            this.cmbSetor.Location = new System.Drawing.Point(100, 245);
            this.cmbSetor.Name = "cmbSetor";
            this.cmbSetor.Size = new System.Drawing.Size(700, 29);
            this.cmbSetor.TabIndex = 4;
            this.cmbSetor.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.cmbSetor.SelectedIndexChanged += new System.EventHandler(this.cmbSetor_SelectedIndexChanged);
            
            // panelSimulador
            this.panelSimulador.BackColor = System.Drawing.Color.White;
            this.panelSimulador.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.panelSimulador.Controls.Add(this.lblSimulador);
            this.panelSimulador.Location = new System.Drawing.Point(100, 320);
            this.panelSimulador.Name = "panelSimulador";
            this.panelSimulador.Size = new System.Drawing.Size(520, 280);
            this.panelSimulador.TabIndex = 5;
            this.panelSimulador.Anchor = System.Windows.Forms.AnchorStyles.Top;

            // panelCpf
            this.panelCpf.BackColor = System.Drawing.Color.White;
            this.panelCpf.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.panelCpf.Controls.Add(this.btnCpfRegistrar);
            this.panelCpf.Controls.Add(this.btnCpfLimpar);
            this.panelCpf.Controls.Add(this.tblCpfKeypad);
            this.panelCpf.Controls.Add(this.txtCpf);
            this.panelCpf.Controls.Add(this.lblCpfTitulo);
            this.panelCpf.Location = new System.Drawing.Point(640, 320);
            this.panelCpf.Name = "panelCpf";
            this.panelCpf.Padding = new System.Windows.Forms.Padding(14, 12, 14, 12);
            this.panelCpf.Size = new System.Drawing.Size(260, 280);
            this.panelCpf.TabIndex = 9;
            this.panelCpf.Anchor = System.Windows.Forms.AnchorStyles.Top;
            
            // lblInstrucao
            this.lblInstrucao.AutoSize = false;
            this.lblInstrucao.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblInstrucao.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
            this.lblInstrucao.Location = new System.Drawing.Point(100, 280);
            this.lblInstrucao.Name = "lblInstrucao";
            this.lblInstrucao.Size = new System.Drawing.Size(700, 25);
            this.lblInstrucao.TabIndex = 8;
            this.lblInstrucao.Text = "Para registrar a producao, selecione o setor e pressione a digital";
            this.lblInstrucao.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblInstrucao.Anchor = System.Windows.Forms.AnchorStyles.Top;
            
            // lblSimulador
            this.lblSimulador.BackColor = System.Drawing.Color.Transparent;
            this.lblSimulador.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSimulador.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblSimulador.ForeColor = System.Drawing.Color.FromArgb(100, 100, 100);
            this.lblSimulador.Location = new System.Drawing.Point(0, 0);
            this.lblSimulador.Name = "lblSimulador";
            this.lblSimulador.Padding = new System.Windows.Forms.Padding(20);
            this.lblSimulador.Size = new System.Drawing.Size(520, 220);
            this.lblSimulador.TabIndex = 0;
            this.lblSimulador.Text = "";
            this.lblSimulador.TextAlign = System.Drawing.ContentAlignment.TopLeft;

            // lblCpfTitulo
            this.lblCpfTitulo.AutoSize = true;
            this.lblCpfTitulo.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblCpfTitulo.ForeColor = System.Drawing.Color.FromArgb(25, 55, 80);
            this.lblCpfTitulo.Location = new System.Drawing.Point(14, 12);
            this.lblCpfTitulo.Name = "lblCpfTitulo";
            this.lblCpfTitulo.Size = new System.Drawing.Size(128, 20);
            this.lblCpfTitulo.TabIndex = 0;
            this.lblCpfTitulo.Text = "Registro por CPF";

            // txtCpf
            this.txtCpf.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this.txtCpf.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCpf.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.txtCpf.ForeColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.txtCpf.Location = new System.Drawing.Point(14, 38);
            this.txtCpf.MaxLength = 11;
            this.txtCpf.Name = "txtCpf";
            this.txtCpf.Size = new System.Drawing.Size(232, 32);
            this.txtCpf.TabIndex = 1;
            this.txtCpf.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtCpf.TextChanged += new System.EventHandler(this.txtCpf_TextChanged);

            // tblCpfKeypad
            this.tblCpfKeypad.ColumnCount = 3;
            this.tblCpfKeypad.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tblCpfKeypad.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tblCpfKeypad.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tblCpfKeypad.Controls.Add(this.btnCpf1, 0, 0);
            this.tblCpfKeypad.Controls.Add(this.btnCpf2, 1, 0);
            this.tblCpfKeypad.Controls.Add(this.btnCpf3, 2, 0);
            this.tblCpfKeypad.Controls.Add(this.btnCpf4, 0, 1);
            this.tblCpfKeypad.Controls.Add(this.btnCpf5, 1, 1);
            this.tblCpfKeypad.Controls.Add(this.btnCpf6, 2, 1);
            this.tblCpfKeypad.Controls.Add(this.btnCpf7, 0, 2);
            this.tblCpfKeypad.Controls.Add(this.btnCpf8, 1, 2);
            this.tblCpfKeypad.Controls.Add(this.btnCpf9, 2, 2);
            this.tblCpfKeypad.Controls.Add(this.btnCpf0, 1, 3);
            this.tblCpfKeypad.Location = new System.Drawing.Point(14, 78);
            this.tblCpfKeypad.Name = "tblCpfKeypad";
            this.tblCpfKeypad.RowCount = 4;
            this.tblCpfKeypad.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblCpfKeypad.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblCpfKeypad.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblCpfKeypad.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblCpfKeypad.Size = new System.Drawing.Size(232, 120);
            this.tblCpfKeypad.TabIndex = 2;

            // btnCpf1
            this.btnCpf1.BackColor = System.Drawing.Color.FromArgb(240, 244, 248);
            this.btnCpf1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCpf1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCpf1.FlatAppearance.BorderSize = 0;
            this.btnCpf1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCpf1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnCpf1.ForeColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.btnCpf1.Name = "btnCpf1";
            this.btnCpf1.Text = "1";
            this.btnCpf1.UseVisualStyleBackColor = false;
            this.btnCpf1.Click += new System.EventHandler(this.CpfDigit_Click);

            // btnCpf2
            this.btnCpf2.BackColor = System.Drawing.Color.FromArgb(240, 244, 248);
            this.btnCpf2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCpf2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCpf2.FlatAppearance.BorderSize = 0;
            this.btnCpf2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCpf2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnCpf2.ForeColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.btnCpf2.Name = "btnCpf2";
            this.btnCpf2.Text = "2";
            this.btnCpf2.UseVisualStyleBackColor = false;
            this.btnCpf2.Click += new System.EventHandler(this.CpfDigit_Click);

            // btnCpf3
            this.btnCpf3.BackColor = System.Drawing.Color.FromArgb(240, 244, 248);
            this.btnCpf3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCpf3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCpf3.FlatAppearance.BorderSize = 0;
            this.btnCpf3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCpf3.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnCpf3.ForeColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.btnCpf3.Name = "btnCpf3";
            this.btnCpf3.Text = "3";
            this.btnCpf3.UseVisualStyleBackColor = false;
            this.btnCpf3.Click += new System.EventHandler(this.CpfDigit_Click);

            // btnCpf4
            this.btnCpf4.BackColor = System.Drawing.Color.FromArgb(240, 244, 248);
            this.btnCpf4.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCpf4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCpf4.FlatAppearance.BorderSize = 0;
            this.btnCpf4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCpf4.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnCpf4.ForeColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.btnCpf4.Name = "btnCpf4";
            this.btnCpf4.Text = "4";
            this.btnCpf4.UseVisualStyleBackColor = false;
            this.btnCpf4.Click += new System.EventHandler(this.CpfDigit_Click);

            // btnCpf5
            this.btnCpf5.BackColor = System.Drawing.Color.FromArgb(240, 244, 248);
            this.btnCpf5.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCpf5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCpf5.FlatAppearance.BorderSize = 0;
            this.btnCpf5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCpf5.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnCpf5.ForeColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.btnCpf5.Name = "btnCpf5";
            this.btnCpf5.Text = "5";
            this.btnCpf5.UseVisualStyleBackColor = false;
            this.btnCpf5.Click += new System.EventHandler(this.CpfDigit_Click);

            // btnCpf6
            this.btnCpf6.BackColor = System.Drawing.Color.FromArgb(240, 244, 248);
            this.btnCpf6.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCpf6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCpf6.FlatAppearance.BorderSize = 0;
            this.btnCpf6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCpf6.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnCpf6.ForeColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.btnCpf6.Name = "btnCpf6";
            this.btnCpf6.Text = "6";
            this.btnCpf6.UseVisualStyleBackColor = false;
            this.btnCpf6.Click += new System.EventHandler(this.CpfDigit_Click);

            // btnCpf7
            this.btnCpf7.BackColor = System.Drawing.Color.FromArgb(240, 244, 248);
            this.btnCpf7.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCpf7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCpf7.FlatAppearance.BorderSize = 0;
            this.btnCpf7.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCpf7.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnCpf7.ForeColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.btnCpf7.Name = "btnCpf7";
            this.btnCpf7.Text = "7";
            this.btnCpf7.UseVisualStyleBackColor = false;
            this.btnCpf7.Click += new System.EventHandler(this.CpfDigit_Click);

            // btnCpf8
            this.btnCpf8.BackColor = System.Drawing.Color.FromArgb(240, 244, 248);
            this.btnCpf8.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCpf8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCpf8.FlatAppearance.BorderSize = 0;
            this.btnCpf8.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCpf8.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnCpf8.ForeColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.btnCpf8.Name = "btnCpf8";
            this.btnCpf8.Text = "8";
            this.btnCpf8.UseVisualStyleBackColor = false;
            this.btnCpf8.Click += new System.EventHandler(this.CpfDigit_Click);

            // btnCpf9
            this.btnCpf9.BackColor = System.Drawing.Color.FromArgb(240, 244, 248);
            this.btnCpf9.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCpf9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCpf9.FlatAppearance.BorderSize = 0;
            this.btnCpf9.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCpf9.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnCpf9.ForeColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.btnCpf9.Name = "btnCpf9";
            this.btnCpf9.Text = "9";
            this.btnCpf9.UseVisualStyleBackColor = false;
            this.btnCpf9.Click += new System.EventHandler(this.CpfDigit_Click);

            // btnCpf0
            this.btnCpf0.BackColor = System.Drawing.Color.FromArgb(240, 244, 248);
            this.btnCpf0.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCpf0.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCpf0.FlatAppearance.BorderSize = 0;
            this.btnCpf0.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCpf0.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnCpf0.ForeColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.btnCpf0.Name = "btnCpf0";
            this.btnCpf0.Text = "0";
            this.btnCpf0.UseVisualStyleBackColor = false;
            this.btnCpf0.Click += new System.EventHandler(this.CpfDigit_Click);

            // btnCpfLimpar
            this.btnCpfLimpar.BackColor = System.Drawing.Color.FromArgb(230, 234, 240);
            this.btnCpfLimpar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCpfLimpar.FlatAppearance.BorderSize = 0;
            this.btnCpfLimpar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCpfLimpar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCpfLimpar.ForeColor = System.Drawing.Color.FromArgb(55, 65, 81);
            this.btnCpfLimpar.Location = new System.Drawing.Point(14, 206);
            this.btnCpfLimpar.Name = "btnCpfLimpar";
            this.btnCpfLimpar.Size = new System.Drawing.Size(232, 26);
            this.btnCpfLimpar.TabIndex = 3;
            this.btnCpfLimpar.Text = "Limpar";
            this.btnCpfLimpar.UseVisualStyleBackColor = false;
            this.btnCpfLimpar.Click += new System.EventHandler(this.btnCpfLimpar_Click);

            // btnCpfRegistrar
            this.btnCpfRegistrar.BackColor = System.Drawing.Color.FromArgb(16, 118, 128);
            this.btnCpfRegistrar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCpfRegistrar.FlatAppearance.BorderSize = 0;
            this.btnCpfRegistrar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCpfRegistrar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCpfRegistrar.ForeColor = System.Drawing.Color.White;
            this.btnCpfRegistrar.Location = new System.Drawing.Point(14, 236);
            this.btnCpfRegistrar.Name = "btnCpfRegistrar";
            this.btnCpfRegistrar.Size = new System.Drawing.Size(232, 30);
            this.btnCpfRegistrar.TabIndex = 4;
            this.btnCpfRegistrar.Text = "Registrar produção";
            this.btnCpfRegistrar.UseVisualStyleBackColor = false;
            this.btnCpfRegistrar.Click += new System.EventHandler(this.btnCpfRegistrar_Click);
            
            // btnSimular
            this.btnSimular.BackColor = System.Drawing.Color.White;
            this.btnSimular.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSimular.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnSimular.FlatAppearance.BorderSize = 2;
            this.btnSimular.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSimular.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnSimular.ForeColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnSimular.Location = new System.Drawing.Point(60, 50);
            this.btnSimular.Name = "btnSimular";
            this.btnSimular.Size = new System.Drawing.Size(400, 35);
            this.btnSimular.TabIndex = 1;
            this.btnSimular.Text = "";
            this.btnSimular.UseVisualStyleBackColor = false;
            this.btnSimular.Visible = false;
            
            // panelFingerprint
            this.panelFingerprint.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.panelFingerprint.Cursor = System.Windows.Forms.Cursors.Default;
            this.panelFingerprint.Location = new System.Drawing.Point(185, 95);
            this.panelFingerprint.Name = "panelFingerprint";
            this.panelFingerprint.Size = new System.Drawing.Size(150, 120);
            this.panelFingerprint.TabIndex = 2;
            this.panelFingerprint.Visible = false;
            
            // lblFingerprint
            this.lblFingerprint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFingerprint.Font = new System.Drawing.Font("Segoe UI", 48F);
            this.lblFingerprint.ForeColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.lblFingerprint.Location = new System.Drawing.Point(0, 0);
            this.lblFingerprint.Name = "lblFingerprint";
            this.lblFingerprint.Size = new System.Drawing.Size(148, 118);
            this.lblFingerprint.TabIndex = 0;
            this.lblFingerprint.Text = "";
            this.lblFingerprint.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblFingerprint.Visible = false;
            
            // lblModoSimulacao
            this.lblModoSimulacao.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblModoSimulacao.ForeColor = System.Drawing.Color.Gray;
            this.lblModoSimulacao.Location = new System.Drawing.Point(20, 225);
            this.lblModoSimulacao.Name = "lblModoSimulacao";
            this.lblModoSimulacao.Size = new System.Drawing.Size(480, 40);
            this.lblModoSimulacao.TabIndex = 3;
            this.lblModoSimulacao.Text = "Modo Simulação Ativo";
            this.lblModoSimulacao.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblModoSimulacao.Visible = false;
            
            // lblStatus
            // panelStatusBar
            this.panelStatusBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelStatusBar.BackColor = System.Drawing.Color.White;
            this.panelStatusBar.Size = new System.Drawing.Size(900, 40);
            this.panelStatusBar.Name = "panelStatusBar";
            this.panelStatusBar.TabIndex = 100;
            this.panelStatusBar.Controls.Add(this.lblStatus);

            // lblStatus
            this.lblStatus.BackColor = System.Drawing.Color.White;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblStatus.ForeColor = System.Drawing.Color.Gray;
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(900, 40);
            this.lblStatus.TabIndex = 6;
            this.lblStatus.Text = "Selecione o setor para ativar o leitor";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblStatus.Padding = new System.Windows.Forms.Padding(0, 8, 0, 8);
            this.lblStatus.BorderStyle = System.Windows.Forms.BorderStyle.None;
            // Bordas arredondadas
            this.lblStatus.Paint += (s, e) => {
                System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
                int radius = 15;
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(this.lblStatus.Width - radius, 0, radius, radius, 270, 90);
                path.AddArc(this.lblStatus.Width - radius, this.lblStatus.Height - radius, radius, radius, 0, 90);
                path.AddArc(0, this.lblStatus.Height - radius, radius, radius, 90, 90);
                path.CloseAllFigures();
                this.lblStatus.Region = new System.Drawing.Region(path);
            };
            
            // timerClock
            this.timerClock.Enabled = true;
            this.timerClock.Interval = 1000;
            this.timerClock.Tick += new System.EventHandler(this.timerClock_Tick);
            
            // LoginForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(240, 242, 245);
            this.ClientSize = new System.Drawing.Size(900, 680);
            this.Padding = new System.Windows.Forms.Padding(0, 0, 0, 0); // Espaço inferior removido, pois o painel já ocupa
            this.Controls.Add(this.panelSimulador);
            this.Controls.Add(this.panelCpf);
            this.Controls.Add(this.lblInstrucao);
            this.Controls.Add(this.cmbSetor);
            this.Controls.Add(this.lblSetorAla);
            this.Controls.Add(this.lblLocalProducao);
            this.Controls.Add(this.panelHeader);
            this.Controls.Add(this.panelStatusBar); // Adiciona a barra de status dockada embaixo
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                        this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Sistema Biométrico - Controle de Produção";
            this.Resize += new System.EventHandler(this.LoginForm_Resize);
            
            this.panelHeader.ResumeLayout(false);
            this.panelSimulador.ResumeLayout(false);
            this.panelCpf.ResumeLayout(false);
            this.panelCpf.PerformLayout();
            this.tblCpfKeypad.ResumeLayout(false);
            this.panelFingerprint.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.Label lblDate;
        private System.Windows.Forms.Label lblLocalProducao;
        private System.Windows.Forms.Label lblSetorAla;
        private System.Windows.Forms.ComboBox cmbSetor;
        private System.Windows.Forms.Label lblInstrucao;
        private System.Windows.Forms.Panel panelSimulador;
        private System.Windows.Forms.Label lblSimulador;
        private System.Windows.Forms.Panel panelCpf;
        private System.Windows.Forms.Label lblCpfTitulo;
        private System.Windows.Forms.TextBox txtCpf;
        private System.Windows.Forms.TableLayoutPanel tblCpfKeypad;
        private System.Windows.Forms.Button btnCpf1;
        private System.Windows.Forms.Button btnCpf2;
        private System.Windows.Forms.Button btnCpf3;
        private System.Windows.Forms.Button btnCpf4;
        private System.Windows.Forms.Button btnCpf5;
        private System.Windows.Forms.Button btnCpf6;
        private System.Windows.Forms.Button btnCpf7;
        private System.Windows.Forms.Button btnCpf8;
        private System.Windows.Forms.Button btnCpf9;
        private System.Windows.Forms.Button btnCpf0;
        private System.Windows.Forms.Button btnCpfLimpar;
        private System.Windows.Forms.Button btnCpfRegistrar;
        private System.Windows.Forms.Button btnSimular;
        private System.Windows.Forms.Panel panelFingerprint;
        private System.Windows.Forms.Label lblFingerprint;
        private System.Windows.Forms.Label lblModoSimulacao;
        private System.Windows.Forms.Panel panelStatusBar;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Timer timerClock;
    }
}
