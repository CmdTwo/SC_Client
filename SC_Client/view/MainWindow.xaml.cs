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

namespace SC_Client.view
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal Client LocalClient;
        private string LastFrom; //TEMP

        internal MainWindow(Client client)
        {
            InitializeComponent();
            LocalClient = client;

            LocalClient.NewMessage += LocalClient_NewMessage;
            LocalClient.UserConnectionAction += LocalClient_UserConnectionAction;

            LocalClient.GetLastMessages(AddLastMessages);
        }

        private void LocalClient_UserConnectionAction(string user, bool action)
        {
            MessageContainer.Dispatcher.Invoke(() =>
            {
                EventAlert newMessage = new EventAlert(user, action);
                MessageContainer.Children.Add(newMessage);
                LastFrom = string.Empty;
            });
        }

        private void LocalClient_NewMessage(string text, string from)
        {
            MessageContainer.Dispatcher.Invoke(() =>
            {
                MessageReceived newMessage = null;

                if (LastFrom == from)
                    newMessage = new MessageReceived(text);
                else
                    newMessage = new MessageReceived(text, from);

                MessageContainer.Children.Add(newMessage);
                LastFrom = from;
            });            
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            MessageSent newMessage = null;

            if (LastFrom == LocalClient.LocalNickname)
                newMessage = new MessageSent(MessageInput.Text);
            else
                newMessage = new MessageSent(MessageInput.Text, LocalClient.LocalNickname);

            LastFrom = LocalClient.LocalNickname;
            MessageContainer.Children.Add(newMessage);

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
                    LocalClient_NewMessage(userMsg.Content, userMsg.UserName);
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            LocalClient.Disconnect();
        }
    }
}
