using BiometricSystem.Database;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BiometricSystem.Forms
{
    public partial class AccessMenuForm : Form
    {
        private LoginForm? parentForm;
        private readonly IConfiguration _config;
        private bool voltarParaProducao = false;

        public AccessMenuForm(LoginForm parent, IConfiguration config)
        {
            InitializeComponent();
            parentForm = parent;
            _config = config;
            
            // Adicionar handler para fechar o sistema quando fechar esta tela
            this.FormClosing += AccessMenuForm_FormClosing;
        }

        private void AccessMenuForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Se não foi clicado no botão "Controle de Produção"
            if (!voltarParaProducao)
            {
                // Permitir que a LoginForm feche realmente
                if (parentForm != null)
                {
                    parentForm.AllowClose = true;
                    // Sinalizar para o LoginForm que deve fechar
                    parentForm.Close();
                }
                // Não chama Application.Exit() aqui!
            }
        }

        private void btnControleProd_Click(object sender, EventArgs e)
        {
            // Marca que quer voltar para produção
            voltarParaProducao = true;
            if (parentForm != null)
            {
                parentForm.AllowClose = false;
                parentForm.VoltarDaProducao = true;
                parentForm.WindowState = FormWindowState.Maximized;
                parentForm.Show();
                parentForm.TopMost = true;
            }
            this.Close();
        }

        private void btnCadastrarBiometria_Click(object sender, EventArgs e)
        {
            try
            {
                var biometriaForm = new CadastrarBiometriaForm(_config);
                biometriaForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erro ao abrir tela de cadastro de biometria:\n{ex.Message}",
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
