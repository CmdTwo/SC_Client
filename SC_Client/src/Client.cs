using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using SC_Common;
using SC_Common.Enum;

namespace SC_Client.src
{
    internal sealed class Client
    {
        private int ServerPort;
        private IPAddress ServerIP;
        private IPEndPoint ServerEndPoint;
        private Socket Handler;
        private PackageManager PackageManag;

        public event Action<bool> ConnectionStatusHasChange;
        public event Action HasAdmitted;
        public event Action<string, string> NewMessage;
        public event Action<string, bool> UserConnectionAction; //TEMP

        public int LocalID { get; private set; }
        public string LocalNickname { get; private set; }

        public Client()
        {
            PackageManag = new PackageManager();
        }

        public void Connect(string ip, int port)
        {
            ServerIP = IPAddress.Parse(ip);
            ServerPort = port;

            ServerEndPoint = new IPEndPoint(ServerIP, ServerPort);
            Handler = new Socket(ServerIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            bool status = false;

            try
            {
                Handler.Connect(ServerEndPoint);
                status = true;
            }
            catch (SocketException)
            {
                status = false;
                return;
            }
            finally
            {
                ConnectionStatusHasChange?.Invoke(status);
            }

            PackageManag.ReceivePackage(Handler, ReceiveCallback);
        }

        public void Disconnect()
        {
            PackageArgs package = new PackageArgs()
            {
                PackageType = PackageType.Command,
                Command = Command.Exit,
                Arguments = null
            };
            PackageManag.SendPackage(Handler, package, DisconnectHandler);         
        }

        private void ReceiveCallback(Socket user, PackageArgs package)
        {
            if (package.PackageType == PackageType.Event)
            {
                switch (package.Event)
                {
                    case (Event.New_Message): NewMessageEventHandler(package); break;
                    case (Event.User_Left): UserConnectionAction(package.Arguments[Argument.Nickname] as string, false); break; //TEMP
                    case (Event.New_User): UserConnectionAction(package.Arguments[Argument.Nickname] as string, true); break; //TEMP
                }
            } else {
                switch (package.Command)
                {
                    case (Command.User_Setup): SetupResponse(package); break;
                    default: break;
                }
            }

            PackageManag.ReceivePackage(Handler, ReceiveCallback);
        }

        #region Event

        private void NewMessageEventHandler(PackageArgs package)
        {
            NewMessage?.Invoke(
                package.Arguments[Argument.Message] as string,
                package.Arguments[Argument.Nickname] as string
                );
        }

        #endregion

        #region Response

        private void SetupResponse(PackageArgs package)
        {
            LocalID = (int)package.Arguments[Argument.UserID];
            LocalNickname = package.Arguments[Argument.Nickname] as string;

            HasAdmitted?.Invoke();
        }


        #endregion

        #region Callback

        private void DisconnectHandler(Socket handler, int packageHash, bool status)
        {
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }

        #endregion

        #region API

        public void SetNickAndContinue(string nickName)
        {
            PackageArgs package = new PackageArgs()
            {
                PackageType = PackageType.Command,
                Command = Command.User_Setup,
                Arguments = new Dictionary<Argument, object>
                {
                    { Argument.Nickname, nickName }
                }
            };
            PackageManag.SendPackage(Handler, package, null);
        }

        public void SendMessage(string message)
        {
            PackageArgs package = new PackageArgs()
            {
                PackageType = PackageType.Command,
                Command = Command.Send_Message,
                Arguments = new Dictionary<Argument, object>
                {
                    { Argument.Message, message },
                    { Argument.Nickname, LocalNickname }
                }
            };
            PackageManag.SendPackage(Handler, package, null);
        }

        #endregion
    }
}
