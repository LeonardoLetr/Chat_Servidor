using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Servidor
{
    public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs e);

    public class ChatServidor
    {
        public static Hashtable htUsuarios = new(30);
        public static Hashtable htConexoes = new(30);

        private readonly IPAddress enderecoIP;
        private TcpClient tcpCliente;

        public static event StatusChangedEventHandler StatusChanged;
        private static StatusChangedEventArgs e;

        public ChatServidor(IPAddress endereco)
        {
            enderecoIP = endereco;
        }

        private Thread thrListener;

        private TcpListener tlsCliente;

        bool ServRodando = false;

        public static void IncluiUsuario(TcpClient tcpUsuario, string strUsername)
        {
            htUsuarios.Add(strUsername, tcpUsuario);
            htConexoes.Add(tcpUsuario, strUsername);

            EnviaMensagemAdmin($"{htConexoes[tcpUsuario]} entrou..");
        }

        public static void RemoveUsuario(TcpClient tcpUsuario)
        {
            if (htConexoes[tcpUsuario] is not null)
            {
                EnviaMensagemAdmin($"{htConexoes[tcpUsuario]} saiu...");

                htUsuarios.Remove(htConexoes[tcpUsuario]);
                htConexoes.Remove(tcpUsuario);
            }
        }

        public static void OnStatusChanged(StatusChangedEventArgs e)
        {
            var statusHandler = StatusChanged;

            if (statusHandler is not null)
            {
                statusHandler(null, e);
            }
        }

        public static void EnviaMensagemAdmin(string Mensagem)
        {
            e = new StatusChangedEventArgs($"Administrador: {Mensagem}");
            OnStatusChanged(e);

            var tcpClientes = new TcpClient[htUsuarios.Count];

            htUsuarios.Values.CopyTo(tcpClientes, 0);

            for (int i = 0; i < tcpClientes.Length; i++)
            {
                try
                {
                    if (Mensagem.Trim() == string.Empty || tcpClientes[i] is null)
                    {
                        continue;
                    }

                    var swSenderSender = new StreamWriter(tcpClientes[i].GetStream());
                    swSenderSender.WriteLine($"Administrador: {Mensagem}");
                    swSenderSender.Flush();
                    swSenderSender = null;
                }
                catch 
                {
                    RemoveUsuario(tcpClientes[i]);
                }
            }
        }

        public static void EnviaMensagem(string Origem, string Mensagem)
        {
            StreamWriter swSenderSender;

            e = new StatusChangedEventArgs($"{Origem} disse: {Mensagem}");
            OnStatusChanged(e);

            var tcpClientes = new TcpClient[htUsuarios.Count];

            htUsuarios.Values.CopyTo(tcpClientes, 0);

            for (int i = 0; i < tcpClientes.Length; i++)
            {
                try
                {
                    if (Mensagem.Trim() == string.Empty || tcpClientes[i] is null)
                    {
                        continue;
                    }

                    swSenderSender = new StreamWriter(tcpClientes[i].GetStream());
                    swSenderSender.WriteLine($"{Origem} disse: {Mensagem}");
                    swSenderSender.Flush();
                    swSenderSender = null;
                }
                catch
                {
                    RemoveUsuario(tcpClientes[i]);
                }
            }
        }

        public void IniciaAtendimento()
        {
            try
            {
                var ipaLocal = enderecoIP;

                tlsCliente = new TcpListener(ipaLocal, 2502);
                tlsCliente.Start();

                ServRodando = true;

                thrListener = new Thread(MantemAtendimento);
                thrListener.Start();
            }
            catch
            {
                throw;
            }
        }

        private void MantemAtendimento()
        {
            while (ServRodando)
            {
                tcpCliente = tlsCliente.AcceptTcpClient();
                _ = new Conexao(tcpCliente);
            }
        }
    }
}
