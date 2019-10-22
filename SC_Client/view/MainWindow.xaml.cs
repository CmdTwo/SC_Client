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

namespace SC_Client.view
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal Client LocalClient;
        private UserControl LastMessage;

        internal MainWindow(Client client)
        {
            InitializeComponent();
            LocalClient = client;

            LocalClient.NewMessage += LocalClient_NewMessage;
            LocalClient.UserConnectionAction += LocalClient_UserHasLeft;

        }

        private void LocalClient_UserHasLeft(string from, bool status)
        {
            MessageContainer.Dispatcher.Invoke(() =>
            {
                EventAlert newMessage = new EventAlert(from, status);
                MessageContainer.Children.Add(newMessage);
                LastMessage = newMessage;
            });
        }

        private void LocalClient_NewMessage(string text, string from)
        {
            MessageContainer.Dispatcher.Invoke(() =>
            {
                MessageReceived newMessage = null;

                if (LastMessage is MessageReceived)
                    newMessage = new MessageReceived(text);
                else
                    newMessage = new MessageReceived(text, from);

                MessageContainer.Children.Add(newMessage);
                LastMessage = newMessage;
            });            
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            MessageSent newMessage = null;

            if(LastMessage is MessageSent)
                newMessage = new MessageSent(MessageInput.Text);
            else
                newMessage = new MessageSent(MessageInput.Text, LocalClient.LocalNickname);

            LastMessage = newMessage;
            MessageContainer.Children.Add(newMessage);

            LocalClient.SendMessage(MessageInput.Text);
            MessageInput.Text = string.Empty;
        }
    }
}
