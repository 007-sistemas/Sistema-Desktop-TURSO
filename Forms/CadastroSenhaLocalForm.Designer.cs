namespace BiometricSystem.Forms
{
    partial class CadastroSenhaLocalForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblSenha;
        private System.Windows.Forms.Label lblConfirmacao;
        private System.Windows.Forms.TextBox txtSenha;
        private System.Windows.Forms.TextBox txtConfirmacao;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancelar;

        private void InitializeComponent()
        {
            this.lblSenha = new System.Windows.Forms.Label();
            this.lblConfirmacao = new System.Windows.Forms.Label();
            this.txtSenha = new System.Windows.Forms.TextBox();
            this.txtConfirmacao = new System.Windows.Forms.TextBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblSenha
            // 
            this.lblSenha.Text = "Senha:";
            this.lblSenha.Location = new System.Drawing.Point(20, 30);
            this.lblSenha.Size = new System.Drawing.Size(80, 20);
            // 
            // txtSenha
            // 
            this.txtSenha.Location = new System.Drawing.Point(110, 30);
            this.txtSenha.Size = new System.Drawing.Size(180, 23);
            this.txtSenha.PasswordChar = '*';
            // 
            // lblConfirmacao
            // 
            this.lblConfirmacao.Text = "Confirme a senha:";
            this.lblConfirmacao.Location = new System.Drawing.Point(20, 70);
            this.lblConfirmacao.Size = new System.Drawing.Size(120, 20);
            // 
            // txtConfirmacao
            // 
            this.txtConfirmacao.Location = new System.Drawing.Point(140, 70);
            this.txtConfirmacao.Size = new System.Drawing.Size(150, 23);
            this.txtConfirmacao.PasswordChar = '*';
            // 
            // btnOk
            // 
            this.btnOk.Text = "OK";
            this.btnOk.Location = new System.Drawing.Point(110, 110);
            this.btnOk.Size = new System.Drawing.Size(80, 30);
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            // 
            // btnCancelar
            // 
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.Location = new System.Drawing.Point(210, 110);
            this.btnCancelar.Size = new System.Drawing.Size(80, 30);
            this.btnCancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            // 
            // CadastroSenhaLocalForm
            // 
            this.AcceptButton = this.btnOk;
            this.CancelButton = this.btnCancelar;
            this.ClientSize = new System.Drawing.Size(320, 170);
            this.Controls.Add(this.lblSenha);
            this.Controls.Add(this.txtSenha);
            this.Controls.Add(this.lblConfirmacao);
            this.Controls.Add(this.txtConfirmacao);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnCancelar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Cadastrar Senha Local";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
