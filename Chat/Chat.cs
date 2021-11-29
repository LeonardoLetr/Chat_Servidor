using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chat
{
    public partial class Chat : Form
    {
        private string NomeUsuario = "Desconhecido";
        private StreamWriter stwEnviador;
        private StreamReader strReceptor;
        private TcpClient tcpServidor;

        private delegate void AtualizaLogCallBack(string strMensagem);
        private delegate void FechaConexaoCallBack(string strMotivo);

        private Thread mensagemThread;
        private IPAddress enderecoIP;
        private bool conectado;

        public Chat()
        {

            Application.ApplicationExit += new EventHandler(OnaApplicationExit);

            InitializeComponent();
        }

        #region Eventos de Tela

        private void Chat_Load(object sender, EventArgs e)
        {

        }

        private void btnConectar_Click(object sender, EventArgs e)
        {
            if (!conectado)
            {
                InicializaConexao();
                return;
            }

            FechaConexao("Desconctado a pedido do usuário");
        }

        private void btnEnviar_Click(object sender, EventArgs e)
        {
            EnviaMensagem();
        }

        private void txtMensagem_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.Equals((char)13))
            {
                EnviaMensagem();
            }
        }

        #endregion

        #region Métodos Privados

        private void InicializaConexao()
        {
            InicializaServidor();

            DefinirEstadoConexao(true);

            ConfigurandoFormConexacaoAberta();

            EnviarUsuarioAoServidor();

            InicializaThreadDeComunicacao();
        }

        private void InicializaServidor()
        {
            enderecoIP = IPAddress.Parse(txtServidorIP.Text);

            tcpServidor = new TcpClient();
            tcpServidor.Connect(enderecoIP, 2502);
        }

        private void DefinirEstadoConexao(bool v)
        {
            conectado = v;
        }

        private bool ObterEstadoConexao()
        {
            return conectado;
        }

        private void ConfigurandoFormConexacaoAberta()
        {
            NomeUsuario = txtUsuario.Text;

            txtServidorIP.Enabled = false;
            txtUsuario.Enabled = false;
            txtMensagem.Enabled = true;
            btnEnviar.Enabled = true;
            btnConectar.Text = "Desconectar";
        }

        private void ConfigurandoFormConexacaoFechada()
        {
            txtServidorIP.Enabled = true;
            txtUsuario.Enabled = true;
            txtMensagem.Enabled = false;
            btnEnviar.Enabled = false;
            btnConectar.Text = "Conectar";
        }

        private void EnviarUsuarioAoServidor()
        {
            stwEnviador = new StreamWriter(tcpServidor.GetStream());
            stwEnviador.WriteLine(txtUsuario.Text);
            stwEnviador.Flush();
        }

        private void InicializaThreadDeComunicacao()
        {
            mensagemThread = new Thread(new ThreadStart(RecebeMensagens));
            mensagemThread.Start();
        }

        private void RecebeMensagens()
        {
            strReceptor = new StreamReader(tcpServidor.GetStream());
            var conResposta = strReceptor.ReadLine();

            if (conResposta[0].Equals("0"))
            {
                var motivo = string.Format($"Não Conectado: {conResposta.Substring(2, conResposta.Length - 2)}");
                Invoke(new FechaConexaoCallBack(this.FechaConexao), new object[] { motivo });
                
                return;
            }

            Invoke(new AtualizaLogCallBack(this.AtualizaLog), new object[] { "Conectado com sucesso!" });

            while(ObterEstadoConexao())
            {
                Invoke(new AtualizaLogCallBack(this.AtualizaLog), new object[] { strReceptor.ReadLine() });
            }
        }

        private void AtualizaLog(string mensagem)
        {
            txtLog.AppendText(mensagem + "\r\n");
        }

        private void EnviaMensagem()
        {
            if (txtMensagem.Lines.Length >= 1)
            {
                stwEnviador.WriteLine(txtMensagem.Text);
                stwEnviador.Flush();

                txtMensagem.Lines = null;
            }

            txtMensagem.Text = string.Empty;
        }

        private void FechaConexao(string motivo)
        {
            txtLog.AppendText(motivo + "\r\n");

            ConfigurandoFormConexacaoFechada();

            FinalizaProcessos();
        }

        private void FinalizaProcessos()
        {
            DefinirEstadoConexao(false);

            stwEnviador.Close();
            strReceptor.Close();
            tcpServidor.Close();
        }

        public void OnaApplicationExit(object sender, EventArgs e)
        {
            if (ObterEstadoConexao())
            {
                FinalizaProcessos();
            }
        }

        #endregion
    }
}
