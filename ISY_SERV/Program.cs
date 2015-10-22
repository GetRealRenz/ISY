using ISY.Domain.Abstract;
using ISY.Domain.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using ISY.Domain.Concrete;

namespace ISY_SERV
{
    
    class Program
    {
        static IKernel kernel;
        static ServerTerminal m_ServerTerminal;
        static void Main(string[] args)
        {
            kernel = new StandardKernel();
            kernel.Bind<IRepositoryBase<Profile>>().To<RepositoryBase<Profile>>();

            m_ServerTerminal = new ServerTerminal();

            m_ServerTerminal.MessageRecived += m_Terminal_MessageRecived;
            m_ServerTerminal.ClientConnect += m_Terminal_ClientConnected;
            m_ServerTerminal.ClientDisconnect += m_Terminal_ClientDisConnected;

            m_ServerTerminal.StartListen();
            Console.ReadKey();
        }

        static void m_Terminal_ClientDisConnected(Socket socket)
        {
            Console.WriteLine(string.Format("Client {0} has been disconnected!", socket.LocalEndPoint));
        }

        static void m_Terminal_ClientConnected(Socket socket)
        {
            Console.WriteLine(string.Format("Client {0} has been connected!", socket.LocalEndPoint));
        }

        static void m_Terminal_MessageRecived(Socket socket, byte[] buffer)
        {
            string message = ConvertBytesToString(buffer, buffer.Length);
            if(message!="")
            {
                Packet GetPacket = new Packet();
                GetPacket = JsonConvert.DeserializeObject<Packet>(message);
                switch(GetPacket.TypePacket)
                {
                    case 1: { RegisterData(GetPacket.PacketMass); break; }
                    case 2:
                        {
                            byte[] data = Encoding.ASCII.GetBytes(LoginUser(GetPacket.PacketMass));
                            socket.Send(data);
                            break;
                        }
                    case 3:
                        {
                            string ButeStr=Encoding.ASCII.GetString(buffer);
                            m_ServerTerminal.DistributeMessage(Encoding.ASCII.GetBytes(ProfileInfo(GetPacket.PacketMass,ButeStr)));
                            break;
                        }
                    case 4:
                        {
                            //socket.Send(Encoding.ASCII.GetBytes(ProfileInfo(GetPacket.PacketMass)));
                            break;
                        }
                }
            }
            // Send Echo
            
        }

        static private string ConvertBytesToString(byte[] bytes, int iRx)
        {
            char[] chars = new char[iRx + 1];
            System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
            d.GetChars(bytes, 0, iRx, chars, 0);
            string szData = new string(chars);

            return szData;
        }


        static void RegisterData(string Value)
        {
            char[] Tert = { '&' };

            string[] RegisterString = Value.Split(Tert[0]);
            Profile client = new Profile();
            var rep = kernel.Get<IRepositoryBase<Profile>>();

            client.Login = RegisterString[0];
            client.Password = RegisterString[1];
            client.Email = RegisterString[2];
            client.Name = RegisterString[3];
            client.Surname = RegisterString[4];
            client.Age = RegisterString[5];
            client.Gender = RegisterString[6];

            //Добавить IMG
            rep.AddItem(client);
            rep.SaveChanges();


        }

        static string LoginUser(string Value)
        {
            char separator = '&';
            string[] LoginData = Value.Split(separator);

            var rep = kernel.Get<IRepositoryBase<Profile>>();
            IEnumerable<Profile> User = rep.AllItems;
            User = from U in User
                   where U.Login == LoginData[0] && U.Password == LoginData[1]
                   select U;

            if (User.Count() > 0)

                return "1";
            else
                return "2";


        }
        static string ProfileInfo(string Value,string Telephone)
        {
            try
            {
                string str="";
                Value = Value.Replace(',', ':');
                string[] StrArrey = Value.Split(':');

                StrArrey[0] = StrArrey[0].Remove(StrArrey[0].IndexOf(' '));

                StrArrey[2] = StrArrey[2].Remove(0, 1);
                StrArrey[4] = StrArrey[4].Remove(0, 1);

                var rep = kernel.Get<IRepositoryBase<Profile>>();
                IEnumerable<Profile> User = rep.AllItems;
                User = from U in User
                       where U.Login == StrArrey[0]
                       select U;

             if (User.Count() > 0)
            foreach(var u in User)
            {
                str= u.Login+"&"+u.Name+"&"+u.Surname+"&"+u.Age+"&"+u.Email+"&"+u.Gender+"&";
                break;
            }
             str += Telephone;
           string obj=str;
           Console.WriteLine(str);

            return obj;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                return "0";
            }
        }
    }
}
