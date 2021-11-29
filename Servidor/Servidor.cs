using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Servidor
{
    public partial class Servidor : Form
    {
        private delegate void AtualizaStatusCallback(string strMensagem);

        public Servidor()
        {
            InitializeComponent();
        }

        private void btnAtender_Click(object sender, EventArgs e)
        {
            if (txtIP.Text == string.Empty)
            {
                MessageBox.Show("Informe o endereço IP.");
                txtIP.Focus();
                return;
            }

            try
            {
                var enderecoIP = IPAddress.Parse(txtIP.Text);
                IniciarConexao(enderecoIP);

                txtLog.AppendText("Monitorando as conexões...\r\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro de conexão : {ex.Message}");
            }
        }

        private void IniciarConexao(IPAddress enderecoIP)
        {
            var mainServidor = new ChatServidor(enderecoIP);
            ChatServidor.StatusChanged += new StatusChangedEventHandler(mainServidor_StatusChanged);

            mainServidor.IniciaAtendimento();
        }

        public void mainServidor_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            Invoke(new AtualizaStatusCallback(AtualizaStatus), new object[] { e.EventMessage });
        }

        private void AtualizaStatus(string strMensagem)
        {
            txtLog.AppendText(strMensagem + "\r\n");
        }
    }
}
