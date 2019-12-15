using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using SC_Client.src;

namespace SC_Client.view
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class SignInWindow : Window
    {
        private Client ClientObj;
        private string ServerIP = "192.168.100.4";
        private int ServerPort = 25252;

        public SignInWindow()
        {
            MessageBox.Show("test", "HEADER", MessageBoxButton.OK);
            SC_Common.Register.IsActive = false;

            InitializeComponent();
            Closing += SignInWindow_Closing;

            ClientObj = new Client();
            ClientObj.ConnectionStatusHasChange += ClientObj_ConnectionStatusHasChange;
            ClientObj.HasAdmitted += ClientObj_HasAdmitted;

            ClientObj.Connect(ServerIP, ServerPort);
        }
     
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            //TEMP
            if (ServerAddressInput.Text.Length >= 1)
            {
                int sepIndex = ServerAddressInput.Text.IndexOf(':');
                ClientObj.Connect(ServerAddressInput.Text.Substring(0, sepIndex),
                    int.Parse(ServerAddressInput.Text.Substring(sepIndex + 1)));
            }
        }

        private void SetupButton_Click(object sender, RoutedEventArgs e)
        {
            //TEMP
            if (NicknameInput.Text.Length >= 1)
            {
                ClientObj.SetNickAndContinue(NicknameInput.Text);
            }
        }

        #region Handlers

        private void ClientObj_ConnectionStatusHasChange(bool status)
        {
            if (status)
            {
                ConnectStatus.Visibility = Visibility.Hidden;
                ConnectPanel.Visibility = Visibility.Collapsed;
                SignInPanel.Visibility = Visibility.Visible;
            }
            else
            {
                ConnectStatus.Visibility = Visibility.Visible;
                ConnectPanel.Visibility = Visibility.Visible;
            }
        }

        private void ClientObj_HasAdmitted()
        {
            ResultStatus.Dispatcher.Invoke(() => 
            {
                ResultStatus.Visibility = Visibility.Visible;
                MainWindow main = new MainWindow(ClientObj);
                main.Show();
                this.Close();
            });            
        }


        private void SignInWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ClientObj.ConnectionStatusHasChange -= ClientObj_ConnectionStatusHasChange;
            ClientObj.HasAdmitted -= ClientObj_HasAdmitted;
        }

        #endregion

        private void NicknameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnterButton.IsEnabled = NicknameInput.Text.Length != 0;
        }
    }
}
