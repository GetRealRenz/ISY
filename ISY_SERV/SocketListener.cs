using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ISY_SERV
{
   public class SocketListener
    {
        private const int BufferLength = 1000;
        AsyncCallback pfnWorkerCallBack;
        Socket m_socWorker;


        public event TCPTerminal_MessageRecivedDel MessageRecived;
        public event TCPTerminal_DisconnectDel Disconnected;


        public void StartReciving(Socket socket)
        {
            m_socWorker = socket;
            WaitForData(socket);
        }


        private void WaitForData(Socket soc)
        {
            try
            {
                if (pfnWorkerCallBack == null)
                {
                    pfnWorkerCallBack = new AsyncCallback(OnDataReceived);
                }

                CSocketPacket theSocPkt = new CSocketPacket(BufferLength);
                theSocPkt.thisSocket = soc;

                soc.BeginReceive(theSocPkt.dataBuffer,
                  0,
                  theSocPkt.dataBuffer.Length,
                  SocketFlags.None,
                  pfnWorkerCallBack,
                  theSocPkt);
            }
            catch (SocketException sex)
            {
                Debug.Fail(sex.ToString(), "WaitForData: Socket failed");
            }
        }

        private void OnDataReceived(IAsyncResult asyn)
        {
            CSocketPacket theSockId = (CSocketPacket)asyn.AsyncState;
            Socket socket = theSockId.thisSocket;

            if (!socket.Connected)
            {
                return;
            }

            try
            {
                int iRx;
                try
                {
                    iRx = socket.EndReceive(asyn);
                }
                catch (SocketException)
                {
                    Debug.Write("Client has been closed and cannot answer.");

                    OnConnectionDroped(socket);
                    return;
                }

                if (iRx == 0)
                {
                    Debug.Write("Client socket has been closed.");

                    OnConnectionDroped(socket);
                    return;
                }

                RaiseMessageRecived(theSockId.dataBuffer);
                // Wait for the next package
                WaitForData(m_socWorker);
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString(), "OnClientConnection: Socket failed");
            }
        }

        public void StopListening()
        {
            if (m_socWorker != null)
            {
                m_socWorker.Close();
                m_socWorker = null;
            }
        }

        private void RaiseMessageRecived(byte[] buffer)
        {
            if (MessageRecived != null)
            {
                MessageRecived(m_socWorker, buffer);
            }
        }

        private void OnDisconnection(Socket socket)
        {
            if (Disconnected != null)
            {
                Disconnected(socket);
            }
        }

        private void OnConnectionDroped(Socket socket)
        {
            m_socWorker = null;
            OnDisconnection(socket);
        }
    }

   public class CSocketPacket
   {
       public System.Net.Sockets.Socket thisSocket;
       public byte[] dataBuffer;

       public CSocketPacket(int buffeLength)
       {
           dataBuffer = new byte[buffeLength];
       }
    }
}
