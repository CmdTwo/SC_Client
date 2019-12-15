using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using SC_Client.src;
using SC_Client.control;
using SC_Common.Messages;
using SC_Common;

namespace SC_Client.view
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal Client LocalClient;
        private string LastFrom; //TEMP
        private Dictionary<string, OnlineUser> OnlineUserDict;

        internal MainWindow(Client client)
        {
            InitializeComponent();
            OnlineUserDict = new Dictionary<string, OnlineUser>();
            LocalClient = client;

            LocalClient.NewMessage += LocalClient_NewMessage;
            LocalClient.UserConnectionAction += LocalClient_UserConnectionAction;
            LocalClient.AuthCommandHasResponsed += LocalClient_AuthCommandHasResponsed;
            LocalClient.HasBanned += LocalClient_HasBanned;
            LocalClient.HasKicked += LocalClient_HasKicked;

            LocalClient.GetLastMessages(AddLastMessages);
        }

        private void LocalClient_HasKicked(string reason)
        {
            this.Dispatcher.Invoke(() =>
            {
                Alert alertWin = new Alert("You have been " +
                    "kicked from the server.\nReason: " + reason);
                alertWin.ShowDialog();
                this.Close();
            });
        }

        private void LocalClient_HasBanned(string reason)
        {
            this.Dispatcher.Invoke(() =>
            {           
                Alert alertWin = new Alert("You have been " +
                    "banned on the server.\nReason: " + reason);
                alertWin.ShowDialog();
                this.Close();
            });
        }

        private void LocalClient_AuthCommandHasResponsed(string text)
        {
            MessageContainer.Dispatcher.Invoke(() =>
            {
                MessageContainer.Children.Add(new AuthMeesageReceived(text));
            });
        }

        private void LocalClient_UserConnectionAction(string user, bool action)
        {
            MessageContainer.Dispatcher.Invoke(() =>
            {
                EventAlert newMessage = new EventAlert(user, action);
                MessageContainer.Children.Add(newMessage);
                LastFrom = string.Empty;
                if (action)
                {
                    OnlineUser onlineUser = new OnlineUser(user, user == LocalClient.LocalNickname);
                    OnlineUserDict.Add(user, onlineUser);
                    UserBank.Children.Add(onlineUser);
                }
                else if(OnlineUserDict.ContainsKey(user))
                {
                    UserBank.Children.Remove(OnlineUserDict[user]);
                    OnlineUserDict.Remove(user);
                }
            });
        }

        private void LocalClient_NewMessage(string text, string from, bool isPM)
        {
            MessageContainer.Dispatcher.Invoke(() =>
            {
                Control chatMessage = null;
                string lastFrom = (LastFrom == from) ? null : from;

                if (!isPM)
                    chatMessage = new MessageReceived(text, lastFrom);
                else
                    chatMessage = new PMReceived(text, lastFrom);

                MessageContainer.Children.Add(chatMessage);
                LastFrom = from;
            });            
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            MessageSent newMessage = null;

            if ((MessageInput.Text.Substring(0, 3) == ChatCommandHelper.AvailableCommand) ||
                MessageInput.Text[0] != ChatCommandHelper.SymbolOfCommand)
            {
                if (LastFrom == LocalClient.LocalNickname)
                    newMessage = new MessageSent(MessageInput.Text);
                else
                    newMessage = new MessageSent(MessageInput.Text, LocalClient.LocalNickname);

                LastFrom = LocalClient.LocalNickname;
                MessageContainer.Children.Add(newMessage);
            }

            LocalClient.SendMessage(MessageInput.Text);
            MessageInput.Text = string.Empty;
        }

        private void AddLastMessages(List<ChatMessage> messages)
        {
            foreach (ChatMessage message in messages)
            {
                if(message is SystemEvent)
                {
                    SystemEvent msgEvet = (SystemEvent)message;
                    LocalClient_UserConnectionAction(msgEvet.Content, msgEvet.Mode);
                }
                else if(message is UserMessage)
                {
                    UserMessage userMsg = (UserMessage)message;
                    LocalClient_NewMessage(userMsg.Content, userMsg.UserName, false);
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ExitFromChat();
        }

        private void ExitFromChat()
        {
            LocalClient.NewMessage -= LocalClient_NewMessage;
            LocalClient.UserConnectionAction -= LocalClient_UserConnectionAction;
            LocalClient.AuthCommandHasResponsed -= LocalClient_AuthCommandHasResponsed;
            LocalClient.HasBanned -= LocalClient_HasBanned;
            LocalClient.HasKicked -= LocalClient_HasKicked;
            LocalClient.Disconnect();
        }
    }
}
