using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ISY_SERV
{
   public class ServerTerminal
    {
        public event TCPTerminal_MessageRecivedDel MessageRecived;
        public event TCPTerminal_ConnectDel ClientConnect;
        public event TCPTerminal_DisconnectDel ClientDisconnect;

        private Socket m_socket;
        private bool m_Closed;

        private Dictionary<long, ConnectedClient> m_clients =
            new Dictionary<long, ConnectedClient>();

        public void StartListen()
        {
            IPEndPoint ipLocal = new IPEndPoint(Dns.GetHostAddresses(System.Environment.MachineName)[1], 11000);
            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                m_socket.Bind(ipLocal);
                Console.WriteLine(Dns.GetHostAddresses(System.Environment.MachineName)[1]);

            }
            catch (Exception e)
            {
                Debug.Fail(e.ToString(), string.Format("Can't connect to port {0}!", 11000));

                return;
            }

            m_socket.Listen(4);

            m_socket.BeginAccept(new AsyncCallback(OnClientConnection), null);
        }

        private void OnClientConnection(IAsyncResult asyn)
        {
            if (m_Closed)
            {
                return;
            }
            try
            {
                Socket clientSocket = m_socket.EndAccept(asyn);
                ConnectedClient connectedClient = new ConnectedClient(clientSocket);

                RaiseClientConnected(clientSocket);
                connectedClient.MessageRecived += OnMessageRecived;
                connectedClient.Disconnected += OnClientDisconnection;

                connectedClient.StartListen();

                long key = clientSocket.Handle.ToInt64();
                if (m_clients.ContainsKey(key))
                {
                    Debug.Fail(string.Format(
                     "Client with handle key '{0}' already exist!", key));
                }
                m_clients[key] = connectedClient;
                m_socket.BeginAccept(new AsyncCallback(OnClientConnection), null);

            }
            catch (ObjectDisposedException odex)
            {
                Debug.Fail(odex.ToString(),
                 "OnClientConnection: Socket has been closed");
            }
            catch (Exception sex)
            {
                Debug.Fail(sex.ToString(),
                "OnClientConnection: Socket failed");
            }
        }
        private void OnClientDisconnection(Socket socket)
        {
            RaiseClientDisconnected(socket);

            long key = socket.Handle.ToInt64();
            if (m_clients.ContainsKey(key))
            {
                m_clients.Remove(key);
            }
            else
            {
                Debug.Fail(string.Format(
                    "Unknown client '{0}' has been disconnected!", key));
            }
        }
        public void DistributeMessage(byte[] buffer)
        {
            try
            {
                foreach (ConnectedClient connectedClient in m_clients.Values)
                {
                    connectedClient.Send(buffer);
                }
            }
            catch (SocketException se)
            {
                Debug.Fail(se.ToString(), string.Format(
                   "Buffer could not be sent"));
            }
        }

        public void Close()
        {
            try
            {
                if (m_socket != null)
                {
                    m_Closed = true;

                    // Close the clients
                    foreach (ConnectedClient connectedClient in m_clients.Values)
                    {
                        connectedClient.Stop();
                    }

                    m_socket.Close();

                    m_socket = null;
                }
            }
            catch (ObjectDisposedException odex)
            {
                Debug.Fail(odex.ToString(), "Stop failed");
            }
        }

        private void RaiseClientConnected(Socket socket)
        {
            if (ClientConnect != null)
            {
                ClientConnect(socket);
            }
        }

        private void RaiseClientDisconnected(Socket socket)
        {
            if (ClientDisconnect != null)
            {
                ClientDisconnect(socket);
            }
        }


        private void OnMessageRecived(Socket socket, byte[] buffer)
        {
            if (MessageRecived != null)
            {
                MessageRecived(socket, buffer);
            }
        }
    }
}
