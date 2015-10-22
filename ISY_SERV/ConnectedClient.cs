using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ISY_SERV
{
   public class ConnectedClient
    {
        private Socket m_clientSocket;
        SocketListener m_listener;


        public ConnectedClient(Socket clientSocket)
        {
            m_clientSocket = clientSocket;
            m_listener = new SocketListener();
        }

        public event TCPTerminal_MessageRecivedDel MessageRecived
        {
            add
            {
                m_listener.MessageRecived += value;
            }
            remove
            {
                m_listener.MessageRecived -= value;
            }
        }

        public event TCPTerminal_DisconnectDel Disconnected
        {
            add
            {
                m_listener.Disconnected += value;
            }
            remove
            {
                m_listener.Disconnected -= value;
            }
        }

        public void StartListen()
        {
            m_listener.StartReciving(m_clientSocket);
        }

        public void Send(byte[] buffer)
        {
            if (m_clientSocket == null)
            {
                throw new Exception("Can't send data. ConnectedClient is Closed!");
            }

            m_clientSocket.Send(buffer);
        }

        public void Stop()
        {
            m_listener.StopListening();
            m_clientSocket = null;
        }
    }
}
