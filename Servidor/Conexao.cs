using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Servidor
{
    public class Conexao
    {
        TcpClient tcpCliente;

        private Thread thrSender;
        private StreamReader srReceptor;
        private StreamWriter swEnviador;
        private string usuarioAtual;
        private string strResposta;

        public Conexao(TcpClient tcpCon)
        {
            tcpCliente = tcpCon;
            thrSender = new Thread(AceitaCliente);
            thrSender.Start();
        }

        private void FechaConexao()
        {
            tcpCliente.Close();
            srReceptor.Close();
            swEnviador.Close();
        }

        private void AceitaCliente()
        {
            srReceptor = new StreamReader(tcpCliente.GetStream());
            swEnviador = new StreamWriter(tcpCliente.GetStream());

            usuarioAtual = srReceptor.ReadLine();

            if (usuarioAtual == string.Empty)
            {
                FechaConexao();
                return;
            }


            if (ChatServidor.htUsuarios.Contains(usuarioAtual))
            {
                swEnviador.WriteLine("0|Este nome de usuário já existe.");
                swEnviador.Flush();
                FechaConexao();
                return;
            }
            else if (usuarioAtual == "Administrator")
            {
                swEnviador.WriteLine("0|Este nome de usuário é reservado.");
                swEnviador.Flush();
                FechaConexao();
                return;
            }
            else
            {
                swEnviador.WriteLine("1");
                swEnviador.Flush();

                ChatServidor.IncluiUsuario(tcpCliente, usuarioAtual);
            }

            try
            {
                while ((strResposta = srReceptor.ReadLine()) != string.Empty)
                {
                    if (strResposta is null)
                    {
                        ChatServidor.RemoveUsuario(tcpCliente);
                        continue;
                    }

                    ChatServidor.EnviaMensagem(usuarioAtual, strResposta);
                }
            }
            catch
            {
                ChatServidor.RemoveUsuario(tcpCliente);
            }
        }
    }
}
