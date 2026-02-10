namespace BiometricSystem.Forms
{
    partial class AccessMenuForm
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnControleProd = new System.Windows.Forms.Button();
            this.btnCadastrarBiometria = new System.Windows.Forms.Button();

            this.panelHeader.SuspendLayout();
            this.SuspendLayout();

            // panelHeader
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(16, 118, 128);
            this.panelHeader.Controls.Add(this.lblTitle);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(600, 120);
            this.panelHeader.TabIndex = 0;

            // lblTitle
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(600, 120);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Menu de Acesso";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // btnControleProd
            this.btnControleProd.BackColor = System.Drawing.Color.FromArgb(34, 139, 87);
            this.btnControleProd.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnControleProd.FlatAppearance.BorderSize = 0;
            this.btnControleProd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnControleProd.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.btnControleProd.ForeColor = System.Drawing.Color.White;
            this.btnControleProd.Location = new System.Drawing.Point(50, 180);
            this.btnControleProd.Name = "btnControleProd";
            this.btnControleProd.Size = new System.Drawing.Size(500, 80);
            this.btnControleProd.TabIndex = 1;
            this.btnControleProd.Text = "📊 Controle de Produção";
            this.btnControleProd.UseVisualStyleBackColor = false;
            this.btnControleProd.Click += new System.EventHandler(this.btnControleProd_Click);

            // btnCadastrarBiometria
            this.btnCadastrarBiometria.BackColor = System.Drawing.Color.FromArgb(33, 150, 243);
            this.btnCadastrarBiometria.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCadastrarBiometria.FlatAppearance.BorderSize = 0;
            this.btnCadastrarBiometria.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCadastrarBiometria.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.btnCadastrarBiometria.ForeColor = System.Drawing.Color.White;
            this.btnCadastrarBiometria.Location = new System.Drawing.Point(50, 300);
            this.btnCadastrarBiometria.Name = "btnCadastrarBiometria";
            this.btnCadastrarBiometria.Size = new System.Drawing.Size(500, 80);
            this.btnCadastrarBiometria.TabIndex = 2;
            this.btnCadastrarBiometria.Text = "👆 Cadastrar Biometria";
            this.btnCadastrarBiometria.UseVisualStyleBackColor = false;
            this.btnCadastrarBiometria.Click += new System.EventHandler(this.btnCadastrarBiometria_Click);

            // AccessMenuForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(240, 242, 245);
            this.ClientSize = new System.Drawing.Size(600, 450);
            this.Controls.Add(this.btnCadastrarBiometria);
            this.Controls.Add(this.btnControleProd);
            this.Controls.Add(this.panelHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AccessMenuForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Menu de Acesso";

            this.panelHeader.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnControleProd;
        private System.Windows.Forms.Button btnCadastrarBiometria;
    }
}
