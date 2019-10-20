using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using SC_Common;
using SC_Common.Enum;

//debug
using System.Windows.Controls;
using System.Windows.Threading;

namespace SC_Client.src
{
    internal sealed class Client
    {      
        //DEBUG
        public TextBox debugBox;

        private int ServerPort;
        private IPAddress ServerIP;
        private IPEndPoint ServerEndPoint;
        private Socket Handler;
        private PackageManager PackageManag;

        public Client()
        {
            PackageManag = new PackageManager();
        }

        public void Connect()
        {
            ServerIP = IPAddress.Parse("192.168.100.5");
            ServerPort = 25252;

            ServerEndPoint = new IPEndPoint(ServerIP, ServerPort);
            Handler = new Socket(ServerIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Handler.Connect(ServerEndPoint);
            PackageArgs package = new PackageArgs()
            {
                PackageType = PackageType.Command,
                Command = Command.User_Setup,
                Arguments = new Dictionary<Argument, object>()
                {
                    { Argument.UserID, 2 },
                    { Argument.Value, 2 }
                }
            };

            PackageManag.SendPackage(Handler, package, SendCallback);
        }

        public void Disconnect()
        {
            PackageArgs package = new PackageArgs()
            {
                PackageType = PackageType.Command,
                Command = Command.Exit,
                Arguments = null
            };

            PackageManag.SendPackage(Handler, package, SendCallback);         
        }

        private void HasDisconnected()
        {
            Handler.Shutdown(SocketShutdown.Both);
            Handler.Close();
        }

        public void SendTestData()
        {
            PackageArgs package = new PackageArgs()
            {
                PackageType = PackageType.Command,
                Command = Command.User_Setup,
                Arguments = new Dictionary<Argument, object>()
                {
                    { Argument.UserID, 2 },
                    { Argument.Value, 2 }
                }
            };

            PackageManag.SendPackage(Handler, package, SendCallback);   
        }

        private void SendCallback(Socket handler, int packageHash, bool status)
        {

        }          
    }
}
