using System;
using System.Windows.Forms;

namespace BiometricSystem.Forms
{
    public partial class CadastroSenhaLocalForm : Form
    {
        public string Senha => txtSenha.Text;
        public string Confirmacao => txtConfirmacao.Text;
        public CadastroSenhaLocalForm()
        {
            InitializeComponent();
        }
    }
}
