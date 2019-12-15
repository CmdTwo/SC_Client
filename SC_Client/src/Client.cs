using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using SC_Common;
using SC_Common.Enum;
using SC_Common.Messages;

namespace SC_Client.src
{
    public sealed class Client
    {
        private int ServerPort;
        private IPAddress ServerIP;
        private IPEndPoint ServerEndPoint;
        private Socket Handler;
        private PackageManager PackageManag;

        public event Action<bool> ConnectionStatusHasChange;
        public event Action<bool, string> HasAdmitted;
        public event Action<string, string, bool> NewMessage;
        public event Action<string, bool> UserConnectionAction; //TEMP
        public event Action<string> AuthCommandHasResponsed;
        public event Action<string> HasBanned;
        public event Action<string> HasKicked;
        public event Action<bool> CheckBlockIPResponsed;

        private Action<List<ChatMessage>> OutHandler;

        public bool IsConnected { get
            {
                if (Handler == null) return false;
                return Handler.Connected;
            }
        }

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
                    case (Event.User_Left): UserConnectionAction(
                        package.Arguments[Argument.UserName] as string, false); break; //TEMP
                    case (Event.New_User): UserConnectionAction(
                        package.Arguments[Argument.UserName] as string, true); break; //TEMP
                    case (Event.New_PM): NewMessageEventHandler(package); break;
                    case (Event.HasBanned): HasBanned?.Invoke(package.Arguments
                        [Argument.Message] as string); break;
                    case (Event.HasKicked): HasKicked?.Invoke(package.Arguments
                        [Argument.Message] as string); break;
                }
            } else {
                switch (package.Command)
                {
                    case (Command.User_Setup): SetupResponse(package); break;
                    case (Command.Get_Last_Messages): OutHandler?.Invoke
                            (package.Arguments[Argument.MessageList] as List<ChatMessage>); break;
                    case (Command.CommandResponse): AuthCommandHasResponsed?.Invoke
                            (package.Arguments[Argument.Message] as string);; break;
                    case (Command.CheckBlockIP): CheckBlockIPResponsed?.Invoke
                            ((bool)(package.Arguments[Argument.IsAdmitted])); break;
                    default: break;
                }
            }

            if(IsConnected)
                PackageManag.ReceivePackage(Handler, ReceiveCallback);
        }

        #region Event

        private void NewMessageEventHandler(PackageArgs package)
        {
            NewMessage?.Invoke(
                package.Arguments[Argument.Message] as string,
                package.Arguments[Argument.UserName] as string,
                (bool)package.Arguments[Argument.IsPM]
                );
        }

        #endregion

        #region Response

        private void SetupResponse(PackageArgs package)
        {
            bool result = (bool)package.Arguments[Argument.IsAdmitted];
            string message = null;
            if (result)
            {
                LocalID = (int)package.Arguments[Argument.UserID];
                LocalNickname = package.Arguments[Argument.UserName] as string;
            }
            else
            {
                message = package.Arguments[Argument.Message] as string;
            }

            HasAdmitted?.Invoke(result, message);
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

        public string GetIP()
        {
            return (Handler.LocalEndPoint as IPEndPoint).Address.ToString();
        }

        public void SetNickAndContinue(string nickName)
        {
            PackageArgs package = new PackageArgs()
            {
                PackageType = PackageType.Command,
                Command = Command.User_Setup,
                Arguments = new Dictionary<Argument, object>
                {
                    { Argument.UserName, nickName }
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
                    { Argument.MessageObj, new UserMessage(LocalID, LocalNickname, message) }
                }
            };
            PackageManag.SendPackage(Handler, package, null);
        }

        public void GetLastMessages(Action<List<ChatMessage>> handler)
        {
            OutHandler = handler;
            PackageArgs package = new PackageArgs()
            {
                PackageType = PackageType.Command,
                Command = Command.Get_Last_Messages
            };
            PackageManag.SendPackage(Handler, package, null);
        }

        public void CheckIP()
        {
            string ip = GetIP();
            PackageArgs package = new PackageArgs()
            {
                PackageType = PackageType.Command,
                Command = Command.CheckBlockIP,
                Arguments = new Dictionary<Argument, object>
                {
                    { Argument.IP, ip }
                }
            };
            PackageManag.SendPackage(Handler, package, null);
        }

        #endregion
    }
}
