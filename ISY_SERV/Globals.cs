using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ISY_SERV
{
    public delegate void TCPTerminal_MessageRecivedDel(Socket socket, byte[] message);
    public delegate void TCPTerminal_ConnectDel(Socket socket);
    public delegate void TCPTerminal_DisconnectDel(Socket socket);
}
