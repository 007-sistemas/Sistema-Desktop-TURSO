namespace BiometricSystem.Forms
{
    partial class AuthDialogForm
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
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.panelTeclado = new System.Windows.Forms.Panel();
            this.btnNum0 = new System.Windows.Forms.Button();
            this.btnNum1 = new System.Windows.Forms.Button();
            this.btnNum2 = new System.Windows.Forms.Button();
            this.btnNum3 = new System.Windows.Forms.Button();
            this.btnNum4 = new System.Windows.Forms.Button();
            this.btnNum5 = new System.Windows.Forms.Button();
            this.btnNum6 = new System.Windows.Forms.Button();
            this.btnNum7 = new System.Windows.Forms.Button();
            this.btnNum8 = new System.Windows.Forms.Button();
            this.btnNum9 = new System.Windows.Forms.Button();
                this.btnLimparPrincipal = new System.Windows.Forms.Button();


            this.SuspendLayout();
            // 
            // lblPassword
            // 
            this.lblPassword.Text = "Senha";
            this.lblPassword.Location = new System.Drawing.Point(0, 20);
            this.lblPassword.Size = new System.Drawing.Size(320, 25);
            this.lblPassword.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblPassword.Font = new System.Drawing.Font("Arial", 11, System.Drawing.FontStyle.Bold);
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(75, 50);
            this.txtPassword.Size = new System.Drawing.Size(170, 28);
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Font = new System.Drawing.Font("Arial", 12);
            this.txtPassword.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // panelTeclado
            this.panelTeclado.Location = new System.Drawing.Point(10, 90);
            this.panelTeclado.Size = new System.Drawing.Size(300, 120);
            //
            // Botões numéricos e Limpar
            //
            int btnW = 60, btnH = 35, pad = 5;
            int offset = (300 - (3 * (btnW + pad))) / 2; // Centralizar o teclado dentro do painel
            // Linha 1: 1 2 3
            this.btnNum1.Text = "1";
            this.btnNum1.Size = new System.Drawing.Size(btnW, btnH);
            this.btnNum1.Location = new System.Drawing.Point(offset, 0);
            this.btnNum2.Text = "2";
            this.btnNum2.Size = new System.Drawing.Size(btnW, btnH);
            this.btnNum2.Location = new System.Drawing.Point(offset + btnW + pad, 0);
            this.btnNum3.Text = "3";
            this.btnNum3.Size = new System.Drawing.Size(btnW, btnH);
            this.btnNum3.Location = new System.Drawing.Point(offset + 2 * (btnW + pad), 0);
            // Linha 2: 4 5 6
            this.btnNum4.Text = "4";
            this.btnNum4.Size = new System.Drawing.Size(btnW, btnH);
            this.btnNum4.Location = new System.Drawing.Point(offset, 1 * (btnH + pad));
            this.btnNum5.Text = "5";
            this.btnNum5.Size = new System.Drawing.Size(btnW, btnH);
            this.btnNum5.Location = new System.Drawing.Point(offset + btnW + pad, btnH + pad);
            this.btnNum6.Text = "6";
            this.btnNum6.Size = new System.Drawing.Size(btnW, btnH);
            this.btnNum6.Location = new System.Drawing.Point(offset + 2 * (btnW + pad), btnH + pad);
            // Linha 3: 7 8 9
            this.btnNum7.Text = "7";
            this.btnNum7.Size = new System.Drawing.Size(btnW, btnH);
            this.btnNum7.Location = new System.Drawing.Point(offset, 2 * (btnH + pad));
            this.btnNum8.Text = "8";
            this.btnNum8.Size = new System.Drawing.Size(btnW, btnH);
            this.btnNum8.Location = new System.Drawing.Point(offset + btnW + pad, 2 * (btnH + pad));
            this.btnNum9.Text = "9";
            this.btnNum9.Size = new System.Drawing.Size(btnW, btnH);
            this.btnNum9.Location = new System.Drawing.Point(offset + 2 * (btnW + pad), 2 * (btnH + pad));
            // Linha 4: apenas 0 centralizado
            this.btnNum0.Text = "0";
            this.btnNum0.Size = new System.Drawing.Size(btnW, btnH);
            this.btnNum0.Location = new System.Drawing.Point(offset + btnW + pad, 3 * (btnH + pad));
            // Adiciona botões ao painel
            this.panelTeclado.Controls.Add(this.btnNum1);
            this.panelTeclado.Controls.Add(this.btnNum2);
            this.panelTeclado.Controls.Add(this.btnNum3);
            this.panelTeclado.Controls.Add(this.btnNum4);
            this.panelTeclado.Controls.Add(this.btnNum5);
            this.panelTeclado.Controls.Add(this.btnNum6);
            this.panelTeclado.Controls.Add(this.btnNum7);
            this.panelTeclado.Controls.Add(this.btnNum8);
            this.panelTeclado.Controls.Add(this.btnNum9);
            this.panelTeclado.Controls.Add(this.btnNum0);
            // 
            this.btnOk.Text = "OK";
            this.btnOk.Location = new System.Drawing.Point(50, 220);
            this.btnOk.Size = new System.Drawing.Size(70, 30);
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            // 
            // btnCancel
            // 
            this.btnCancel.Text = "Cancelar";
            this.btnCancel.Location = new System.Drawing.Point(125, 220);
            this.btnCancel.Size = new System.Drawing.Size(70, 30);
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            // 
            // btnLimparPrincipal
            // 
            this.btnLimparPrincipal.Text = "Limpar";
            this.btnLimparPrincipal.Size = new System.Drawing.Size(70, 30);
            this.btnLimparPrincipal.Location = new System.Drawing.Point(200, 220);
            // 
            // AuthDialogForm
            // 
            this.AcceptButton = this.btnOk;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(320, 270);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.panelTeclado);
            // Configuração do botão Limpar
            this.btnLimparPrincipal.Text = "Limpar";
            this.btnLimparPrincipal.Size = new System.Drawing.Size(70, 30);
            this.btnLimparPrincipal.Location = new System.Drawing.Point(200, 220);
            this.Controls.Add(this.btnLimparPrincipal);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Autenticação Administrativa";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Panel panelTeclado;
        private System.Windows.Forms.Button btnNum0;
        private System.Windows.Forms.Button btnNum1;
        private System.Windows.Forms.Button btnNum2;
        private System.Windows.Forms.Button btnNum3;
        private System.Windows.Forms.Button btnNum4;
        private System.Windows.Forms.Button btnNum5;
        private System.Windows.Forms.Button btnNum6;
        private System.Windows.Forms.Button btnNum7;
        private System.Windows.Forms.Button btnNum8;
        private System.Windows.Forms.Button btnNum9;
        private System.Windows.Forms.Button btnLimparPrincipal;
    }
}
